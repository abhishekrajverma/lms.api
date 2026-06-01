# 00 — Overview, DDD Boundaries, Modules, Aggregates

## 1. High-Level Architecture

### 1.1 Logical Architecture (Modular Monolith, Event-Driven)

```
                                  ┌──────────────────────────────────────────┐
                                  │             Edge / Clients               │
                                  │  React+TS+Vite SPA · Mobile · 3rd-party  │
                                  └────────────────────┬─────────────────────┘
                                                       │ HTTPS (TLS 1.3)
                                  ┌────────────────────▼─────────────────────┐
                                  │          API Gateway / WAF / CDN          │
                                  │     (Azure Front Door / AWS CloudFront)   │
                                  │   - DDoS  - Rate limit  - Geo routing     │
                                  └────────────────────┬─────────────────────┘
                                                       │
                          ┌────────────────────────────┼────────────────────────────┐
                          │                            │                            │
                ┌─────────▼─────────┐        ┌─────────▼─────────┐        ┌─────────▼─────────┐
                │  ASP.NET Core 9   │        │  ASP.NET Core 9   │  ...   │  ASP.NET Core 9   │
                │   API Pod (N=20)  │        │   API Pod (N=20)  │        │   API Pod (N=20)  │
                │ Modular Monolith  │        │ Modular Monolith  │        │ Modular Monolith  │
                └─────┬───────┬─────┘        └─────┬───────┬─────┘        └─────┬───────┬─────┘
                      │       │                    │       │                    │       │
        ┌─────────────▼─┐  ┌──▼─────────┐  ┌───────▼──┐  ┌─▼──────────┐    ┌────▼──┐  ┌─▼─────┐
        │   SQL Server   │  │   Redis    │  │ RabbitMQ │  │ Hangfire DB│    │ Blob  │  │ Seq   │
        │ AlwaysOn AG    │  │  Cluster   │  │  Cluster │  │  (SQL)     │    │Storage│  │OTel   │
        │ Primary + 2 RR │  │ 6 nodes    │  │ 3 nodes  │  │            │    │       │  │ Tempo │
        └────────────────┘  └────────────┘  └────┬─────┘  └────────────┘    └───────┘  └───────┘
                                                 │
                                ┌────────────────┼─────────────────────────┐
                                │                │                         │
                       ┌────────▼────────┐ ┌─────▼──────┐         ┌────────▼─────────┐
                       │ Worker Service  │ │  Hangfire  │         │  Hangfire        │
                       │ (RabbitMQ       │ │  Server    │   ...   │  Server          │
                       │  Consumers)     │ │ (Recurring)│         │ (Recurring)      │
                       └─────────────────┘ └────────────┘         └──────────────────┘
                                  │
                       ┌──────────▼─────────────┐
                       │ External Integrations  │
                       │  SendGrid · Twilio ·   │
                       │  Razorpay/Stripe ·     │
                       │  WhatsApp BSP · GPS    │
                       └────────────────────────┘
```

### 1.2 Process Topology

| Process | Purpose | Replicas (prod) | Scaling Trigger |
|---------|---------|-----------------|-----------------|
| `SchoolErp.Api` | HTTP request handling, command/query dispatch, immediate domain transactions, **outbox writes only** | 20–60 | CPU > 65 %, p95 > 400 ms |
| `SchoolErp.Worker` | RabbitMQ consumers (outbox publisher + inbox processors per module) | 6–20 | Queue depth, consumer lag |
| `SchoolErp.Hangfire` | Recurring jobs (billing runs, report generation, archiving, retention) | 2–4 | Job queue length |
| `SchoolErp.Migrator` | Deploy-time EF Core migrations + seeding | Job (per release) | n/a |

### 1.3 Why a Modular Monolith (and not microservices on day one)

- **Single deployable** with strong module boundaries gives microservice-grade isolation **without** distributed transactions, ops overhead, or polyglot persistence cost.
- **Public surface** of every module is an `IModuleApi` (CQRS commands/queries) + integration events. Internal entities are `internal` to the module assembly. This makes future extraction to microservices a refactor, not a rewrite.
- **Event-driven via Outbox/Inbox** gives us eventual-consistency primitives across modules immediately, again preserving the future microservice path.

---

## 2. DDD Boundaries

### 2.1 Bounded Contexts (Strategic DDD)

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CORE DOMAIN (Differentiator)                │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐    │
│  │   Academics      │ │    Students      │ │   Examinations    │    │
│  │  (Curriculum)    │ │  (Lifecycle)     │ │   (Assessment)    │    │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘    │
│  ┌──────────────────┐ ┌──────────────────┐                          │
│  │   Attendance     │ │      Fees        │                          │
│  └──────────────────┘ └──────────────────┘                          │
└─────────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────────┐
│                      SUPPORTING DOMAIN                              │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌────────────┐ │
│  │  Employees   │ │   Transport  │ │   Library    │ │ Inventory  │ │
│  └──────────────┘ └──────────────┘ └──────────────┘ └────────────┘ │
│  ┌──────────────┐                                                   │
│  │Communication │                                                   │
│  └──────────────┘                                                   │
└─────────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────────┐
│                       GENERIC DOMAIN                                │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌────────────┐ │
│  │  Tenants     │ │   Identity   │ │   Billing    │ │   Audit    │ │
│  └──────────────┘ └──────────────┘ └──────────────┘ └────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Context Map (with relationship patterns)

| Upstream | Downstream | Pattern | Notes |
|----------|------------|---------|-------|
| Tenants | **All other** | Open Host Service / Published Language | Tenant lifecycle events bind everything |
| Identity | All others | Open Host Service | Publishes `UserCreated`, `UserDeactivated` |
| Billing | Tenants | Customer-Supplier | Subscription state gates feature access |
| Academics | Students, Attendance, Exams, Fees | Shared Kernel (`AcademicYearId`, `ClassId`) | Read-only IDs flow downstream |
| Students | Attendance, Exams, Fees, Transport, Library | Conformist downstream | Students owns the canonical student |
| Attendance | Communication | Customer-Supplier | Absence triggers parent SMS/email |
| Exams | Communication | Customer-Supplier | Result publish triggers notifications |
| Fees | Communication, Billing | Customer-Supplier | Receipts, dunning |
| Communication | All | Anti-Corruption Layer to gateways | Templates owned here, not duplicated |
| Audit | All | Generic subscriber on `schoolerp.audit.exchange` | Cross-cutting |

### 2.3 Ubiquitous Language (selected)

| Term | Definition |
|------|------------|
| **Tenant** | A school customer (not a school). One Tenant may run multiple **Schools** (campuses) |
| **AcademicYear** | Tenant-scoped time window with terms, e.g. *2025–2026* |
| **Term** | Sub-window within AcademicYear (Semester, Trimester, Quarter) |
| **Class** | Grade level (Grade 5). **Section** is a stream within a class (5-A) |
| **Enrollment** | The fact that a Student is registered to a Section in a specific AcademicYear |
| **Promotion** | An end-of-year event moving a Student from one Section/Class to the next |
| **AttendanceSession** | A markable slot (period or full-day) for a Section |
| **FeeStructure** | Template assigning FeeTypes (with amounts) to a Class for an AcademicYear |
| **Invoice** | A demand for payment. Always tenant- and student-scoped |

---

## 3. Module Breakdown

15 modules total — 13 business + 2 platform. Every module has 4 layers (Domain, Application, Infrastructure, Persistence) plus an `Api` exposed surface (controllers/minimal APIs registered by the host).

```
src/Modules/<Module>/
├── <Module>.Domain          # Aggregates, entities, VOs, domain events, domain services, specs
├── <Module>.Application     # CQRS handlers, validators, mappers, integration event handlers, IModuleApi
├── <Module>.Infrastructure  # External adapters (gateways, file storage, cache, MQ adapters)
├── <Module>.Persistence     # DbContext, configurations, migrations, repositories, query services
└── <Module>.Api             # Controllers / Minimal API endpoints, request/response DTOs
```

### 3.1 Tenant Management
- **Purpose:** onboarding, branding, per-tenant configuration, custom domains.
- **Aggregates:** `Tenant`, `TenantBranding`, `TenantDomain`.
- **Tables:** `Tenants`, `TenantSettings`, `TenantBranding`, `TenantDomains`, `TenantStorageSettings`, `TenantEmailSettings`, `TenantSmsSettings`.
- **Public events:** `TenantProvisionedEvent`, `TenantSuspendedEvent`, `TenantUpgradedEvent`, `TenantDeprovisionedEvent`.

### 3.2 Identity & Access Management
- **Purpose:** authn/z, JWT, RBAC, password policy, MFA, sessions.
- **Aggregates:** `User`, `Role`, `Permission`, `RefreshToken`.
- **Tables:** `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions`, `RefreshTokens`, `LoginHistory`, `PasswordHistory`, `UserSessions`.
- **Public events:** `UserRegisteredEvent`, `UserAuthenticatedEvent`, `UserLockedOutEvent`, `RoleAssignedEvent`, `PermissionGrantedEvent`.

### 3.3 Subscription & Billing
- **Purpose:** plans, metered usage, invoices, PG integration, webhooks.
- **Aggregates:** `Plan`, `Subscription`, `SubscriptionInvoice`, `Coupon`, `Refund`.
- **Tables:** `Plans`, `Features`, `PlanFeatures`, `Subscriptions`, `SubscriptionInvoices`, `SubscriptionPayments`, `SubscriptionUsage`, `Coupons`, `Refunds`, `WebhookEvents`, `BillingAddresses`, `PaymentMethods`, `TaxConfigurations`.
- **Public events:** `SubscriptionActivatedEvent`, `SubscriptionRenewedEvent`, `SubscriptionPastDueEvent`, `InvoiceGeneratedEvent`, `PaymentSucceededEvent`, `PaymentFailedEvent`.

### 3.4 Academic Management
- **Purpose:** academic calendar, classes/sections, subjects, timetable.
- **Aggregates:** `AcademicYear` (with `Term`s), `Class` (with `Section`s), `Subject`, `Timetable` (with `Period`s).
- **Tables:** `AcademicYears`, `Terms`, `Classes`, `Sections`, `Subjects`, `SubjectAssignments`, `Timetables`, `Periods`, `ClassTeachers`.
- **Public events:** `AcademicYearStartedEvent`, `AcademicYearClosedEvent`, `ClassCreatedEvent`, `TimetablePublishedEvent`.

### 3.5 Student Management
- **Purpose:** student lifecycle (admission → promotion → exit), parent linkage.
- **Aggregates:** `Student` (root with `Address`, `Document`, `MedicalRecord`, `EmergencyContact`, `StatusHistory`, `PromotionHistory`), `StudentEnrollment` (root, references AcademicYear/Class/Section).
- **Tables:** `Students`, `StudentEnrollments`, `StudentParents`, `StudentAddresses`, `StudentDocuments`, `StudentMedicalRecords`, `StudentEmergencyContacts`, `StudentPromotionHistory`, `StudentStatusHistory`, `Parents`.
- **Public events:** `StudentCreatedEvent`, `StudentEnrolledEvent`, `StudentPromotedEvent`, `StudentTransferredEvent`, `StudentGraduatedEvent`, `StudentDeactivatedEvent`.

### 3.6 Employee Management
- **Purpose:** employee lifecycle, payroll, attendance.
- **Aggregates:** `Employee` (with `Document`, `BankAccount`), `Department`, `Designation`, `EmployeePayroll`.
- **Tables:** `Employees`, `Departments`, `Designations`, `EmployeeDocuments`, `EmployeeAttendance`, `EmployeePayroll`, `EmployeeBankAccounts`.
- **Public events:** `EmployeeHiredEvent`, `EmployeeTerminatedEvent`, `PayrollProcessedEvent`.

### 3.7 Attendance Management
- **Purpose:** student & employee attendance at scale (100M+ rows/year).
- **Aggregates:** `AttendanceSession` (root, owns line-items as VOs in domain but as separate table for performance).
- **Tables:** `AttendanceSessions`, `StudentAttendance`, `EmployeeAttendance` (consumes from Employee context where richer).
- **Public events:** `AttendanceSessionOpenedEvent`, `AttendanceMarkedEvent` (batch), `AttendanceSessionFinalizedEvent`, `StudentAbsentEvent` (per-student, throttled).

### 3.8 Examination Management
- **Purpose:** exam scheduling, marks entry, grading, report cards.
- **Aggregates:** `Exam` (with `ExamSchedule`, `ExamSubject`), `ExamResult`, `ReportCard`.
- **Tables:** `Exams`, `ExamSchedules`, `ExamSubjects`, `ExamResults`, `Grades`, `ReportCards`.
- **Public events:** `ExamScheduledEvent`, `ExamCompletedEvent`, `ExamResultPublishedEvent`, `ReportCardGeneratedEvent`.

### 3.9 Fee Management
- **Purpose:** fee setup, invoicing, collection, refunds.
- **Aggregates:** `FeeStructure` (root with `FeeInstallment`s), `StudentFeeAssignment`, `FeeInvoice` (with `FeeInvoiceLine`s), `FeePayment`, `FeeRefund`, `FeeDiscount`.
- **Tables:** `FeeTypes`, `FeeStructures`, `FeeInstallments`, `StudentFeeAssignments`, `FeeInvoices`, `FeeInvoiceLines`, `FeePayments`, `FeeDiscounts`, `FeeRefunds`.
- **Public events:** `FeeInvoiceGeneratedEvent`, `FeePaymentReceivedEvent`, `FeePaymentFailedEvent`, `FeeRefundIssuedEvent`, `FeeOverdueEvent`.

### 3.10 Transport Management
- **Purpose:** routes, vehicles, drivers, GPS, student transport assignment.
- **Aggregates:** `Route` (with `Stop`s), `Vehicle` (with `VehicleAssignment`), `Driver`, `StudentTransportAssignment`, `GpsDevice`.
- **Tables:** `Vehicles`, `Routes`, `Stops`, `Drivers`, `VehicleAssignments`, `StudentTransportAssignments`, `GPSDevices`, `GPSPings` (high-volume time-series).
- **Public events:** `RoutePublishedEvent`, `StudentBoardedEvent`, `StudentAlightedEvent`, `VehicleSosEvent`.

### 3.11 Communication Management
- **Purpose:** templates, multi-channel notifications (email, SMS, WhatsApp, push), delivery tracking.
- **Aggregates:** `NotificationTemplate`, `Notification` (with `NotificationRecipient`s).
- **Tables:** `NotificationTemplates`, `Notifications`, `NotificationRecipients`, `EmailLogs`, `SmsLogs`, `WhatsAppLogs`, `PushLogs`.
- **Public events:** `NotificationQueuedEvent`, `NotificationSentEvent`, `NotificationFailedEvent`, `EmailSentEvent`, `SmsSentEvent`, `WhatsAppSentEvent`.

### 3.12 Library Management
- **Purpose:** catalog, copies, issue/return cycle, fines.
- **Aggregates:** `Book` (with `BookCopy`), `BookIssue`.
- **Tables:** `Books`, `BookCategories`, `BookCopies`, `BookIssues`, `BookReturns`, `BookFines`.
- **Public events:** `BookIssuedEvent`, `BookReturnedEvent`, `BookOverdueEvent`.

### 3.13 Inventory Management
- **Purpose:** assets, stock, vendors, purchase orders.
- **Aggregates:** `InventoryItem`, `Vendor`, `PurchaseOrder` (with `PurchaseOrderItem`s).
- **Tables:** `InventoryItems`, `InventoryCategories`, `Vendors`, `PurchaseOrders`, `PurchaseOrderItems`, `StockTransactions`.
- **Public events:** `PurchaseOrderRaisedEvent`, `PurchaseOrderReceivedEvent`, `StockAdjustedEvent`.

### 3.14 Audit & Logging
- **Purpose:** business audit, system audit, error capture, outbox/inbox storage, background job log.
- **Aggregates:** `AuditLog`, `ActivityLog`, `ErrorLog`, `OutboxMessage`, `InboxMessage`, `BackgroundJob`.
- **Tables:** `AuditLogs`, `ActivityLogs`, `ErrorLogs`, `BackgroundJobs`, `OutboxMessages`, `InboxMessages`.
- **Subscribes to:** every `*.exchange` via fanout binding to `schoolerp.audit.q.events`.

### 3.15 Shared Infrastructure (BuildingBlocks + SharedKernel)
- **Purpose:** base abstractions, primitives, cross-cutting infra.
- **Contains:**
  - `BuildingBlocks.Domain`: `AggregateRoot<TId>`, `Entity<TId>`, `ValueObject`, `IDomainEvent`, `BusinessRule`, `Result<T>`, `Error`.
  - `BuildingBlocks.Application`: `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, MediatR pipeline behaviors (validation, logging, transaction, caching, idempotency).
  - `BuildingBlocks.Infrastructure`: `IUnitOfWork`, outbox writer, inbox reader, RabbitMQ abstractions (`IEventBus`, `IIntegrationEventHandler<T>`), Redis cache, file storage, encryption (`IDataProtector`).
  - `BuildingBlocks.MultiTenancy`: `ITenantContext`, `TenantResolutionMiddleware`, `TenantHeaderResolver`, `TenantHostResolver`, `TenantClaimResolver`, global query filter helpers.
  - `BuildingBlocks.Authorization`: `IPermissionAuthorizationHandler`, `[RequirePermission(...)]`.
  - `BuildingBlocks.Observability`: `IActivitySource`, log enrichers (`TenantId`, `UserId`, `CorrelationId`), HealthChecks.
  - `SharedKernel`: cross-context VOs (`Money`, `Address`, `PhoneNumber`, `Email`, `DateRange`, `AcademicYearId`, `ClassId`, `SectionId`, `StudentId`).

---

## 4. Aggregate Roots

> **Rule:** one transaction = one aggregate root. Cross-aggregate consistency is achieved via integration events through Outbox.

### 4.1 Aggregate Inventory

| # | Module | Aggregate Root | Internal Entities / VOs | Invariants |
|---|--------|----------------|--------------------------|------------|
| 1 | Tenant | `Tenant` | `TenantSetting`, `TenantBranding`, `TenantDomain`, `TenantStorageSettings`, `TenantEmailSettings`, `TenantSmsSettings` | One default domain; status transitions are linear (`Pending → Active → Suspended → Deprovisioned`) |
| 2 | Identity | `User` | `UserSession`, `PasswordHistory`, `LoginHistory` | Email unique per tenant; lock-out after N failed attempts |
| 3 | Identity | `Role` | — | Role names unique per tenant |
| 4 | Identity | `Permission` | — | Globally unique name (system-defined) |
| 5 | Identity | `RefreshToken` | — | One-shot use; rotation enforced |
| 6 | Billing | `Plan` | `PlanFeature` | Active plan must have ≥ 1 feature |
| 7 | Billing | `Subscription` | `SubscriptionUsage` | Cannot overlap active subs for same tenant |
| 8 | Billing | `SubscriptionInvoice` | `SubscriptionPayment` | `Total = Subtotal + Tax − Discount` |
| 9 | Billing | `Coupon` | — | Redemption count ≤ Max |
| 10 | Academics | `AcademicYear` | `Term` | Terms non-overlapping inside AY |
| 11 | Academics | `Class` | `Section` | Section names unique within Class |
| 12 | Academics | `Subject` | `SubjectAssignment` | Assignment unique per (Section, Subject, AY) |
| 13 | Academics | `Timetable` | `Period` | No two periods overlap on same day for same section |
| 14 | Students | `Student` | `StudentAddress`, `StudentDocument`, `StudentMedicalRecord`, `StudentEmergencyContact`, `StudentStatusHistory`, `StudentPromotionHistory` | Admission no. unique per tenant |
| 15 | Students | `StudentEnrollment` | — | One *active* enrollment per (Student, AY) |
| 16 | Students | `Parent` | — | Email/phone unique per tenant |
| 17 | Employees | `Employee` | `EmployeeDocument`, `EmployeeBankAccount` | Employee no. unique per tenant |
| 18 | Employees | `Department` | — | Name unique per tenant |
| 19 | Employees | `Designation` | — | Name unique per tenant |
| 20 | Employees | `EmployeePayroll` | — | One payroll record per (Employee, Month, Year) |
| 21 | Attendance | `AttendanceSession` | (StudentAttendance is a separate aggregate for write fan-out) | One session per (Section, Date, Period) |
| 22 | Attendance | `StudentAttendance` | — | One mark per (Student, Session) |
| 23 | Attendance | `EmployeeAttendance` | — | One mark per (Employee, Date) |
| 24 | Exams | `Exam` | `ExamSchedule`, `ExamSubject` | Schedule slots non-overlapping |
| 25 | Exams | `ExamResult` | — | Marks ≤ MaxMarks per subject |
| 26 | Exams | `ReportCard` | — | One per (Student, Exam) |
| 27 | Exams | `Grade` | — | Grade ranges non-overlapping per scale |
| 28 | Fees | `FeeType` | — | Code unique per tenant |
| 29 | Fees | `FeeStructure` | `FeeInstallment` | Sum of installments == structure total |
| 30 | Fees | `StudentFeeAssignment` | — | One assignment per (Student, AY, FeeStructure) |
| 31 | Fees | `FeeInvoice` | `FeeInvoiceLine` | `Total = Σ Lines − Discount + Tax` |
| 32 | Fees | `FeePayment` | — | Sum(Payments) ≤ Invoice.Total |
| 33 | Fees | `FeeRefund` | — | Refund ≤ Σ Payments |
| 34 | Fees | `FeeDiscount` | — | Per-student or per-structure scope |
| 35 | Transport | `Vehicle` | `VehicleAssignment` | One driver active at a time |
| 36 | Transport | `Route` | `Stop` | Stop sequence is contiguous |
| 37 | Transport | `Driver` | — | License unique per tenant |
| 38 | Transport | `StudentTransportAssignment` | — | One active per (Student, AY) |
| 39 | Transport | `GpsDevice` | — | IMEI unique globally |
| 40 | Communication | `NotificationTemplate` | — | Code unique per tenant |
| 41 | Communication | `Notification` | `NotificationRecipient` | At least one recipient |
| 42 | Library | `Book` | `BookCopy` | ISBN unique per tenant |
| 43 | Library | `BookIssue` | — | Cannot issue an already-issued copy |
| 44 | Inventory | `InventoryItem` | — | SKU unique per tenant |
| 45 | Inventory | `Vendor` | — | GST/Tax id unique per tenant |
| 46 | Inventory | `PurchaseOrder` | `PurchaseOrderItem` | `Total = Σ Items` |
| 47 | Audit | `AuditLog` | — | Append-only |
| 48 | Audit | `OutboxMessage` | — | Idempotent dispatch |
| 49 | Audit | `InboxMessage` | — | Idempotent consumption |

### 4.2 Aggregate Sizing & Boundaries Rationale

- **Student is *not* the parent of Enrollment** — Enrollment changes per academic year and is read by many contexts (Attendance, Fees, Exams). Modeling it as its own root reduces lock contention on the Student aggregate.
- **AttendanceSession ≠ StudentAttendance parent** — a session can have 5,000+ rows in a multi-grade exam. Holding them in one aggregate would force giant transactions; they are co-aggregated only by FK + business rule, not by transactional boundary.
- **FeeInvoice owns FeeInvoiceLine** but not `FeePayment` — payments arrive asynchronously (PG webhooks, manual, bank); making them a separate root lets us update them without optimistic-concurrency conflicts on the invoice.
- **Tenant is small but central** — owning all *settings* keeps consistency tight (changing branding never races with changing storage settings) and is fine because tenant updates are infrequent.

### 4.3 Aggregate Diagram (selected key contexts)

```
Students Context                       Academics Context
┌──────────────────────┐                ┌──────────────────────┐
│ <<root>>  Student    │                │ <<root>>             │
│ + StudentAddress[]   │     refs       │ AcademicYear         │
│ + StudentDocument[]  │ ◄──────────►   │  + Term[]            │
│ + MedicalRecord      │                └──────────────────────┘
│ + EmergencyContact[] │                ┌──────────────────────┐
│ + StatusHistory[]    │                │ <<root>> Class       │
│ + PromotionHistory[] │                │  + Section[]         │
└─────────┬────────────┘                └──────────────────────┘
          │ 1..*                                ▲
          │                                     │
┌─────────▼─────────────┐                       │
│ <<root>>              │   refs Class/Section/AY
│ StudentEnrollment     ├───────────────────────┘
│ (per AcademicYear)    │
└───────────────────────┘

Fees Context                                Attendance Context
┌──────────────────────┐                    ┌─────────────────────┐
│ <<root>> FeeStructure│                    │ <<root>>            │
│  + FeeInstallment[]  │                    │ AttendanceSession   │
└──────────┬───────────┘                    └──────────┬──────────┘
           │                                           │ 1..*
┌──────────▼─────────────┐                  ┌──────────▼──────────┐
│ <<root>>               │                  │ <<root>>            │
│ StudentFeeAssignment   │                  │ StudentAttendance   │
└──────────┬─────────────┘                  └─────────────────────┘
           │
┌──────────▼─────────────┐
│ <<root>> FeeInvoice    │
│   + FeeInvoiceLine[]   │
└──────────┬─────────────┘
           │
┌──────────▼─────────┐    ┌─────────────────┐
│ <<root>> FeePayment│    │ <<root>> Refund │
└────────────────────┘    └─────────────────┘
```
