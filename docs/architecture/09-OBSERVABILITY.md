# 09 — Observability

## 23. Observability Architecture

### 23.1 Three Pillars + Profiling

| Pillar | Tool | Notes |
|--------|------|-------|
| Logs | **Serilog → Seq + Loki** (cluster) | Structured, JSON-formatted, enriched with TenantId/UserId/CorrelationId |
| Metrics | **OpenTelemetry → Prometheus → Grafana** | Histograms via OTel, scraped from `/metrics` |
| Traces | **OpenTelemetry → Tempo / Jaeger** | W3C trace-context propagation across HTTP, MQ, DB |
| Profiling | **dotnet-monitor + Pyroscope** | On-demand CPU/alloc dumps; continuous profiling sampled |
| RUM | **Sentry** | Frontend errors + Web Vitals |

### 23.2 Architecture Diagram

```
        ┌──────────┐    ┌──────────┐    ┌──────────────┐
        │  API pod │    │ Worker   │    │ Hangfire pod │
        └────┬─────┘    └────┬─────┘    └──────┬───────┘
             │   stdout       │                  │
             │   /metrics     │                  │
             ▼                ▼                  ▼
        ┌────────────────────────────────────────────┐
        │   OTel Collector (DaemonSet, gateway)      │
        │   - receivers: otlp, prometheus, fluent    │
        │   - processors: batch, attributes, filter  │
        │   - exporters: prometheusremotewrite,      │
        │                tempo, loki, seq            │
        └────────────────┬───────────────────────────┘
                         │
       ┌─────────────────┼────────────────────────────┐
       ▼                 ▼                            ▼
  ┌──────────┐    ┌─────────────┐               ┌─────────────┐
  │Prometheus│    │   Tempo     │               │  Loki / Seq │
  └────┬─────┘    └──────┬──────┘               └──────┬──────┘
       └──────────┬──────┴──────────────────────────────┘
                  ▼
              ┌──────────┐
              │  Grafana │ ← unified dashboards (logs, traces, metrics)
              └──────────┘
              ┌──────────┐
              │Alertmgr  │ → PagerDuty/Slack/Teams/email
              └──────────┘
```

### 23.3 Logging (Serilog)

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.With<TenantEnricher>()
    .Enrich.With<UserEnricher>()
    .Enrich.WithCorrelationIdHeader("X-Correlation-Id")
    .Enrich.WithProperty("Service", "schoolerp.api")
    .Enrich.WithProperty("Env", env)
    .Enrich.WithExceptionDetails()
    .Destructure.ByTransforming<Pii>(p => "[REDACTED]")
    .WriteTo.Console(formatter: new ExpressionTemplate(
        "{ {@t, @l, @mt, @x, ..@p} }\n", theme: TemplateTheme.Code))
    .WriteTo.Seq(seqUrl, apiKey: seqKey, controlLevelSwitch: lvlSwitch)
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = otelEndpoint;
        options.Protocol = OtlpProtocol.Grpc;
        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = "schoolerp.api",
            ["service.namespace"] = "schoolerp",
            ["deployment.environment"] = env
        };
    })
    .CreateLogger();
```

#### Log Schema (every event)

| Field | Source |
|-------|--------|
| `@t`, `@l`, `@mt`, `@x` | Serilog standard |
| `TenantId`, `TenantCode` | `TenantEnricher` |
| `UserId`, `UserType` | `UserEnricher` |
| `CorrelationId`, `CausationId` | `LogContext` (request middleware + integration event handler) |
| `TraceId`, `SpanId` | OTel auto |
| `Module` | Logger category mapped from namespace |
| `RequestPath`, `Method`, `StatusCode`, `ElapsedMs` | Request logging middleware |
| `MessageId`, `EventType` | RabbitMQ consumer scope |

#### Log Levels

| Level | Use |
|-------|-----|
| `Debug` | Dev only; off in prod |
| `Information` | Domain transitions, MQ publish/consume, login success |
| `Warning` | Retried failures, lock contention, slow queries |
| `Error` | Unhandled exceptions, dead-lettered messages, failed payments |
| `Fatal` | Process termination, unrecoverable infra |

#### Sampling

- 100 % for errors, warnings, business events.
- Tail-based sampling for high-volume DEBUG/INFO via OTel Collector when needed.

### 23.4 Metrics (OpenTelemetry → Prometheus)

#### 23.4.1 Service Metrics (RED)

- Rate `http_server_requests_per_second{tenant,route,method,status_class}`
- Errors `http_server_errors_total{tenant,route,exception}`
- Duration `http_server_duration_seconds_bucket{tenant,route}`

#### 23.4.2 Resource Metrics (USE)

- `process_cpu_seconds_total`
- `process_resident_memory_bytes`
- `gc_heap_size_bytes`, `gc_pause_seconds_bucket`
- `dotnet_threadpool_threads`

#### 23.4.3 Business Metrics

| Metric | Type | Labels |
|--------|------|--------|
| `students_active` | Gauge | tenant |
| `attendance_marks_total` | Counter | tenant, status |
| `fees_invoices_generated_total` | Counter | tenant |
| `fees_payments_received_total` | Counter | tenant, method, status |
| `fees_collection_amount_total` | Counter | tenant, currency |
| `subscriptions_active` | Gauge | plan |
| `notifications_sent_total` | Counter | tenant, channel, status |
| `notifications_failed_total` | Counter | tenant, channel, reason |
| `outbox_pending` | Gauge | (cluster) |
| `outbox_publish_latency_seconds` | Histogram | exchange |
| `outbox_failed_total` / `outbox_dead_lettered_total` | Counter | exchange |
| `inbox_processed_total` / `inbox_failed_total` | Counter | consumer |
| `mq_queue_depth` | Gauge | queue (exporter from RabbitMQ) |
| `mq_consumer_lag` | Gauge | queue |
| `db_pool_size` / `db_pool_in_use` | Gauge | pod |
| `cache_hit_ratio` | Gauge | namespace |
| `redis_command_latency_seconds` | Histogram | command |

#### 23.4.4 Histograms

- Use **explicit-bucket** histograms tuned for the SLO (e.g., 5/10/25/50/100/200/400/1000/2000/5000 ms).
- Prefer `OpenTelemetry.Instrumentation.AspNetCore`, `EntityFrameworkCore`, `HttpClient`, `Runtime` packages for auto-instrumentation.

### 23.5 Traces (OTel → Tempo)

- W3C Trace Context propagation across:
  - HTTP (auto)
  - RabbitMQ (custom propagator, embedded in headers)
  - Hangfire jobs (custom enricher)
  - SQL Server (`Activity` per command)
- **Sampling:** parent-based with 10 % head sampling + always-sample for traces with errors or `slow=true` baggage.
- **Tags** every span: `tenant.id`, `user.id`, `db.statement` (sanitized), `mq.exchange`, `mq.routing_key`, `event.type`.
- **Custom spans** at handler boundaries (`MediatR` pipeline behavior `TracingBehavior`).

### 23.6 Health Checks

- ASP.NET Core HealthChecks at `/health/live` (liveness, no DB) and `/health/ready` (readiness, deeper checks).
- Probes:
  - SQL Server (`SELECT 1`).
  - RabbitMQ (`channel.Open`).
  - Redis (`PING`).
  - Hangfire server alive.
  - Outbox: pending count under threshold.
- Surfaced via `/_admin/healthchecks-ui` (admin-only).

### 23.7 Dashboards (Grafana)

| Dashboard | Audience | Panels |
|-----------|----------|--------|
| **Platform Overview** | SRE | Req/s, p50/95/99, error %, queue depth, outbox pending |
| **Tenant 360** | CSM | Per-tenant active users, top API errors, queue lag, recent webhooks, billing health |
| **API Performance** | Backend | RED per route, slowest 20 endpoints, EF query top |
| **Worker Performance** | Backend | Outbox lag, queue depth/lag per consumer, retries/dead letters |
| **Database** | DBA | Wait stats, top queries, page life expectancy, blocking, deadlocks, tempdb |
| **Cache** | Backend | Redis ops/s, hit ratio per namespace, eviction rate |
| **Business KPI** | Product | Daily fee collection, attendance %, exam publish funnel |
| **Communication** | Ops | Email/SMS/WhatsApp success %, cost per channel, vendor SLA |
| **Security** | SecOps | Failed logins, lockouts, MFA rate, suspicious patterns |
| **Capacity** | SRE | Connection pool saturation, RAM/CPU, RabbitMQ memory pressure |

### 23.8 Alerts (Prometheus AlertManager)

| Alert | Condition | Severity | Routing |
|-------|-----------|----------|---------|
| `ApiHighErrorRate` | rate > 2 % for 5m | Page | PagerDuty |
| `ApiP95High` | http p95 > 800ms for 10m | Warn | Slack |
| `OutboxPendingHigh` | pending > 5,000 for 5m | Page | PagerDuty |
| `OutboxDeadLettered` | dead_lettered_total > 0 for 5m | Page | PagerDuty |
| `ConsumerLagHigh` | lag > 10,000 msgs for 5m | Page | PagerDuty |
| `DbConnPoolSaturation` | in_use / size > 0.85 for 5m | Warn | Slack |
| `RedisMemoryHigh` | used / max > 0.8 for 10m | Warn | Slack |
| `MqQueueDepth` | depth > 100,000 for 10m | Warn | Slack |
| `BillingFailedPaymentSpike` | >10x baseline for 15m | Warn | Email Finance |
| `LoginFailureSurge` | failed_logins > 50/min from one IP for 5m | Page | SecOps |
| `TenantSuspendedAutomation` | webhook → tenant.suspended | Info | CSM Slack |

### 23.9 Synthetic & Real User Monitoring

- **Synthetic** (Grafana k6/Playwright cron): login, attendance mark, fee receipt, dashboard load — every 5 min from 3 regions.
- **RUM** via Sentry: Web Vitals (LCP, INP, CLS), JS errors, source-mapped, sampled to 20 % of sessions in prod.

### 23.10 Audit & Compliance Telemetry

- Independent stream into immutable Loki tenant; daily integrity hash chain (see `07-SECURITY.md`).
- Retention: 7 years (regulatory).
- Read-only Grafana datasource for auditors with row-level filter on tenant.

### 23.11 Cost & Volume Controls

- Log volume budget per tenant (anomaly detection on noisy tenant).
- Trace sampling adjustable per tenant via runtime feature flag.
- Metric cardinality guarded — max 10 labels, no high-cardinality user IDs in metrics (only in logs/traces).
