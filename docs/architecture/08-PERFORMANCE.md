# 08 — Performance, Caching, Normalization

## 21. Performance Architecture

### 21.1 Targets (SLOs)

| Metric | SLO |
|--------|-----|
| API p95 (read) | ≤ 200 ms |
| API p95 (write) | ≤ 400 ms |
| API p99 | ≤ 1 s |
| Login | ≤ 300 ms p95 |
| Attendance batch (50 students) | ≤ 250 ms p95 |
| Fee invoice generation (per tenant 5K students) | ≤ 5 minutes |
| Report card generation (per student) | ≤ 1 s |
| Outbox publish lag | ≤ 5 s p95 |
| Availability | 99.95 % monthly |

### 21.2 SQL Server Performance Strategy

#### 21.2.1 Topology

- **Always On Availability Group**: 1 primary + 2 synchronous replicas + 1 async DR replica.
- **Read replicas** carry read-only routing for **reports, dashboards, analytics**, isolated from OLTP.
- Buffer pool target ≥ 70 % cached working set; `MAXDOP = 4` for OLTP, `8` for reporting.
- **Read Committed Snapshot Isolation (RCSI)** enabled. **No `WITH (NOLOCK)`** anywhere.

#### 21.2.2 Partitioning

| Table | Strategy | Why |
|-------|----------|-----|
| `attendance.StudentAttendance` | Monthly RANGE on `SessionDate` | 100M+ rows; 90% of reads scope to last month |
| `fees.FeeInvoices` / `FeeInvoiceLines` | Yearly RANGE on `IssuedOn` | Bulk dunning + year-over-year reporting |
| `audit_.AuditLogs` | Monthly RANGE on `AuditDate` | Append-only; old partitions move to cold storage |
| `transport.GPSPings` | Monthly RANGE on `PingDate` | Massive write rate; 90 days hot |

Switch-out at archival is **metadata-only** (`ALTER TABLE ... SWITCH PARTITION ...`). Data-warehouse columnstore archive is a single-statement copy.

#### 21.2.3 Archiving / Retention

| Domain | Hot | Warm (read-replica) | Cold (archive) |
|--------|-----|---------------------|----------------|
| Attendance | last 12 months | up to 3 years | 7 years (columnstore parquet) |
| Audit | last 6 months | 12 months | 7 years |
| Fee invoices | current AY + last AY | 7 years | regulatory |
| GPS pings | 90 days | 1 year | none |
| Email/SMS logs | 90 days | 1 year | aggregate counts only |

#### 21.2.4 Query Optimization Practices

- **No SELECT \***. Projections via DTOs.
- **Pagination by keyset** (seek method): `WHERE (CreatedAt, Id) < (@last, @lastId) ORDER BY CreatedAt DESC, Id DESC LIMIT 50`. Stable + index-friendly.
- **Compiled queries** (`EF.CompileAsyncQuery`) for hot read paths.
- **AsNoTracking()** for queries (never ChangeTracker overhead on reads).
- **Split queries** for collection includes only when ROI is positive; otherwise default single SQL.
- **Avoid N+1**: explicit `Include` or projection.
- **`SELECT TOP (n)`** for paging always; never `OFFSET` with large skip values.
- **Batched updates** with `ExecuteUpdateAsync` / `ExecuteDeleteAsync` (EF 7+) for bulk maintenance.
- **Minimal API + endpoint filters** for the hottest endpoints (attendance mark, login, payment webhook).

#### 21.2.5 Bulk Operations

- **`SqlBulkCopy`** wrapped via `EFCore.BulkExtensions` for:
  - Bulk attendance import (CSV).
  - Bulk student import (admission).
  - Bulk fee invoice generation.
- **`MERGE`** via stored procedures for upserts (e.g., daily aggregate roll-up).
- **Memory-optimized table types** for high-frequency batched payloads (attendance fan-out).

#### 21.2.6 Connection Pooling

- ADO.NET pool min 5, max 200 per pod (tunable per env).
- **Pool reuse hint**: `Application Name=schoolerp.api;` + tenant tag in `ApplicationName` for diagnostics.
- **Open Connections per pod** dashboarded and alerted (> 80 % saturation triggers HPA).

#### 21.2.7 Locking & Blocking

- RCSI default; reads do not block writes.
- **Hot rows protected** by:
  - Per-payment idempotency unique index (no second concurrent write).
  - Optimistic concurrency via `RowVersion` (retry on conflict at handler level).
  - Sequential GUIDs to avoid hot last-page contention on insert.
- **Long-running reports** redirected to read replicas with `ApplicationIntent=ReadOnly`.

### 21.3 EF Core Performance

| Practice | Notes |
|----------|-------|
| `AsNoTracking()` for queries | Default for queries via `IQueryProvider` decorator |
| `AsSplitQuery()` only when measured | Single-query default; some report endpoints opt-in |
| Compiled queries | `static readonly Func<…> _byId = EF.CompileAsyncQuery(...)` for top 50 endpoints |
| `IQueryable` projection to DTO | Use `Select(x => new XDto { … })` to project fewer columns and avoid materializing entities |
| Batched SaveChanges | Default; we tune `MaxBatchSize` based on table |
| Disable change-detector when bulk | `ctx.ChangeTracker.AutoDetectChangesEnabled = false` then re-enable |
| Use `ExecuteUpdateAsync` for set-based updates | e.g., "mark all overdue invoices" |
| Pre-evaluate query expression once | Avoid building expression trees per request |
| Cache `IModel` by tenant filter | DbContext is scoped; one model per process; `IModelCacheKeyFactory` if dynamic |
| Disable unused features | `EnableSensitiveDataLogging=false` in prod |
| `Database.SetCommandTimeout` per use | 5s default, 30s for reports |

### 21.4 Read Optimization

- **CQRS read side**: query handlers read directly from DbContext with optimized SQL/projections; bypass domain entities.
- **Read replicas** routed via `ApplicationIntent=ReadOnly` on the read-replica connection string.
- **Materialized views** (via Hangfire roll-ups) for heavy dashboards:
  - `dashboard_AttendanceDailySummary(TenantId, Date, ClassId, SectionId, Present, Absent, Total)`
  - `dashboard_FeeCollectionDaily(TenantId, Date, Collected, Outstanding, RefundsTotal)`
- **Search** uses SQL Server **Full-Text** (or Elastic for global tenant search if scaled out).
- **Pagination contracts**: cursor-based (returns `nextCursor`), no `total` by default to avoid scans.

### 21.5 Write Optimization

- **Outbox writes** are co-located in the same transaction as business writes — always cheap.
- **Attendance fan-out** uses `SqlBulkCopy` for the 50–60 student rows of one session (single round trip).
- **Idempotent commands** so retries never double-write.
- **Async/await** end-to-end; no sync-over-async; `ConfigureAwait(false)` in libraries.

## 22. Caching Strategy

### 22.1 Cache Levels

| Level | Purpose | Tech | TTL |
|-------|---------|------|-----|
| L1 | Process-local hot lookup | `IMemoryCache` | 30–60 s |
| L2 | Cross-pod shared | **Redis** (cluster) | 1 min – 1 hour |
| L3 | CDN / browser | Cloudflare/Front Door | 5 min – 1 day |

### 22.2 Redis Cluster

- 6 nodes (3 masters, 3 replicas). RDB + AOF, mTLS, ACLs per app.
- Eviction policy `allkeys-lru` for cache DB; `noeviction` for session DB.
- Pipelining everywhere for multi-key sets.
- Pub/Sub used for **cache invalidation broadcasts** triggered by integration events.

### 22.3 Tenanted Keyspace

```
t:{TenantId:N}:tenant:meta              -> 60m (rarely changes)
t:{TenantId:N}:user:{UserId:N}:perms    -> 5m
t:{TenantId:N}:academic:current-year    -> 30m
t:{TenantId:N}:section:{SectionId:N}:roster -> 10m
t:{TenantId:N}:settings:branding        -> 60m
t:{TenantId:N}:rl:email                 -> 1m sliding window (token bucket)
sess:{SessionId}                        -> 24h sliding (user session blob)
jwt:revoked:{Jti}                       -> until exp (denylist)
```

### 22.4 What We Cache

| Domain | Items | Strategy |
|--------|-------|----------|
| Tenant resolution | TenantId by Code/Host | Read-through; busted on TenantUpdatedEvent |
| Permissions | Effective perms per (Tenant,User) | Read-through; busted on RoleAssignedEvent etc. |
| Branding/Theme | Logo URL, primary color | Read-through; busted on TenantBrandingChanged |
| Academic year | Current AY id | Read-through; busted on AY transition |
| Section roster | StudentEnrollments per Section | Read-through; busted on enrollment events |
| Class teacher | Section -> Employee | Read-through; busted on Assignment events |
| Plans/Features | Active plans + features | Read-through; busted by admin update |
| Subscription state | Tenant -> Plan, Status | Read-through; 5 min TTL safety |
| Lookup tables | FeeTypes, Subjects, Departments | 30 min TTL |
| Hot dashboards | Daily summaries | Pre-warmed by `AggregatorWorker` |

### 22.5 What We Do NOT Cache

- Anything **transactional** (invoices, payments, attendance marks) — these are read directly from DB read replicas.
- Anything **PII-sensitive** without justification (and never beyond TTL).
- Cross-tenant data — namespacing forbids accidental cross-tenant cache hits.

### 22.6 Cache Stampede Protection

- `IMemoryCache` + `SemaphoreSlim` per key for L1 (single-flight).
- **Redis** uses `SET NX EX` lock + jittered backoff; alternatively, use **stale-while-revalidate**: serve stale value while a single worker re-fills.

### 22.7 Cache Invalidation via Events

```
[handler]  → updates DB → publishes RoleAssignedEvent (outbox)
                                                │
                                                ▼
                                   ┌───────────────────────────┐
                                   │ identity.q.cache-bust     │
                                   │ CacheInvalidationWorker   │
                                   └────────────┬──────────────┘
                                                │ DEL t:{TID}:user:{UID}:perms
                                                ▼
                                          Redis cluster
```

### 22.8 HTTP Caching

- API responses for **public, immutable** assets (e.g., themes, branding logos): `Cache-Control: public, max-age=86400, immutable`, ETags.
- **Authenticated GETs**: `Cache-Control: private, max-age=60` + ETag with weak validators.
- **Idempotency keys** on POST endpoints (`Idempotency-Key` header, stored 24h).

### 22.9 Database Result Cache

- Heavy reports (annual fee collection, year-end attendance) are **materialized to a snapshot table** via Hangfire and served from there with ETag.
- A short Redis cache (10 minutes) sits in front of dashboards to absorb refresh storms after release/login.

## 22.10 Normalization Analysis

> The schema is **3NF** across all modules with a few **deliberate denormalizations** for performance, each annotated.

### 1NF — Atomicity
- Every column holds an atomic value. Multi-valued fields (e.g., parent contacts, addresses) are split into child tables (`StudentEmergencyContacts`, `StudentAddresses`).
- JSON/`NVARCHAR(MAX)` is used **only** for opaque blobs (event payloads, integration metadata) — never for queryable attributes.

### 2NF — Full Functional Dependency on the PK
- Composite PKs (e.g., `(TenantId, StudentId, AddressId)`) — every non-key attribute depends on the **whole** PK.
- Lookup attributes (e.g., `Class.Name`) are not duplicated in `Section`; instead `Section.ClassId` references `Classes`.

### 3NF — No Transitive Dependencies
- `Student` does not store `ClassName`/`SectionName`; it stores no class/section at all (history lives in `StudentEnrollments`). To get current class, join to `StudentEnrollments` for the *current* AY.
- `FeeInvoice.Total` is **computed** (`AS PERSISTED`) from columns it depends on — not a transitive copy from another table.
- `Employee.DepartmentName` is not stored; only `DepartmentId`.

### Deliberate Denormalizations (with rationale)

| Field | Where | Why | Mitigation |
|-------|-------|-----|------------|
| `IssuedOn` on `FeeInvoiceLines`, `FeePayments` | Fees | Required as partition key on the parent's partitioned PK; FKs must include partition column | Application sets it from invoice; FK enforces equality |
| `PingDate` on `GPSPings` | Transport | Computed `PERSISTED` for partitioning | Always derived |
| `SessionDate` on `StudentAttendance` | Attendance | Same partition reason | Domain rule — equals session date |
| `StockOnHand` on `InventoryItems` | Inventory | Hot-path read; recomputable from `StockTransactions` | Reconciled nightly job |
| `FullName`/`DisplayName` computed columns | Users/Students | Search/sort | `PERSISTED` and indexed |
| `AmountDue` computed column | Invoices | Hot dashboards | `PERSISTED` |
| `Net` salary computed | Payroll | Reporting | `PERSISTED` |

### Why 3NF (and not BCNF/4NF here)

- BCNF would mostly differ from 3NF in cases of multi-attribute candidate keys. We don't have nontrivial cases that break BCNF that are worth refactoring.
- 4NF concerns multi-valued dependencies; we already split them into child tables.
- We **chose** to remain pragmatically 3NF + denormalized hot fields where measured pay-off > correctness cost.

### Validation

- Normalization is enforced in code by:
  - **Aggregate boundaries** (one transaction per aggregate) → discourages accidental denormalization.
  - **CI check** runs `ef migrations script` and rejects migrations that introduce duplicate columns flagged by a custom analyzer.
  - **Periodic audit query** verifies denormalized fields against their source of truth (e.g., `SUM(StockTransactions.Quantity) == InventoryItem.StockOnHand`).
