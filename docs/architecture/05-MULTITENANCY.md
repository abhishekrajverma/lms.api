# 05 — Multi-Tenant Design

## 14. Multi-Tenant Design

### 14.1 Tenancy Model: Shared DB, Shared Schema, Discriminator (`TenantId`)

| Choice | Decision | Rationale |
|--------|----------|-----------|
| DB-per-tenant | ✗ | Operational nightmare at 10K tenants (10K DBs to backup/migrate). |
| Schema-per-tenant | ✗ | Same problem in a smaller form; SQL Server schema registry not designed for it. |
| **Shared DB, Shared Schema, `TenantId` discriminator** | ✓ | Single backup/migration surface; horizontal sharding by `TenantId` later if needed. |

Tenant isolation is enforced by a **defense-in-depth** stack:
1. **Tenant Resolution Middleware** at the edge.
2. **Tenant Context** carried through `IHttpContextAccessor` and async-local for background work.
3. **EF Core Global Query Filters** automatically inject `TenantId = @CurrentTenantId`.
4. **EF Core SaveChanges Interceptor** auto-stamps `TenantId` on inserts and refuses updates that change it.
5. **SQL Server Row-Level Security (RLS)** as a final backstop (DB-level filter predicate).
6. **Tenant-scoped composite indexes** force seeks to be tenant-leading.
7. **Tenant-aware caching** with namespaced keys: `t:{TenantId}:...`.

### 14.2 Tenant Resolution Pipeline

```
┌──────────────────────────────────────────────────────────────────────────┐
│  HTTP request                                                             │
│  Host: campus.acmeschools.com                                             │
│  Header: X-Tenant-Code: acme-edu (optional)                               │
│  Authorization: Bearer <JWT with tid claim>                               │
└────────────────────────────────────────┬─────────────────────────────────┘
                                         │
                                         ▼
                ┌────────────────────────────────────────────┐
                │     TenantResolutionMiddleware             │
                │  Order:                                    │
                │   1. Claim resolver  (jwt.tid)             │
                │   2. Header resolver (X-Tenant-Code)       │
                │   3. Host  resolver  (host → TenantDomain) │
                │   4. Path  resolver  (/t/{slug}/...)       │
                │  First match wins.                         │
                └─────────────────────┬──────────────────────┘
                                      │
                                      ▼
                ┌────────────────────────────────────────────┐
                │    ITenantStore.GetByCodeAsync(code)       │
                │  Hits Redis (5-min TTL), falls back to DB. │
                └─────────────────────┬──────────────────────┘
                                      │
                                      ▼
                ┌────────────────────────────────────────────┐
                │     ITenantContext is bound to request     │
                │  TenantId, Code, Status, PlanId, Locale    │
                └─────────────────────┬──────────────────────┘
                                      │
                                      ▼
                ┌────────────────────────────────────────────┐
                │  If status != Active → 403 TenantSuspended │
                │  Else continue                              │
                └────────────────────────────────────────────┘
```

### 14.3 Code: ITenantContext + Resolvers

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string Code { get; }
    TenantStatus Status { get; }
    Guid? PlanId { get; }
    string Locale { get; }
    string TimeZone { get; }
    string Currency { get; }
    bool IsResolved { get; }
}

public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; init; }
    public string Code { get; init; } = string.Empty;
    public TenantStatus Status { get; init; }
    public Guid? PlanId { get; init; }
    public string Locale { get; init; } = "en-US";
    public string TimeZone { get; init; } = "UTC";
    public string Currency { get; init; } = "USD";
    public bool IsResolved => TenantId != Guid.Empty;
}

public interface ITenantResolver
{
    Task<TenantContext?> ResolveAsync(HttpContext ctx, CancellationToken ct);
    int Order { get; }
}

public sealed class JwtClaimTenantResolver(ITenantStore store) : ITenantResolver
{
    public int Order => 0;
    public async Task<TenantContext?> ResolveAsync(HttpContext ctx, CancellationToken ct)
    {
        var tid = ctx.User.FindFirst("tid")?.Value;
        return Guid.TryParse(tid, out var id)
            ? await store.GetByIdAsync(id, ct)
            : null;
    }
}

public sealed class HostHeaderTenantResolver(ITenantStore store) : ITenantResolver
{
    public int Order => 2;
    public async Task<TenantContext?> ResolveAsync(HttpContext ctx, CancellationToken ct)
    {
        var host = ctx.Request.Host.Host;
        return await store.GetByHostAsync(host, ct);
    }
}

public sealed class TenantResolutionMiddleware(
    IEnumerable<ITenantResolver> resolvers,
    ILogger<TenantResolutionMiddleware> log)
{
    private readonly ITenantResolver[] _ordered = [.. resolvers.OrderBy(r => r.Order)];

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next, ITenantContextAccessor acc, CancellationToken ct)
    {
        if (IsAnonymousEndpoint(ctx)) { await next(ctx); return; }

        TenantContext? tenant = null;
        foreach (var r in _ordered)
        {
            tenant = await r.ResolveAsync(ctx, ct);
            if (tenant is { IsResolved: true }) break;
        }

        if (tenant is null || !tenant.IsResolved)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(new { error = "tenant_not_resolved" }, ct);
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsJsonAsync(new { error = $"tenant_{tenant.Status.ToString().ToLowerInvariant()}" }, ct);
            return;
        }

        acc.Set(tenant);
        using var scope = log.BeginScope(new Dictionary<string, object>
        {
            ["TenantId"] = tenant.TenantId,
            ["TenantCode"] = tenant.Code
        });
        await next(ctx);
    }
}
```

### 14.4 ITenantContextAccessor — flowing tenant context everywhere

```csharp
public interface ITenantContextAccessor
{
    ITenantContext? Current { get; }
    void Set(ITenantContext ctx);
}

public sealed class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder?> _current = new();
    public ITenantContext? Current => _current.Value?.Context;
    public void Set(ITenantContext ctx) => _current.Value = new TenantContextHolder(ctx);
    private sealed record TenantContextHolder(ITenantContext Context);
}
```

> Background workers (RabbitMQ consumers, Hangfire jobs) **read TenantId from the message envelope / job parameters** and call `accessor.Set(...)` at the very start of processing. There is no global "default" tenant.

### 14.5 EF Core Base Entity + Interfaces

```csharp
public interface ITenantOwned       { Guid TenantId { get; } }
public interface ISoftDeletable     { bool IsDeleted { get; } DateTime? DeletedAt { get; } }
public interface IAuditable
{
    DateTime CreatedAt { get; }
    Guid? CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
}
public interface IConcurrencyAware  { byte[] RowVersion { get; } }

public abstract class TenantEntity<TId> : Entity<TId>,
    ITenantOwned, ISoftDeletable, IAuditable, IConcurrencyAware
{
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;

    internal void SetTenant(Guid tenantId) => TenantId = tenantId;
    internal void SoftDelete(Guid? userId, DateTime now)
    {
        IsDeleted = true; DeletedAt = now; DeletedBy = userId;
    }
}
```

### 14.6 EF Core Global Query Filters

```csharp
public abstract class TenantDbContext(
    DbContextOptions options,
    ITenantContextAccessor accessor,
    ICurrentUser user,
    TimeProvider clock) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder mb)
    {
        foreach (var e in mb.Model.GetEntityTypes())
        {
            if (typeof(ITenantOwned).IsAssignableFrom(e.ClrType) &&
                typeof(ISoftDeletable).IsAssignableFrom(e.ClrType))
            {
                var p = Expression.Parameter(e.ClrType, "x");
                var tidProp   = Expression.Property(p, nameof(ITenantOwned.TenantId));
                var delProp   = Expression.Property(p, nameof(ISoftDeletable.IsDeleted));
                var tidConst  = Expression.Constant(accessor.Current?.TenantId ?? Guid.Empty);
                var falseExpr = Expression.Constant(false);
                var body = Expression.AndAlso(
                    Expression.Equal(tidProp, tidConst),
                    Expression.Equal(delProp, falseExpr));
                mb.Entity(e.ClrType).HasQueryFilter(Expression.Lambda(body, p));
            }
            // RowVersion concurrency token
            if (typeof(IConcurrencyAware).IsAssignableFrom(e.ClrType))
                mb.Entity(e.ClrType).Property("RowVersion").IsRowVersion();
        }
    }
}
```

> **Important:** the `tidConst` is captured at the point the model is built. Therefore the **DbContext is registered as Scoped** so that each request resolves a fresh `ITenantContextAccessor` and rebuilds the query model only once per scope. If you need the tenant id to be re-evaluated dynamically, use `accessor.Current!.TenantId` inside the lambda body without `Expression.Constant` — but that requires `EF.Property` and `IModelCacheKeyFactory` overrides; the canonical solution is to keep DbContext scoped to one tenant per request.

### 14.7 SaveChanges Interceptor (auto-stamping)

```csharp
public sealed class TenantAuditInterceptor(
    ITenantContextAccessor accessor,
    ICurrentUser user,
    TimeProvider clock) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData ed, InterceptionResult<int> r, CancellationToken ct = default)
    {
        var tid = accessor.Current?.TenantId ?? Guid.Empty;
        var uid = user.UserId;
        var now = clock.GetUtcNow().UtcDateTime;
        var ctx = ed.Context!;

        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is ITenantOwned to && entry.Property("TenantId").CurrentValue is Guid g && g == Guid.Empty)
                        entry.Property("TenantId").CurrentValue = tid;
                    StampAuditCreate(entry, uid, now);
                    break;
                case EntityState.Modified:
                    if (entry.Entity is ITenantOwned)
                        entry.Property("TenantId").IsModified = false; // immutable
                    StampAuditUpdate(entry, uid, now);
                    break;
                case EntityState.Deleted:
                    if (entry.Entity is ISoftDeletable)
                    {
                        entry.State = EntityState.Modified;
                        entry.Property("IsDeleted").CurrentValue = true;
                        entry.Property("DeletedAt").CurrentValue = now;
                        entry.Property("DeletedBy").CurrentValue = uid;
                    }
                    break;
            }
        }
        return base.SavingChangesAsync(ed, r, ct);
    }
}
```

### 14.8 Row-Level Security (DB-side backstop)

```sql
-- Session context flows tenant id from the app pool to SQL.
CREATE FUNCTION security.fn_TenantPredicate(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE WITH SCHEMABINDING
AS RETURN SELECT 1 AS Allowed
   WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER)
      OR IS_ROLEMEMBER('db_tenant_admin') = 1;  -- platform ops bypass

CREATE SECURITY POLICY security.TenantSecurityPolicy
    ADD FILTER PREDICATE security.fn_TenantPredicate(TenantId) ON student.Students,
    ADD BLOCK  PREDICATE security.fn_TenantPredicate(TenantId) ON student.Students
        AFTER INSERT, AFTER UPDATE,
    ADD FILTER PREDICATE security.fn_TenantPredicate(TenantId) ON fees.FeeInvoices,
    -- ... repeat for every tenant-scoped table
WITH (STATE = ON);
```

In ASP.NET Core, set the session context once per connection:

```csharp
public sealed class TenantSessionContextInterceptor(ITenantContextAccessor a) : DbConnectionInterceptor
{
    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection cn, ConnectionEventData d, InterceptionResult r, CancellationToken ct = default)
    {
        await base.ConnectionOpeningAsync(cn, d, r, ct);
        return r;
    }
    public override async Task ConnectionOpenedAsync(
        DbConnection cn, ConnectionEndEventData d, CancellationToken ct = default)
    {
        await using var cmd = cn.CreateCommand();
        cmd.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@tid;";
        var p = cmd.CreateParameter(); p.ParameterName = "@tid"; p.Value = a.Current?.TenantId ?? Guid.Empty;
        cmd.Parameters.Add(p);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
```

> RLS is a **belt and suspenders**. The application's query filter and stamp interceptor are sufficient in normal operation; RLS catches mistakes (e.g., raw SQL queries that forgot the tenant predicate).

### 14.9 Tenant-Aware Caching

```csharp
public sealed class TenantedCacheKey
{
    public static string For(ITenantContext ctx, string ns, params object[] parts) =>
        $"t:{ctx.TenantId:N}:{ns}:{string.Join(':', parts)}";
}

// Example
var key = TenantedCacheKey.For(_tenant, "students", "section", sectionId);
var list = await _cache.GetOrSetAsync(key, () => Repo.ListAsync(...), TimeSpan.FromMinutes(2));
```

Redis keyspace is partitioned per tenant; a tenant deprovisioning job runs `UNLINK` against the prefix `t:{TenantId:N}:*` (using `SCAN` to avoid blocking).

### 14.10 Tenant-Aware Logging & Metrics

`Serilog` enriches every log event with `TenantId` and `TenantCode`:

```csharp
public sealed class TenantEnricher(ITenantContextAccessor a) : ILogEventEnricher
{
    public void Enrich(LogEvent e, ILogEventPropertyFactory f)
    {
        var t = a.Current;
        if (t is null) return;
        e.AddOrUpdateProperty(f.CreateProperty("TenantId", t.TenantId));
        e.AddOrUpdateProperty(f.CreateProperty("TenantCode", t.Code));
    }
}
```

`OpenTelemetry` adds the same as activity tags so distributed traces and metrics are filterable by tenant in Grafana/Tempo.

### 14.11 Tenant Lifecycle State Machine

```
   [Pending] ──provision─▶ [Active] ──suspend─▶ [Suspended] ──resume──▶ [Active]
                                  └─deprovision─▶ [Deprovisioned] (terminal)
```

| Transition | Trigger | Side effects |
|------------|---------|--------------|
| Pending → Active | Onboarding workflow completes | Seed default roles, default AY, default plans, default email/sms config (sandbox), publish `TenantProvisionedEvent` |
| Active → Suspended | Billing past-due > 7 days OR admin action | Read-only mode (write endpoints return 423), publish `TenantSuspendedEvent` |
| Suspended → Active | Payment received OR admin action | publish `TenantReactivatedEvent` |
| Active → Deprovisioned | Customer cancellation + 90-day grace expires | Tombstone tenant, schedule data deletion (`DataRetentionWorker`), revoke API keys, publish `TenantDeprovisionedEvent` |

### 14.12 Tenant Data Export / Import (GDPR & Migration)

- **Export:** Hangfire job builds a tenant-scoped Parquet pack (one file per table, partitioned by month for big tables) and signs a 7-day URL.
- **Import:** Same shape; staging schema + cross-validation before merge into live.
- **Right to erasure:** `DataErasureWorker` updates PII columns (`Email`, `PhoneNumber`, `AadharNumberHash`, addresses) to a deterministic `[REDACTED]` while keeping referential integrity.

### 14.13 What If We Outgrow a Single Database?

- **Shard by `TenantId` modulus** across N "cells" (each cell is a SQL Server AG with ≤ 1,500 tenants).
- **TenantStore** becomes the *router* (cached in Redis): `TenantId → ConnectionString`.
- **No code changes** in modules — we already inject `IDbContextFactory<TModule DbContext>` per request, which can pick the right connection.
- **Cross-cell joins** were never allowed (we forbade them at the architecture level on day one), so the migration to sharding is purely operational.
