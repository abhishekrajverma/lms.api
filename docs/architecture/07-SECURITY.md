# 07 — Security Architecture

## 20. Security Architecture

### 20.1 Security Pillars

| Pillar | Mechanism |
|--------|-----------|
| Authentication | JWT (RS256), Refresh tokens (rotated, hashed, one-shot), MFA (TOTP), passwordless via magic-link |
| Authorization | RBAC + permission-based with policy provider, claims transformation, resource-based ABAC for "owns this student" checks |
| Tenant isolation | Defense-in-depth (see `05-MULTITENANCY.md`): middleware → context → query filter → interceptor → RLS |
| Encryption at rest | TDE on SQL Server, AES-256-GCM at app for sensitive blobs, Always Encrypted with secure enclaves for PII columns |
| Encryption in transit | TLS 1.3 everywhere; mTLS between services; HSTS + cert pinning on apps |
| Secrets management | Azure Key Vault / AWS Secrets Manager via DPAPI/IDataProtector; no secrets in env files |
| PII protection | Field-level encryption (`AadharNumberHash`, `AccountNumberHash`); minimization; redaction on logs |
| Audit | Append-only `audit_.AuditLogs`, signed with daily HMAC chain |
| Threat protection | WAF (OWASP rules), bot detection, rate limiting, DDoS at edge |
| Vulnerability mgmt | Dependabot, Snyk, container scans (Trivy), SAST (CodeQL), DAST (ZAP) in CI |
| Supply chain | SBOM (CycloneDX), signed images (cosign), reproducible builds |

### 20.2 Authentication

#### 20.2.1 JWT (Access Token)

- Signed with **RS256**; private key in Key Vault, rotated quarterly with overlap window.
- Claims: `sub` (UserId), `tid` (TenantId), `tcode` (TenantCode), `email`, `roles[]`, `perms[]` (compressed permission names), `sid` (SessionId), `jti`, `iat`, `nbf`, `exp` (15 min).
- Verified at API edge with **JWKS** endpoint cache (5 min TTL).
- **Audience binding**: `aud=schoolerp.api`; refresh tokens have separate audience `schoolerp.refresh`.

#### 20.2.2 Refresh Tokens

- 256-bit cryptographically random; only the **SHA-256 hash** is stored (`RefreshTokens.TokenHash BINARY(32)` UNIQUE).
- **Rotation**: every refresh issues a new token and revokes the previous via `ReplacedByTokenId`.
- **Reuse detection**: if a revoked token is presented again, **all sessions for that user are revoked** and an alert fires (suspected token theft).
- **Sliding window**: 14 days inactivity → invalid.

#### 20.2.3 Password Policy

- Argon2id hashing (memory 64 MB, iterations 3, parallelism 4) — stored as `PasswordHash` + `PasswordSalt`.
- Minimum 12 chars, breached-password check (HIBP k-anonymity API), reuse check against last 5 (`PasswordHistory`).
- Lockout: 10 failed attempts → 15-min lock; `AccessFailedCount` resets on success.

#### 20.2.4 MFA

- TOTP (RFC 6238), backup codes (one-shot, 10 codes), WebAuthn for staff.
- MFA enforced for `Platform Admin`, `Tenant Admin`, `Finance`, `Principal` roles.

### 20.3 Authorization

#### 20.3.1 Permission Model

```
Permission (system-defined, code = "<module>.<resource>.<action>")
    e.g. "students.read", "students.write", "students.delete",
         "fees.invoice.issue", "fees.payment.refund",
         "exam.result.publish"
Role (per-tenant) -- RolePermissions --> Permission
User -- UserRoles --> Role
```

#### 20.3.2 ASP.NET Core Plumbing

```csharp
public sealed class RequirePermissionAttribute(string permission) : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) : base($"perm:{permission}") { }
}

public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string Prefix = "perm:";
    public Task<AuthorizationPolicy?> GetPolicyAsync(string name)
    {
        if (!name.StartsWith(Prefix)) return Task.FromResult<AuthorizationPolicy?>(null);
        var p = name[Prefix.Length..];
        return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(p))
            .Build());
    }
    /* default + fallback policy delegates to base */
}

public sealed class PermissionAuthorizationHandler(IPermissionResolver resolver)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, PermissionRequirement req)
    {
        var userId = ctx.User.GetUserId();
        var tenantId = ctx.User.GetTenantId();
        var perms = await resolver.GetForAsync(tenantId, userId);
        if (perms.Contains(req.Permission)) ctx.Succeed(req);
    }
}
```

`IPermissionResolver` caches in Redis (`t:{TenantId}:perms:{UserId}`, TTL 5 min). Cache busted on `RoleAssignedEvent`, `PermissionGrantedEvent`, `RoleUpdatedEvent`.

#### 20.3.3 Resource-Based Authorization

For "this teacher can only access *their* sections":

```csharp
public sealed class TeacherSectionRequirementHandler(IModuleApi<IAcademicModuleApi> academic)
    : AuthorizationHandler<TeacherSectionRequirement, Guid /*sectionId*/>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, TeacherSectionRequirement req, Guid sectionId)
    {
        var employeeId = ctx.User.GetEmployeeId();
        if (employeeId is null) return;
        if (await academic.Value.IsTeacherOfSectionAsync(employeeId.Value, sectionId))
            ctx.Succeed(req);
    }
}
```

Used as `[Authorize(Policy = "TeacherSection")]` with route binding `[FromRoute] Guid sectionId`.

### 20.4 Tenant Isolation (recap)

- See `05-MULTITENANCY.md`. Defense-in-depth.
- Cross-tenant API access is impossible: middleware would fail to resolve, JWT claim mismatch would 403, query filters would return zero rows, RLS would block writes.

### 20.5 Encryption

| Layer | What | How |
|-------|------|-----|
| At rest (DB pages) | All data | SQL Server **TDE** + customer-managed keys (CMK) in Key Vault |
| At rest (sensitive cols) | Aadhar/SSN, bank details, Account/Routing | **Always Encrypted with Secure Enclaves** (deterministic for `AadharNumberHash`, randomized for `BankAccountInfo`) |
| At rest (blobs) | Documents, photos, PDFs | AES-256-GCM with per-tenant DEK, DEK wrapped by KEK in Key Vault |
| In transit (client → API) | All HTTPS | TLS 1.3, HSTS preload, certificate transparency monitoring |
| In transit (intra-cluster) | API ↔ DB, API ↔ RMQ, API ↔ Redis | mTLS (managed identities or SPIFFE) |
| Secrets | API keys, conn strings | DPAPI / `IDataProtector` keyring rotated by app, root key in Key Vault |
| Tokens | Refresh tokens | Stored as SHA-256 hash only |

### 20.6 PII Handling

- **Discovery & classification**: tagged columns at the model level via attributes (`[Pii]`, `[PiiSensitive]`).
- **Logging redaction**: Serilog destructuring policy strips `[Pii]` properties; tests assert no PII in log fields.
- **Masking on read**: API DTOs apply masking based on caller permission (`pii.read.full` vs `pii.read.masked`).
- **Right to access / erasure**: `DataErasureWorker` rewrites PII columns to placeholders, preserves FK integrity via tombstones.

### 20.7 Audit Trail

- Two audit streams:
  1. **Business audit** in `audit_.AuditLogs`: every `Create/Update/Delete` on critical aggregates writes old/new JSON via the `AuditInterceptor` (built on top of `SaveChangesInterceptor`).
  2. **Security audit** via `audit_.LoginHistory`, `audit_.ActivityLogs`, plus immutable Seq/OTel logs.
- **Tamper-evident**: each day a `Hangfire` job computes an HMAC chain (`H_n = HMAC(K, H_{n-1} || row_hash)`) and stores in `audit_.AuditChain`. Verifiable on demand.
- **Retention**: hot 12 months, archive cold tier 7 years (regulatory).

### 20.8 Rate Limiting & Abuse Protection

- ASP.NET Core fixed-window + token-bucket via `Microsoft.AspNetCore.RateLimiting`:
  - Anonymous: 60 req/min/IP.
  - Authenticated: 600 req/min/User, 6000 req/min/Tenant.
  - Burst (login, password reset, payment endpoints): 10 req/15 min/IP per user identifier.
- Bots blocked at WAF (managed rule sets + Cloudflare Turnstile on public forms).
- Suspicious patterns (impossible travel, new device, anomaly score) → step-up MFA.

### 20.9 Input Validation & Output Encoding

- **FluentValidation** on every command/query DTO; pipeline behavior `ValidationBehavior` short-circuits before handler.
- **Server-side schema validation** mirrors DB constraints (length, regex, enum domain).
- **HTML output**: React + JSX escapes by default; `dangerouslySetInnerHTML` is forbidden by lint rule.
- **SQL injection**: parameters everywhere; `FromSqlInterpolated` over `FromSqlRaw`.

### 20.10 OWASP Top 10 Coverage

| Risk | Coverage |
|------|----------|
| A01 Broken Access Control | Permission policy + resource auth + RLS |
| A02 Cryptographic Failures | TLS 1.3, AES-256-GCM, Argon2id |
| A03 Injection | EF Core parameters, FluentValidation, output encoding |
| A04 Insecure Design | Threat model per module, design reviews, CTF-style red team yearly |
| A05 Security Misconfiguration | IaC scanning, CIS benchmark, hardened images |
| A06 Vulnerable Components | Dependabot, Snyk, SBOM, image signing |
| A07 Identification & Authn Failures | MFA, lockout, breached password check |
| A08 Software & Data Integrity | Signed artifacts, checksum verification, immutable audit |
| A09 Security Logging & Monitoring | OTel + SIEM ingestion (Sentinel/QRadar) |
| A10 SSRF | Outbound allow-list, no blind URL fetch from user input |

### 20.11 Compliance Posture

- **GDPR / DPDP Act** (India): controller/processor agreements per tenant; export & erasure tooling.
- **FERPA** (US K-12): student records access logged; parent consent flags on `Parents`.
- **SOC 2 Type II**: continuous controls — access reviews, change management, vulnerability management, incident response.
- **PCI DSS scope reduction**: card data never touches our DB; we tokenize via PG (Razorpay/Stripe).
