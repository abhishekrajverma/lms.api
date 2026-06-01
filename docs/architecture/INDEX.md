# School ERP SaaS — Enterprise Architecture Blueprint

> **Scale Targets:** 10,000+ schools · 5M+ students · 100M+ attendance records · 50M+ fee transactions
> **Stack:** ASP.NET Core 9 · EF Core · SQL Server · React/TS/Vite · MediatR · FluentValidation · AutoMapper · JWT + Refresh · Redis · RabbitMQ · Hangfire · Serilog · OpenTelemetry · Docker
> **Style:** Modular Monolith · Domain Driven Design · Event-Driven · Multi-Tenant SaaS

---

## Document Map

| # | Section | File |
|---|---------|------|
| 1 | High-Level Architecture | [00-OVERVIEW.md](./00-OVERVIEW.md#1-high-level-architecture) |
| 2 | DDD Boundaries | [00-OVERVIEW.md](./00-OVERVIEW.md#2-ddd-boundaries) |
| 3 | Module Breakdown | [00-OVERVIEW.md](./00-OVERVIEW.md#3-module-breakdown) |
| 4 | Aggregate Roots | [00-OVERVIEW.md](./00-OVERVIEW.md#4-aggregate-roots) |
| 5 | Complete ERD | [01-DATABASE-ERD.md](./01-DATABASE-ERD.md#5-complete-erd) |
| 6 | Mermaid ER Diagram | [01-DATABASE-ERD.md](./01-DATABASE-ERD.md#6-mermaid-er-diagram) |
| 7 | DBML (dbdiagram.io) | [01-DATABASE-ERD.md](./01-DATABASE-ERD.md#7-dbml) |
| 8 | SQL Server Schema | [02-DATABASE-SCHEMA.md](./02-DATABASE-SCHEMA.md) |
| 9 | Primary Keys | [03-RELATIONSHIPS.md](./03-RELATIONSHIPS.md#9-primary-keys) |
| 10 | Foreign Keys | [03-RELATIONSHIPS.md](./03-RELATIONSHIPS.md#10-foreign-keys) |
| 11 | Relationship Explanations | [03-RELATIONSHIPS.md](./03-RELATIONSHIPS.md#11-relationship-explanations) |
| 12 | Cardinality Matrix | [03-RELATIONSHIPS.md](./03-RELATIONSHIPS.md#12-cardinality-matrix) |
| 13 | Indexing Strategy | [04-INDEXING.md](./04-INDEXING.md) |
| 14 | Multi-Tenant Design | [05-MULTITENANCY.md](./05-MULTITENANCY.md) |
| 15 | RabbitMQ Architecture | [06-EVENT-DRIVEN.md](./06-EVENT-DRIVEN.md#15-rabbitmq-architecture) |
| 16 | Exchange & Queue Design | [06-EVENT-DRIVEN.md](./06-EVENT-DRIVEN.md#16-exchange--queue-design) |
| 17 | Outbox Pattern | [06-EVENT-DRIVEN.md](./06-EVENT-DRIVEN.md#17-outbox-pattern) |
| 18 | Inbox Pattern | [06-EVENT-DRIVEN.md](./06-EVENT-DRIVEN.md#18-inbox-pattern) |
| 19 | Background Workers | [06-EVENT-DRIVEN.md](./06-EVENT-DRIVEN.md#19-background-workers) |
| 20 | Security Architecture | [07-SECURITY.md](./07-SECURITY.md) |
| 21 | Performance Architecture | [08-PERFORMANCE.md](./08-PERFORMANCE.md#21-performance-architecture) |
| 22 | Caching Strategy | [08-PERFORMANCE.md](./08-PERFORMANCE.md#22-caching-strategy) |
| 23 | Observability | [09-OBSERVABILITY.md](./09-OBSERVABILITY.md) |
| 24 | Backup & Recovery | [10-OPERATIONS.md](./10-OPERATIONS.md#24-backup--recovery) |
| 25 | Disaster Recovery | [10-OPERATIONS.md](./10-OPERATIONS.md#25-disaster-recovery) |
| 26 | Production Deployment | [10-OPERATIONS.md](./10-OPERATIONS.md#26-production-deployment) |
| 27 | Docker Architecture | [10-OPERATIONS.md](./10-OPERATIONS.md#27-docker-architecture) |
| 28 | CI/CD Architecture | [10-OPERATIONS.md](./10-OPERATIONS.md#28-cicd-architecture) |
| 29 | Scalability Recommendations | [11-SCALABILITY.md](./11-SCALABILITY.md#29-scalability-recommendations) |
| 30 | Best Practices | [11-SCALABILITY.md](./11-SCALABILITY.md#30-best-practices) |

---

## Reading Order for New Engineers

1. **Architects:** 00 → 05 → 06 → 11
2. **Backend devs:** 00 → 01 → 02 → 03 → 04 → 06
3. **DBAs:** 02 → 03 → 04 → 08 → 10
4. **DevOps/SRE:** 09 → 10 → 11
5. **Security:** 05 → 07 → 09

---

## Authoritative Conventions

- **Naming:** `PascalCase` for SQL identifiers, schemas per module (`dbo` reserved), prefixed surrogate keys (`StudentId`, `TenantId`).
- **Keys:** All PKs are `UNIQUEIDENTIFIER` (sequential GUID via `NEWSEQUENTIALID()`) for distributed-friendly inserts. `BIGINT IDENTITY` is used only for high-volume append-only ledgers (`AuditLogs`, `OutboxMessages`, `StudentAttendance`).
- **Money:** `DECIMAL(18,4)` everywhere, currency code `CHAR(3)` ISO-4217.
- **Time:** `DATETIME2(3)` UTC always, `DATETIMEOFFSET(3)` only at edges.
- **Soft delete:** `IsDeleted BIT NOT NULL DEFAULT 0` + filtered indexes `WHERE IsDeleted = 0`.
- **Concurrency:** `RowVersion ROWVERSION` on every aggregate root.
- **Tenant column:** `TenantId UNIQUEIDENTIFIER NOT NULL` is the **leading** column on every composite index.
