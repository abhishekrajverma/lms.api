# 06 — Event-Driven: RabbitMQ, Outbox, Inbox, Workers

## 15. RabbitMQ Architecture

### 15.1 Why RabbitMQ + Outbox/Inbox

- **Atomic state + event publish:** business write and event "publish intent" share one DB transaction (Outbox). The actual broker publish is asynchronous.
- **Guaranteed delivery:** Outbox publisher has at-least-once delivery; Inbox idempotency makes consumers exactly-once at the application level.
- **Loose coupling:** modules talk via integration events on topic exchanges. No module imports another module's domain types directly — only contract DTOs from `Contracts.<Module>`.

### 15.2 Cluster Topology

```
                        ┌──────────────────┐
                        │   Load Balancer  │  AMQPS 5671 / Mgmt 15672
                        └────────┬─────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              │                  │                  │
       ┌──────▼─────┐     ┌──────▼─────┐     ┌──────▼─────┐
       │ rabbitmq-1 │     │ rabbitmq-2 │     │ rabbitmq-3 │
       │ (mirrored) │     │ (mirrored) │     │ (mirrored) │
       │ disk node  │     │ disk node  │     │ disk node  │
       └────────────┘     └────────────┘     └────────────┘
                          Quorum queues (Raft)
                          TLS, vhost: schoolerp
                          users: erp_publisher, erp_consumer (limited)
```

- **3-node cluster, quorum queues** (replicates state via Raft, durable through node loss).
- **Lazy queues** for high-volume low-priority (e.g., `audit.q.events`).
- **Federation** to a DR cluster in a second region (best-effort).

## 16. Exchange & Queue Design

### 16.1 Exchange Catalog (all `topic`, durable)

| Exchange | Producers | Description |
|----------|-----------|-------------|
| `schoolerp.tenant.exchange` | Tenant module | Tenant lifecycle |
| `schoolerp.identity.exchange` | Identity module | User/role/permission events |
| `schoolerp.billing.exchange` | Billing module | Subscription, invoice, payment, webhook |
| `schoolerp.academic.exchange` | Academics module | AY, Class, Section, Timetable |
| `schoolerp.student.exchange` | Students module | Lifecycle, enrollment, promotion |
| `schoolerp.employee.exchange` | Employees module | Hire/terminate/payroll |
| `schoolerp.attendance.exchange` | Attendance module | Sessions, marks, absences |
| `schoolerp.exam.exchange` | Exams module | Schedule, results, report cards |
| `schoolerp.fees.exchange` | Fees module | Invoice, payment, refund, overdue |
| `schoolerp.transport.exchange` | Transport module | Routes, board/alight, GPS SOS |
| `schoolerp.communication.exchange` | Communication module | Notification queued/sent/failed |
| `schoolerp.library.exchange` | Library module | Issue/return/overdue |
| `schoolerp.inventory.exchange` | Inventory module | PO, stock |
| `schoolerp.audit.exchange` | Fanout subscriber | Mirrors all `*.exchange` for audit |

### 16.2 Routing Key Convention

```
<context>.<aggregate>.<event>.<v1>
```

Examples:
- `student.student.created.v1`
- `student.enrollment.promoted.v1`
- `attendance.session.finalized.v1`
- `fees.invoice.generated.v1`
- `fees.payment.succeeded.v1`
- `billing.subscription.activated.v1`
- `comm.notification.sent.v1`

> Versioning is in the routing key **and** in the message header `x-schema-version`. Bumping a major version of an event creates a *new* routing key (`...v2`); consumers can migrate independently.

### 16.3 Queue Naming Convention

```
<consumer-module>.q.<source-context>.<event-or-pattern>
```

Examples:
- `comm.q.attendance.absent` ← bound to `attendance.student.absent.v1`
- `comm.q.exam.result-published` ← bound to `exam.exam.result-published.v1`
- `audit.q.all` ← bound to `#` on every `*.exchange` (fanout-style audit)
- `billing.q.tenant.lifecycle` ← bound to `tenant.tenant.*.v1`

### 16.4 Queue Properties

| Property | Value | Reason |
|----------|-------|--------|
| `x-queue-type` | `quorum` | Replication & durability |
| `durable` | `true` | Survive broker restart |
| `x-dead-letter-exchange` | `<exchange>.dlx` | Send poison messages to DLX |
| `x-message-ttl` | 86400000 (24h) for retry queues | Time-bounded retries |
| `x-max-length` | 1,000,000 (audit) | Cap memory growth on lazy queues |
| `x-overflow` | `reject-publish-dlx` | When full, dead-letter further publishes |
| `x-max-priority` | 5 (notification) | Allow priority delivery |

### 16.5 Retry & Dead-Letter Topology

```
   ┌───────────┐  publish  ┌───────────────────────────┐
   │ Producer  │──────────▶│ <ctx>.exchange (topic)    │
   └───────────┘           └────────────┬──────────────┘
                                        │ routes
                                        ▼
                            ┌───────────────────────────┐
                            │ <consumer>.q.<pattern>    │
                            │ args:                     │
                            │  x-dead-letter-exchange = │
                            │  <ctx>.dlx                │
                            └────────────┬──────────────┘
                              consumer fail / nack
                                        │
                                        ▼
                            ┌───────────────────────────┐
                            │ <ctx>.dlx (topic)         │
                            └────────────┬──────────────┘
                                        │ routes by orig key
                                        ▼
                            ┌───────────────────────────┐
                            │ <consumer>.q.<pattern>.   │
                            │   retry-30s   (TTL=30s,   │
                            │   DLX=<ctx>.exchange)     │
                            └────────────┬──────────────┘
                              TTL expires
                                        │
                                        ▼
                            (re-published to source exchange,
                             same routing key, x-attempts++)
                            after 5 attempts → final DLQ:
                            ┌───────────────────────────┐
                            │ <consumer>.q.<pat>.dlq    │
                            │ (parked, alert raised)    │
                            └───────────────────────────┘
```

Concretely the retry ladder is **5, 30, 120, 600, 1800 seconds** via dedicated retry queues with `x-message-ttl`. Each retry increments header `x-attempts`. After the cap, the message is parked in a tenant-scoped DLQ for human triage.

### 16.6 Sample Bindings (declared via `IRabbitMqTopology`)

```csharp
// inside module bootstrap:
topology.DeclareExchange("schoolerp.attendance.exchange", ExchangeType.Topic);
topology.DeclareExchange("schoolerp.attendance.dlx",       ExchangeType.Topic);

topology.DeclareQueue("comm.q.attendance.absent", new QueueArgs
{
    Quorum = true,
    DeadLetterExchange = "schoolerp.attendance.dlx",
    MessageTtlMs = null
});

topology.Bind("comm.q.attendance.absent",
              "schoolerp.attendance.exchange",
              "attendance.student.absent.v1");

topology.DeclareRetryQueue("comm.q.attendance.absent.retry-30s",
                           dlx: "schoolerp.attendance.exchange",
                           ttlMs: 30_000);
```

### 16.7 Message Envelope Contract

Every message body is JSON wrapped in a versioned envelope (also surfaced as headers for routing):

```json
{
  "messageId": "0193f0c9-2c39-7a91-9a55-5d4e9c1f8a01",
  "tenantId":  "8a5d9b22-2c1e-4cba-9f8d-4d10b6a2fe11",
  "occurredOnUtc": "2026-06-01T13:35:24.512Z",
  "correlationId": "0193f0c9-2c39-7d4e-99c5-1ab38e99c001",
  "causationId":   null,
  "type": "Student.Domain.Events.StudentCreatedEvent",
  "schemaVersion": "v1",
  "data": {
    "studentId": "0193f0c9-2c39-78bf-9c0a-2d61a8e6b5d3",
    "admissionNumber": "ACME/2026/00123",
    "firstName": "Aanya",
    "lastName": "Sharma",
    "classId": "...",
    "sectionId": "..."
  }
}
```

Headers mirrored:
- `x-tenant-id`, `x-correlation-id`, `x-causation-id`
- `x-schema-version`, `x-attempts`
- `content-type: application/json`
- `content-encoding: utf-8`

## 17. Outbox Pattern

### 17.1 Why

A **single DB transaction** writes the business state *and* enqueues the integration event(s). A separate worker reliably publishes them to RabbitMQ. This avoids the dual-write problem (DB committed but broker publish failed → ghost state).

### 17.2 Schema Recap (`audit_.OutboxMessages`)

Already defined in `02-DATABASE-SCHEMA.md`. Highlights:
- `OutboxMessageId BIGINT IDENTITY` clustered PK (sequential write).
- `MessageId UNIQUEIDENTIFIER UNIQUE` — used downstream for inbox idempotency.
- `Status TINYINT` (`0=Pending,1=Processing,2=Published,3=Failed,4=DeadLettered`).
- `Attempts`, `LastError`, `AvailableAtUtc` (so failed messages can be deferred).
- Filtered covering index `IX_Outbox_Pending` on `(Status, AvailableAtUtc) WHERE Status IN (0,3)`.

### 17.3 Writer (inside the same SaveChanges)

```csharp
public sealed class OutboxRecorder : IOutboxRecorder
{
    private readonly OutboxDbContext _db;
    public OutboxRecorder(OutboxDbContext db) => _db = db;

    public void Record(IIntegrationEvent evt, EventEnvelopeContext ctx)
    {
        var json = JsonSerializer.Serialize(evt, evt.GetType(), JsonOptions.Default);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            MessageId      = Guid.NewGuid(),
            TenantId       = ctx.TenantId,
            Module         = ctx.Module,
            Exchange       = ctx.Exchange,
            RoutingKey     = ctx.RoutingKey,
            EventType      = evt.GetType().FullName!,
            ContentType    = "application/json",
            SchemaVersion  = ctx.SchemaVersion,
            Payload        = json,
            Headers        = JsonSerializer.Serialize(ctx.Headers),
            CorrelationId  = ctx.CorrelationId,
            CausationId    = ctx.CausationId,
            OccurredOnUtc  = ctx.OccurredOnUtc,
            AvailableAtUtc = ctx.OccurredOnUtc,
            Status         = OutboxStatus.Pending
        });
    }
}
```

A `MediatR` pipeline behavior (`DomainEventDispatchBehavior`) gathers domain events from the `ChangeTracker` after the business `SaveChanges`, lifts them into integration events via per-module mappers (`IIntegrationEventFactory<TDomainEvent>`), and writes them through `OutboxRecorder` **inside the same transaction**.

### 17.4 OutboxProcessorWorker (publisher)

- Hosted as a `BackgroundService` on the Worker process (multi-replica, leader-elected via Hangfire `IBackgroundJobClient` or DB advisory lock for the *publisher* leader).
- Pulls a batch of 200 with `READPAST, UPDLOCK, ROWLOCK` and sets `Status = Processing`.
- Publishes each to RabbitMQ with publisher confirms.
- On confirm → `Status = Published`, `ProcessedAtUtc = now`.
- On nack/timeout → `Status = Failed`, `Attempts++`, `AvailableAtUtc = now + backoff(Attempts)`.
- After max attempts → `Status = DeadLettered`, alert raised.

```csharp
public sealed class OutboxProcessorWorker(
    IServiceScopeFactory scopes,
    IRabbitPublisher pub,
    ILogger<OutboxProcessorWorker> log,
    IOptions<OutboxOptions> opts) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        var o = opts.Value;
        while (!stop.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stop); }
            catch (Exception ex) { log.LogError(ex, "Outbox batch failed"); }
            await Task.Delay(o.PollIntervalMs, stop);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var batch = await db.OutboxMessages
            .FromSqlRaw(@"
                SELECT TOP (200) * FROM audit_.OutboxMessages WITH (READPAST, UPDLOCK, ROWLOCK)
                WHERE Status IN (0,3) AND AvailableAtUtc <= SYSUTCDATETIME()
                ORDER BY OutboxMessageId")
            .ToListAsync(ct);

        if (batch.Count == 0) { await tx.CommitAsync(ct); return; }

        foreach (var m in batch) m.Status = OutboxStatus.Processing;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        foreach (var m in batch)
        {
            try
            {
                await pub.PublishAsync(m.Exchange, m.RoutingKey, m.Payload,
                    headers: BuildHeaders(m), confirm: true, ct: ct);
                m.Status = OutboxStatus.Published;
                m.ProcessedAtUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                m.Status = OutboxStatus.Failed;
                m.Attempts++;
                m.LastError = ex.Message;
                m.AvailableAtUtc = DateTime.UtcNow.AddSeconds(Backoff(m.Attempts));
                if (m.Attempts >= 8) m.Status = OutboxStatus.DeadLettered;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static int Backoff(int n) => n switch { 1 => 5, 2 => 30, 3 => 120, 4 => 600, _ => 1800 };
}
```

### 17.5 Operational Health

- Prometheus metrics: `outbox_pending_total`, `outbox_failed_total`, `outbox_publish_latency_seconds`, `outbox_dead_lettered_total`.
- Alerts: pending > 5,000 for 5 min, failed > 100 for 5 min, dead-lettered > 0 for 1 min.
- Dashboard: per-tenant pending depth, oldest pending age, attempt distribution.

## 18. Inbox Pattern

### 18.1 Why

Consumers are **at-least-once** by nature (broker re-delivery, worker restarts). Inbox makes them **idempotent**: each `(Consumer, MessageId)` is recorded once and re-delivery is a no-op.

### 18.2 Schema Recap (`audit_.InboxMessages`)

- Unique index `UX_IB_Idem (Consumer, MessageId)`.
- `Status`, `Attempts`, `LastError`, `ProcessedAtUtc`.

### 18.3 Consumer Pipeline

```
RabbitMQ delivery
       │
       ▼
┌────────────────────────────────────────────────────────┐
│  IntegrationEventConsumer<TEvent>                      │
│   1. Parse envelope → message id, tenant id, payload   │
│   2. Set TenantContext (accessor.Set(...))             │
│   3. Begin DB transaction                               │
│   4. Try INSERT into InboxMessages (Consumer, MessageId)│
│      - on PK conflict → ack + return (already done)    │
│   5. Run handler logic (writes to module DB)           │
│   6. UPDATE InboxMessages SET Status=Processed         │
│   7. Commit + ack                                      │
│      - on exception: rollback, requeue with retry      │
└────────────────────────────────────────────────────────┘
```

### 18.4 Reference Implementation

```csharp
public abstract class InboxConsumer<TEvent> : IIntegrationEventHandler<TEvent>
    where TEvent : class, IIntegrationEvent
{
    private readonly InboxDbContext _inbox;
    private readonly ITenantContextAccessor _accessor;
    protected InboxConsumer(InboxDbContext inbox, ITenantContextAccessor a)
    { _inbox = inbox; _accessor = a; }

    public async Task HandleAsync(EventEnvelope<TEvent> env, CancellationToken ct)
    {
        var consumer = $"{Module}:{GetType().FullName}";
        await using var tx = await _inbox.Database.BeginTransactionAsync(ct);

        var inserted = await _inbox.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO audit_.InboxMessages
                (MessageId, TenantId, Consumer, Module, EventType, SchemaVersion, Payload, Headers,
                 CorrelationId, CausationId, ReceivedAtUtc, Status, Attempts)
            SELECT {env.MessageId}, {env.TenantId}, {consumer}, {Module}, {typeof(TEvent).FullName!},
                   {env.SchemaVersion}, {env.RawJson}, {env.HeadersJson}, {env.CorrelationId}, {env.CausationId},
                   SYSUTCDATETIME(), 1, 0
            WHERE NOT EXISTS (
                SELECT 1 FROM audit_.InboxMessages
                WHERE Consumer = {consumer} AND MessageId = {env.MessageId});", ct);

        if (inserted == 0)               // already processed
        {
            await tx.CommitAsync(ct);
            return;
        }

        _accessor.Set(env.ToTenantContext());
        try
        {
            await OnHandleAsync(env.Data, ct);
            await _inbox.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE audit_.InboxMessages
                SET Status = 2, ProcessedAtUtc = SYSUTCDATETIME()
                WHERE Consumer = {consumer} AND MessageId = {env.MessageId};", ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;                       // broker will redeliver via retry queue
        }
    }

    protected abstract string Module { get; }
    protected abstract Task OnHandleAsync(TEvent evt, CancellationToken ct);
}
```

### 18.5 Idempotency Beyond Inbox

- **Webhook-driven payment events:** the `(Provider, GatewayPaymentId)` unique index in `fees.FeePayments` guarantees at-most-one credit even if the inbox slipped.
- **Side-effect actions** (sending an email/SMS) carry an `idempotencyKey = (NotificationId, RecipientId, ProviderMessageHash)` checked in the channel log.

## 19. Background Workers

### 19.1 Worker Roster

| Worker | Type | Responsibility | Trigger | Scaling |
|--------|------|----------------|---------|---------|
| `OutboxProcessorWorker` | `BackgroundService` | Read pending outbox, publish to RMQ, mark Published/Failed | Polling (200ms) | 2–4 replicas, leader-elected per partition |
| `EmailWorker` | RMQ consumer (`comm.q.email.send`) | Render template, call SendGrid/SES, write `EmailLogs` | Message arrival | Auto-scale on queue depth |
| `SmsWorker` | RMQ consumer (`comm.q.sms.send`) | Twilio/MSG91 send, write `SmsLogs` | Msg arrival | Auto-scale |
| `WhatsAppWorker` | RMQ consumer (`comm.q.whatsapp.send`) | Provider call, template approval check | Msg arrival | Auto-scale |
| `PushWorker` | RMQ consumer | FCM/APNs send | Msg arrival | Auto-scale |
| `NotificationWorker` | RMQ consumer (`comm.q.attendance.absent`, `comm.q.exam.result-published`, `comm.q.fees.payment-succeeded`, …) | Convert domain events to channel-fanned notifications, enqueue per-channel send commands | Msg arrival | 4–10 replicas |
| `BillingWorker` | Hangfire recurring (daily 02:00) | Generate invoices, run dunning, charge auto-pay | Cron | 1 active leader |
| `AttendanceWorker` | RMQ consumer (`attendance.q.aggregate`) | Roll-up to daily/weekly/monthly aggregates for fast reports | Msg arrival | 2–4 |
| `AuditWorker` | RMQ consumer (`audit.q.all`) | Sink all `*.exchange` events into `audit_.AuditLogs` | Msg arrival | 2–4 |
| `ReportGenerationWorker` | RMQ consumer + Hangfire | Build PDFs (report cards, fee receipts, attendance reports) | Command/cron | 4–10 |
| `DataRetentionWorker` | Hangfire recurring (weekly Sun 03:00) | Archive partitions > 12 months, purge expired tenants | Cron | 1 |
| `IndexMaintenanceWorker` | Hangfire weekly | Reorganize/Rebuild indexes, update stats | Cron | 1 |
| `WebhookProcessorWorker` | Hangfire recurring (every 30s) | Re-process `billing.WebhookEvents` with `Status IN (0,2)` | Cron | 1 |
| `RefreshTokenCleanupWorker` | Hangfire daily | Delete tokens where `ExpiresAtUtc < now() - 30d` | Cron | 1 |
| `GpsArchiveWorker` | Hangfire daily | Switch out partitions of `GPSPings` older than 90 days to columnstore | Cron | 1 |

### 19.2 Hangfire Configuration

- **Storage:** SQL Server (separate `Hangfire` database) — keeps Hangfire's polling tables off the OLTP main DB.
- **Servers:** 2–4 replicas; **`UseRecommendedIsolationLevel = true`**, `SchemaName = "Hangfire"`.
- **Recurring jobs** registered on app startup (idempotent `RecurringJob.AddOrUpdate(...)`).
- **Dashboards** mounted at `/_admin/hangfire`, protected by `RequirePlatformAdmin` policy.

### 19.3 RabbitMQ Consumer Hosting

- Hosted in `SchoolErp.Worker` process via a custom `IConsumerHost` that:
  - Opens a single `IConnection` per process, one `IChannel` per consumer worker.
  - Sets `BasicQos(prefetchCount: 50)` per consumer for fair dispatch.
  - Wires consumer → `InboxConsumer<TEvent>` via DI.
- **Per-tenant rate limits** for outbound channels (Email/SMS/WhatsApp) are enforced in-worker via Redis token-bucket keyed `t:{TenantId}:rl:email`.

### 19.4 Notification Fan-out (concrete example)

```
                           attendance.student.absent.v1 (per student, throttled batch)
                                       │
                                       ▼
                       ┌───────────────────────────────┐
                       │ comm.q.attendance.absent      │
                       │ NotificationWorker            │
                       └──────────────┬────────────────┘
                                      │ creates per-channel commands
              ┌───────────────────────┼─────────────────────────┐
              │                       │                         │
              ▼                       ▼                         ▼
       comm.q.email.send       comm.q.sms.send         comm.q.whatsapp.send
       EmailWorker             SmsWorker               WhatsAppWorker
              │                       │                         │
              ▼                       ▼                         ▼
       SendGrid/SES            Twilio/MSG91             WhatsApp BSP
              │                       │                         │
              ▼                       ▼                         ▼
       comm.EmailLogs          comm.SmsLogs            comm.WhatsAppLogs
```

### 19.5 Worker Reliability Checklist

- [x] Idempotent (Inbox) on every consumer.
- [x] Bounded prefetch + concurrency per worker.
- [x] Cancellation-aware (CancellationToken propagated).
- [x] Redelivery-safe DB ops (SQL is `MERGE` or upsert).
- [x] Backpressure: queue depth → autoscale → finally DLQ.
- [x] Observability: every consumer emits trace + metrics with `messageId`, `tenantId`, `consumer`.
- [x] Per-tenant fairness: round-robin or weighted across tenants in the worker pool.
- [x] Tested with chaos (broker kill, DB blip) in nightly e2e.
