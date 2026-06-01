# 11 — Scalability & Best Practices + Folder Structure

## Modular Monolith Folder Structure (full reference)

```
SchoolErp.sln (or Backend.slnx)

src/
├── BuildingBlocks/                                  # Pure infra-agnostic primitives
│   ├── BuildingBlocks.Domain/
│   │   ├── AggregateRoot.cs
│   │   ├── Entity.cs
│   │   ├── ValueObject.cs
│   │   ├── BusinessRule.cs
│   │   ├── DomainException.cs
│   │   ├── IDomainEvent.cs
│   │   └── Results/{Result.cs, Error.cs}
│   ├── BuildingBlocks.Application/
│   │   ├── Cqrs/{ICommand.cs, IQuery.cs, ICommandHandler.cs, IQueryHandler.cs}
│   │   ├── Behaviors/{ValidationBehavior.cs, LoggingBehavior.cs, TransactionBehavior.cs,
│   │   │              CachingBehavior.cs, IdempotencyBehavior.cs, TracingBehavior.cs}
│   │   ├── Mapping/IMappingProfile.cs
│   │   ├── Pagination/{PageRequest.cs, PageResult.cs, Cursor.cs}
│   │   └── Modules/{IModuleApi.cs, IIntegrationEvent.cs}
│   └── BuildingBlocks.Infrastructure/
│       ├── Persistence/{IUnitOfWork.cs, BaseDbContext.cs, Interceptors/}
│       ├── Outbox/{OutboxRecorder.cs, OutboxProcessorWorker.cs, OutboxDbContext.cs}
│       ├── Inbox/{InboxConsumer.cs, InboxDbContext.cs}
│       ├── Caching/{ICacheService.cs, RedisCacheService.cs, TenantedCacheKey.cs}
│       ├── EventBus/RabbitMq/...
│       ├── Files/{IFileStorage.cs, BlobStorageProvider.cs}
│       ├── Authorization/{PermissionPolicyProvider.cs, IPermissionResolver.cs}
│       ├── Crypto/{IDataProtection.cs, FieldEncryption.cs}
│       └── Telemetry/{TenantEnricher.cs, UserEnricher.cs}
│
├── SharedKernel/                                    # Cross-context VOs and IDs
│   ├── Money.cs, Address.cs, Email.cs, PhoneNumber.cs, DateRange.cs
│   └── Identifiers/{TenantId.cs, StudentId.cs, ClassId.cs, SectionId.cs, ...}
│
├── MultiTenancy/
│   ├── ITenantContext.cs, TenantContext.cs, ITenantContextAccessor.cs
│   ├── Resolvers/{JwtClaimTenantResolver.cs, HostHeaderTenantResolver.cs, PathTenantResolver.cs}
│   ├── Middleware/TenantResolutionMiddleware.cs
│   ├── ITenantStore.cs, RedisTenantStore.cs
│   └── EfCore/TenantSessionContextInterceptor.cs
│
├── Authorization/
│   ├── PermissionRequirement.cs, PermissionAuthorizationHandler.cs
│   ├── ResourceAuthorization/{TeacherSectionRequirement.cs, ParentChildRequirement.cs}
│   └── PolicyNames.cs
│
├── Caching/
│   ├── CacheKeys.cs
│   └── CacheInvalidationConsumer.cs
│
├── EventBus/
│   ├── RabbitMq/
│   │   ├── IRabbitMqConnection.cs, RabbitMqConnection.cs
│   │   ├── IRabbitMqTopology.cs, RabbitMqTopology.cs
│   │   ├── Publishers/{IRabbitPublisher.cs, RabbitPublisher.cs, ConfirmsPublisher.cs}
│   │   ├── Consumers/{IConsumerHost.cs, RabbitMqConsumerHost.cs}
│   │   ├── Exchanges/Exchanges.cs               # constants for exchange names
│   │   ├── Queues/Queues.cs                     # constants for queue names
│   │   ├── Retry/RetryStrategy.cs
│   │   └── DeadLetter/DeadLetterStrategy.cs
│   └── Contracts/                                  # public DTOs for integration events
│       ├── Tenant/*.cs, Identity/*.cs, Billing/*.cs, Academic/*.cs,
│       ├── Students/*.cs, Employees/*.cs, Attendance/*.cs, Exam/*.cs,
│       ├── Fees/*.cs, Transport/*.cs, Communication/*.cs,
│       ├── Library/*.cs, Inventory/*.cs
│
├── Observability/
│   ├── Logging/SerilogConfig.cs
│   ├── Tracing/OtelConfig.cs
│   ├── Metrics/{ApiMetrics.cs, DomainMetrics.cs, OutboxMetrics.cs}
│   └── Health/HealthChecks.cs
│
├── Modules/
│   ├── TenantManagement/
│   │   ├── TenantManagement.Domain/
│   │   ├── TenantManagement.Application/
│   │   ├── TenantManagement.Infrastructure/
│   │   ├── TenantManagement.Persistence/
│   │   └── TenantManagement.Api/
│   ├── Identity/                  (… same shape …)
│   ├── Billing/
│   ├── Academics/
│   ├── Students/
│   ├── Employees/
│   ├── Attendance/
│   ├── Examinations/
│   ├── Fees/
│   ├── Transport/
│   ├── Communication/
│   ├── Library/
│   ├── Inventory/
│   └── Audit/
│
├── Api/SchoolErp.Api/                              # Composition root for HTTP
│   ├── Program.cs (Minimal hosting + module registrations)
│   ├── Modules/{ModuleRegistration.cs}             # IModule.AddModule(IServiceCollection)
│   ├── Endpoints/                                   # global endpoints (health, ops)
│   ├── Filters/{ExceptionMappingFilter.cs}
│   ├── Authentication/JwtBearerOptionsConfig.cs
│   └── Dockerfile
│
├── Worker/SchoolErp.Worker/                        # RabbitMQ consumer host + outbox
│   ├── Program.cs
│   ├── ConsumerRegistration.cs
│   └── Dockerfile
│
├── Worker/SchoolErp.Hangfire/                      # Hangfire server host
│   ├── Program.cs
│   ├── RecurringJobs/{BillingJobs.cs, RetentionJobs.cs, IndexMaintenanceJobs.cs, ...}
│   └── Dockerfile
│
└── Database/
    ├── Migrations/                                  # EF migrations per DbContext
    ├── Scripts/{partitions.sql, security.policies.sql, seed.sql}
    └── SchoolErp.Migrator/                          # `dotnet ef database update` host

tests/
├── UnitTests/<Module>.UnitTests/
├── IntegrationTests/<Module>.IntegrationTests/   (testcontainers)
├── ArchitectureTests/SchoolErp.ArchitectureTests/ (NetArchTest)
├── ContractTests/SchoolErp.ContractTests/          (Pact)
└── E2ETests/SchoolErp.E2E/                          (Playwright + k6)

infra/
├── docker/{otel/, prometheus/, grafana/}
├── helm/schoolerp/
├── terraform/<env>/
└── k8s/<env>/

docs/
├── architecture/                                    # this directory
├── runbooks/
└── adr/                                             # Architecture Decision Records
```

> Module isolation is enforced by:
> - Each module is its own assembly with `internal` types.
> - Public surface (`<Module>.Application/Public/IModuleApi.cs`) is the **only** way other modules talk in-process.
> - Cross-module write coupling is **not allowed**; only read-only `IModuleApi` reads or **integration events**.
> - `NetArchTest` rules enforce the above at build time.

---

## 29. Scalability Recommendations

### 29.1 Vertical Targets (single-cell limits)

| Resource | Healthy ceiling | Migration trigger |
|----------|------------------|--------------------|
| SQL Server primary | 80 % CPU, 70 % buffer pool, < 5 ms log write | Begin sharding decision |
| RabbitMQ memory | < 60 % high-watermark | Add nodes / split exchanges |
| Redis memory | < 70 % `maxmemory` | Add shards |
| API pod p95 | < 400 ms | Add replicas / optimize hot path |

### 29.2 Horizontal Strategies

#### 29.2.1 API & Worker
- Stateless. **Already horizontally scalable** via HPA (CPU + custom queue depth metric).
- No sticky sessions; JWT carries identity.

#### 29.2.2 SQL Server — Read Scaling
- **Read replicas** with `ApplicationIntent=ReadOnly`.
- Route via **secondary connection string** in `IDbContextFactory<TReadDbContext>`.
- All **queries** can be replica-routed; **commands** stay primary.

#### 29.2.3 SQL Server — Write Scaling (Sharding)
- When > 1,500 active tenants per cell or > 8 TB hot data:
  - **Tenant-affinity sharding**: hash `TenantId` → cell index.
  - Each cell hosts an SQL AG with the same schema.
  - Tenant Store (Redis-cached) maps `TenantId → ConnectionString` and `TenantId → BlobAccount`.
  - **No cross-tenant joins** ever — already enforced.
- Migration runbook to move a tenant between cells:
  1. Suspend writes for tenant (read-only mode flag).
  2. Snapshot tenant rows into staging shard via SqlBulkCopy + verification.
  3. Drain outbox/inbox; replay all events into new shard.
  4. Update Tenant Store mapping; resume writes.
  5. Decommission rows from old shard after verification.
- **Total effort minimized** because:
  - All FKs are already tenant-scoped.
  - All blobs already stored under `t/{TenantId}/...` prefixes.
  - No cross-tenant cache/state exists.

#### 29.2.4 RabbitMQ Scaling
- Add nodes; quorum queues handle replication.
- For very high-volume topics (audit), introduce **per-region** clusters with **federation** instead of one global cluster.

#### 29.2.5 Redis Scaling
- Native Redis Cluster sharding by hash slot.
- Hot keys (e.g., subscription state) localized via tagged keys `{tenant:abc}:something` to keep tenant data co-located.

#### 29.2.6 Blob & Files
- Per-tenant container with private endpoint; CDN with signed URLs for hot reads.
- For very large tenants (> 10 TB), provision dedicated storage account.

### 29.3 Workload-Specific Scale Plans

| Workload | Bottleneck | Scale Plan |
|----------|------------|------------|
| Attendance fan-out (peak 9–10 AM) | DB writes | Pre-warm connection pool; bulk insert; read replicas absorb dashboards |
| Fee billing run (start of month) | DB + outbox | Schedule per-tenant in time-slices; bulk invoice generation; dedicated worker shard |
| Notification storm (exam result) | Outbound provider rate limits | Throttle per tenant via Redis token-bucket; fallback to email if SMS exceeds quota |
| Login spike (8 AM) | Auth + Redis | Pre-warm JWKS cache; permission cache pre-fetch on login |
| Reports (year-end) | Read CPU | Run on read-replica with lower priority; pre-materialize roll-ups |
| GPS pings | Write rate, retention | Direct-to-Kafka path optional; SQL Server + monthly archival to columnstore |

### 29.4 Capacity Planning (rough targets per cell)

| Tenant size | Students | Storage growth | Reads/sec | Writes/sec |
|-------------|----------|----------------|-----------|------------|
| Small | 200 | 0.5 GB/yr | 5 | 1 |
| Medium | 1,500 | 4 GB/yr | 30 | 6 |
| Large | 10,000 | 30 GB/yr | 200 | 40 |
| 1,500 mixed tenants/cell | ~2 M total | ~5 TB/yr | ~12k | ~2k |
| 10K tenants total | 5 M | 30 TB/yr | 80k | 14k |

These numbers fit comfortably in 7 cells, each with 24-vCPU SQL primary + 2 read replicas.

### 29.5 Async-First Design

- Every cross-module workflow defaults to **eventual consistency** via outbox/inbox.
- Synchronous in-process calls only for read queries via `IModuleApi`.
- Saga pattern (orchestrated) for multi-step workflows (admission, promotion, refund).

### 29.6 Cost Optimization

- **Reserved instances** for SQL/Redis/AKS baselines.
- **Spot/Preemptible** workers for non-critical paths (report generation).
- **Tiered storage** for blobs (hot/cool/archive).
- **Log volume budgets** per tenant to avoid noisy neighbors.

---

## 30. Best Practices Used by Enterprise SaaS Products

### 30.1 Architecture & Code

- **Modular Monolith with DDD**: clean layers, aggregates, domain events, no God classes.
- **CQRS** at the application boundary (commands/queries separated).
- **Outbox + Inbox** for at-least-once integration with idempotent consumers.
- **API surface stability**: REST + JSON, semantic versioning, `Accept-Version` header for breaking changes, OpenAPI per module + global.
- **Consumer-driven contracts** (Pact) between modules and clients.
- **Clean Architecture** dependency rule: Domain ← Application ← Infrastructure; Api/Worker compose.
- **NetArchTest** to enforce module boundaries in CI.
- **Result types over exceptions** for expected business failures; exceptions for truly exceptional flows.
- **Strong typing of IDs** (`StudentId`, not `Guid`) to prevent mix-ups.
- **Immutable value objects** (`Money`, `Address`).

### 30.2 SaaS-Specific

- **Multi-tenant defense-in-depth** (middleware → query filter → interceptor → RLS).
- **Per-tenant feature flags**, plan gating, and quotas (`SubscriptionUsage`).
- **Tenant lifecycle automation** (provision, suspend, deprovision, export, erasure).
- **Per-tenant rate limiting** at API and outbound channels.
- **White-label theming** + custom domains with auto-managed TLS.
- **Per-tenant audit + data export tooling** for compliance.
- **Quota dashboards** for customers and ops.

### 30.3 Data

- **Partitioning** for hot, append-heavy tables.
- **Sequential GUIDs** for distributed-friendly inserts.
- **Soft delete + audit** universally.
- **Optimistic concurrency** via `RowVersion` on every aggregate root.
- **Computed/persisted columns** for derived hot fields.
- **Filtered indexes** for `IsDeleted=0` and status-bound predicates.
- **Tenant-leading composite indexes** to force tenant-scoped seeks.
- **3NF + measured denormalizations**, never accidental.

### 30.4 Reliability & Resilience

- **Idempotent commands & consumers** end-to-end.
- **Polly** for HTTP retries (decorrelated jitter, circuit breakers).
- **Bulkheads** for outbound channels (separate Redis-backed limiters per provider).
- **Graceful shutdown**: drain channels, finish in-flight, then exit.
- **Chaos testing** (broker kill, DB latency injection) in nightly CI.
- **Capacity tested** with k6 + realistic data volumes.

### 30.5 Security

- **JWT + rotated refresh tokens** with reuse detection.
- **RBAC + permission policy + resource-based authorization**.
- **mTLS** between services; **TLS 1.3** on edges; HSTS preload.
- **TDE + Always Encrypted** for sensitive PII; AES-GCM for blobs.
- **Argon2id** password hashing; **MFA** for privileged roles.
- **WAF + bot management + edge rate limiting**.
- **Continuous SAST/DAST/SCA + SBOM + signed images**.
- **Tamper-evident audit chain**.

### 30.6 Observability

- **Three pillars + profiling**, all enriched with tenant context.
- **SLOs + error budgets**; alerts trigger on **burn rate**, not raw thresholds.
- **Tenant 360 dashboard** so CSMs and SRE see the same view.
- **Synthetic checks** from multiple regions + RUM.

### 30.7 Delivery

- **Trunk-based development**, short-lived branches.
- **Blue/green API**, canary at edge.
- **Expand-then-contract migrations**; feature flags decouple deploy from release.
- **GitOps**: declarative, audited, reproducible.
- **One-click rollback**; forward-only DB with compensating migrations.

### 30.8 Operations

- **Tested DR drills**, RPO/RTO measured, not guessed.
- **Runbooks for every alert** (link in alert annotation).
- **Standby cells warm**, blue-green-region-failover practiced.
- **Cost dashboards** per tenant + team to keep margins healthy.
- **Compliance**: SOC 2, GDPR, FERPA tooling baked in.

### 30.9 Engineering Culture

- **PR review SLA 24h**, mandatory for `main`.
- **Architecture Decision Records (ADRs)** for any significant decision.
- **On-call rotation** with blameless postmortems within 5 business days.
- **Service Catalog** (each module has owner, on-call, SLO, runbook, dashboards).
- **Quarterly capacity reviews** & **annual security audits**.

### 30.10 Anti-Patterns We Reject

- ❌ Distributed transactions across modules.
- ❌ Synchronous HTTP between modules in the same process.
- ❌ Schema-per-tenant (operational nightmare at scale).
- ❌ `WITH (NOLOCK)` for "fast" reads.
- ❌ Caching PII without strict justification + TTL.
- ❌ Direct foreign-key navigation across module boundaries in domain code.
- ❌ "Magic strings" for permissions/queues/exchanges (use constants).
- ❌ Logging or metrics with high-cardinality labels (raw user emails, PII).
- ❌ Long-running cross-aggregate transactions; we use sagas + outbox instead.
- ❌ Manual production database changes without a migration script.
