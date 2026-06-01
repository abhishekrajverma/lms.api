# 10 — Backup, DR, Deployment, Docker, CI/CD

## 24. Backup & Recovery

### 24.1 Objectives

| Tier | RPO | RTO |
|------|-----|-----|
| SQL Server (primary OLTP) | ≤ 5 min | ≤ 30 min |
| RabbitMQ | ≤ 1 min (mirrored) | ≤ 10 min |
| Redis (cache only) | "lossy ok" | re-warm < 10 min |
| Hangfire DB | ≤ 5 min | ≤ 30 min |
| Blob storage (docs/PDFs) | ≤ 15 min (GRS) | ≤ 1 hr |

### 24.2 SQL Server Backup Plan

| Backup | Frequency | Retention | Location |
|--------|-----------|-----------|----------|
| FULL | Weekly Sunday 01:00 | 90 days | Geo-redundant blob |
| DIFFERENTIAL | Daily 01:00 (except Sun) | 30 days | Geo-redundant blob |
| LOG | Every 5 min | 14 days | Geo-redundant blob (Premium) |
| Snapshot (managed instance) | 12-hour cadence | 35 days | Native |
| Long-term retention (LTR) | Monthly first FULL | 7 years | Compliance archive |

#### Verification
- **Test restore weekly** in non-prod from random week-old backup; CI fails if RTO/RPO miss.
- **`DBCC CHECKDB` weekly** on a restored copy to detect corruption without touching prod.
- Encrypted with TDE; backup files encrypted with separate key.

#### Point-in-Time Recovery (PITR)
- Up to 14 days from log chain.
- Runbook: choose target time → spin restore instance → smoke test → cut over via DNS / connection string.

### 24.3 Application & Config Backups

- **IaC** (Terraform/Bicep) in Git, signed tags.
- **Helm charts** + values per env in Git.
- **Secrets**: Key Vault soft-delete + purge protection; daily export to encrypted offline backup.
- **Hangfire jobs** are idempotent and reseeded on deploy; no DB-level backup tied to job state required (DB backup covers it).

### 24.4 Blob Storage

- Versioning + soft-delete 30 days.
- GRS replication to secondary region.
- Lifecycle rules: cool tier after 30 days, archive after 365 days.

## 25. Disaster Recovery

### 25.1 Strategy: Active-Passive Multi-Region

```
                ┌─────────────────────────────────────────────┐
                │              Front Door (Global)            │
                │       Health-probe driven failover          │
                └────────────────┬─────────────────┬──────────┘
                                 │                 │
                          PRIMARY (Asia)    SECONDARY (EU)  (warm-standby)
                                 │                 │
                ┌────────────────▼────────┐  ┌─────▼────────────────┐
                │ AKS/EKS – API/Worker     │  │ AKS/EKS (scale-down) │
                │ SQL AG (sync local + 1   │  │ SQL AG async replica │
                │ async to EU)            │  │                      │
                │ RabbitMQ cluster        │  │ RMQ cluster (idle)   │
                │ Redis cluster           │  │ Redis (idle)         │
                │ Blob (GRS pair)         │  │ Blob (read access)   │
                └─────────────────────────┘  └──────────────────────┘
```

### 25.2 Failover Runbook

1. Front Door health probe trips → traffic rerouted to EU region.
2. Promote async replica → read/write (manual approval gate or automated for catastrophic events).
3. Bring up scaled-down EU AKS to full capacity (HPA + cluster autoscaler).
4. RabbitMQ federation links flush; consumers attach to EU cluster.
5. Redis is rebuilt cold; cache warms up via background priming.
6. Blob GRS pair becomes read-write (RA-GRS read endpoint promoted).
7. Notify customers via status page; record RTO/RPO actuals.

### 25.3 Failback

- After primary region is healthy:
  1. Re-establish AG; async log shipping in reverse direction until in-sync.
  2. Drain consumers in EU; replay any local-only outbox.
  3. Cut DNS back to primary in low-traffic window; verify with synthetic checks.

### 25.4 DR Drills

- **Quarterly**: full simulated regional failover in non-prod copy of prod.
- **Annual**: live-fire drill (planned) in prod for 1 hour; SLA-protected by customer agreement.

### 25.5 Data Retention & Tenant Deletion

- **Tenant offboarding:** soft-suspend → 30-day grace → 90-day archive → erase. Final erase removes from primary, replicas, blob, and audit (except tamper-evident hash chain).
- **GDPR / DPDP** export pack stays available 7 days post-erase (signed URL).

## 26. Production Deployment Architecture

### 26.1 Environments

| Env | Purpose | Tenants | Hardware |
|-----|---------|---------|----------|
| `dev` | Devs | mock | shared, low spec |
| `qa` | E2E + perf | seeded | mid spec |
| `staging` | Pre-prod, blue-green target | mirror of prod | prod-like |
| `prod-asia` | Customers (APAC) | live | prod |
| `prod-eu` | Customers (EU) | live | prod |
| `dr` | Warm standby | replica | prod scaled to 30 % |

### 26.2 Topology (Kubernetes)

```
Namespace: schoolerp
├── Deployment  api          (HPA 20–60, anti-affinity, PDB)
├── Deployment  worker       (HPA 6–20)
├── Deployment  hangfire     (replicas=2)
├── Job         migrator     (per release; runs `dotnet ef database update`)
├── Service     api-svc       ClusterIP
├── Ingress     api-ingress   AGIC/NGINX → API
├── ServiceMonitor (Prometheus)
└── ConfigMap / Secrets (mounted via CSI Key Vault)
```

### 26.3 Release Strategy

- **Trunk-based development**, feature flags via LaunchDarkly/OpenFeature.
- **Blue/Green** for the `api` deployment; Hangfire and Worker use **rolling**.
- **Canary** at the load-balancer level: 1 % → 5 % → 25 % → 100 % over an hour with auto-rollback on error budget burn.
- **DB migrations** run in a separate **Migrator Job** **before** code deploy; migrations are **expand-then-contract** (additive first, code switch after, then drop).
- **Outbox events** are versioned: producers and consumers can run side-by-side during rollout.

### 26.4 Configuration & Secrets

- Per-env `appsettings.<Env>.json` + env vars; secrets injected via CSI from Key Vault/Secrets Manager.
- Feature flags evaluated server-side and client-side; cache 60 s.

## 27. Docker Architecture

### 27.1 Image Layout

```
images/
├── schoolerp/api:<gitsha>     (~ 110 MB, distroless aspnet)
├── schoolerp/worker:<gitsha>  (~ 110 MB)
├── schoolerp/hangfire:<gitsha>(~ 110 MB)
└── schoolerp/migrator:<gitsha>(~ 130 MB) -- includes EF tools
```

### 27.2 API Dockerfile (multi-stage, distroless, non-root)

```dockerfile
# syntax=docker/dockerfile:1.7
ARG DOTNET_VERSION=9.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY ["Directory.Packages.props", "Directory.Build.props", "Backend.slnx", "./"]
COPY ["src/", "src/"]
COPY ["tests/", "tests/"]

RUN dotnet restore "src/Api/SchoolErp.Api/SchoolErp.Api.csproj"
RUN dotnet publish "src/Api/SchoolErp.Api/SchoolErp.Api.csproj" \
    -c Release -o /app/publish \
    /p:UseAppHost=false /p:DebugType=embedded /p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble-chiseled AS runtime
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_PRINT_TELEMETRY_MESSAGE=false \
    DOTNET_NOLOGO=true \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_GCServer=1
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=2s --retries=3 \
    CMD curl -fsS http://127.0.0.1:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "SchoolErp.Api.dll"]
```

### 27.3 docker-compose (local development)

```yaml
# docker-compose.yml (excerpt)
services:
  api:
    build: { context: ., dockerfile: src/Api/SchoolErp.Api/Dockerfile }
    ports: ["8080:8080"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__SchoolErp=Server=mssql,1433;Database=SchoolErp;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True
      - ConnectionStrings__Hangfire=Server=mssql,1433;Database=Hangfire;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True
      - Redis__Configuration=redis:6379
      - RabbitMq__Hosts=rabbitmq
    depends_on:
      mssql:    { condition: service_healthy }
      redis:    { condition: service_started }
      rabbitmq: { condition: service_healthy }

  worker:
    build: { context: ., dockerfile: src/Worker/SchoolErp.Worker/Dockerfile }
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__SchoolErp=Server=mssql,1433;Database=SchoolErp;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True
      - RabbitMq__Hosts=rabbitmq
      - Redis__Configuration=redis:6379
    depends_on: [ mssql, rabbitmq, redis ]

  hangfire:
    build: { context: ., dockerfile: src/Worker/SchoolErp.Hangfire/Dockerfile }
    environment:
      - ConnectionStrings__Hangfire=Server=mssql,1433;Database=Hangfire;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True
    depends_on: [ mssql ]

  mssql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_PID=Developer
      - SA_PASSWORD=${SA_PASSWORD}
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -No
      interval: 5s
      retries: 30
    volumes: [ "mssqldata:/var/opt/mssql" ]
    ports: ["1433:1433"]

  redis:
    image: redis:7-alpine
    command: ["redis-server", "--appendonly", "yes"]
    volumes: [ "redisdata:/data" ]
    ports: ["6379:6379"]

  rabbitmq:
    image: rabbitmq:3.13-management
    environment:
      - RABBITMQ_DEFAULT_USER=erp
      - RABBITMQ_DEFAULT_PASS=${RMQ_PASSWORD}
    volumes: [ "rmqdata:/var/lib/rabbitmq" ]
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      retries: 10
    ports: ["5672:5672", "15672:15672"]

  seq:
    image: datalust/seq:latest
    environment: [ "ACCEPT_EULA=Y" ]
    ports: ["5341:80"]

  otel:
    image: otel/opentelemetry-collector-contrib:latest
    command: ["--config=/etc/otel/config.yaml"]
    volumes: [ "./infra/otel/config.yaml:/etc/otel/config.yaml:ro" ]
    ports: ["4317:4317", "4318:4318"]

  prometheus:
    image: prom/prometheus:latest
    volumes: [ "./infra/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml:ro" ]
    ports: ["9090:9090"]

  grafana:
    image: grafana/grafana:latest
    environment: [ "GF_SECURITY_ADMIN_PASSWORD=admin" ]
    ports: ["3000:3000"]

volumes:
  mssqldata:
  redisdata:
  rmqdata:
```

### 27.4 Production Deployment (K8s)

- **Helm chart** `schoolerp/` packages api/worker/hangfire/migrator with values per env.
- Replicas: api 20, worker 6, hangfire 2 by default.
- HPA: api on CPU 65 % + custom metric `http_p95_seconds`.
- PodDisruptionBudget: minAvailable 75 %.
- NetworkPolicies: api → DB, RMQ, Redis, Key Vault only.
- CSI Key Vault driver mounts secrets as files.
- TLS via cert-manager (Let's Encrypt + DNS01) for tenant subdomains.

## 28. CI/CD Architecture

### 28.1 Pipeline Stages (GitHub Actions / Azure DevOps)

```
[ pull-request ]
  ├── lint (dotnet format, eslint, biome)
  ├── build (Release)
  ├── unit tests (xUnit + coverage 80% gate)
  ├── architecture tests (NetArchTest: module boundaries)
  ├── EF migrations check (no destructive changes without flag)
  ├── SAST: CodeQL, gitleaks
  ├── SBOM: CycloneDX
  └── docker build + Trivy scan (warn at HIGH, fail at CRITICAL)

[ push to main ]
  ├── all of the above
  ├── integration tests (testcontainers: SQL, RMQ, Redis)
  ├── contract tests (Pact for IModuleApi + integration events)
  ├── perf smoke (k6 100 RPS for 5 min on key flows)
  ├── publish images to ACR/ECR (signed with cosign)
  ├── push helm chart to registry
  └── auto-deploy to staging via ArgoCD

[ tag v*.*.* ]
  ├── promote staging → prod-asia (canary 1→100%)
  ├── DAST (OWASP ZAP) against staging
  ├── DB migration job in prod (expand)
  ├── canary deploy api (1% → 5% → 25% → 100% over 60 min, auto-rollback on SLO burn)
  ├── DB migration contract step (after 7 days of stability)
  └── promote prod-asia → prod-eu after 24h soak
```

### 28.2 Quality Gates

| Gate | Threshold |
|------|-----------|
| Unit test coverage | ≥ 80 % overall, ≥ 90 % domain layer |
| Mutation testing (Stryker) | ≥ 65 % score on critical aggregates |
| Architecture rules | NetArchTest: domain has no infra refs; modules don't import each other directly |
| Security | 0 CRITICAL CVEs in image; 0 secrets leaked |
| Performance | k6 smoke: p95 within 110 % of baseline; failure rate < 1 % |
| Migration | EF script reviewed in PR; destructive changes require explicit `// Destructive` annotation |

### 28.3 Branching Model

- `main` always deployable.
- Short-lived feature branches; PRs squash-merged.
- Long-running schema changes use **feature flags** to decouple deploy from release.

### 28.4 Rollback Strategy

- API: instant blue-green flip.
- Worker: scale down old, scale up new; messages fall back to retry queues if needed.
- DB: forward-only migrations; revert via **compensating migration** rather than rollback. Critical schemas use **shadow tables** for risky changes.

### 28.5 GitOps

- ArgoCD watches `infra/` repo for environment manifests.
- App-of-apps pattern: parent app declares api, worker, hangfire, observability addons, ingress.
- Sync waves ensure migrator runs before api.

### 28.6 Audit & Compliance in Pipeline

- Each release records:
  - Commit SHA, signed by author.
  - SBOM, vulnerability report.
  - Test, coverage, mutation reports.
  - Deployment approver.
- Stored 7 years for SOC 2 evidence.
