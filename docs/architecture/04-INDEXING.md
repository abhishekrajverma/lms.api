# 04 — Indexing Strategy

> Goal: every hot read in the system should be served by an **index seek + a few RID lookups**, never a scan. Every "list X for tenant T" query must use a tenant-leading composite index. High-volume tables (Attendance, Invoices, Audit, GPS) are partitioned and indexed accordingly.

## 13. Indexing Strategy

### 13.1 Principles

1. **Tenant column always leads.** Every NC index for a tenant-scoped table starts with `TenantId`. This guarantees a seek even when statistics are skewed and prevents accidental cross-tenant scans.
2. **Filtered indexes for soft delete.** Most reads are `WHERE IsDeleted = 0`. We add `WHERE IsDeleted = 0` to large indexes to halve their size and skip the predicate at runtime.
3. **Cover the hot path.** For pages that show "lists with badges" (e.g., students with status, attendance %, fee due), we add `INCLUDE` columns rather than forcing key lookups.
4. **Prefer composite over multiple single-column indexes.** SQL Server can intersect indexes but it's costly; composite indexes on `(TenantId, X, Y)` are far more selective.
5. **Partition aligned indexes** for `StudentAttendance`, `FeeInvoices`, `AuditLogs`, `GPSPings` so partition switching at archival time is metadata-only.
6. **Ascending date columns are tail-heavy.** Use a `DESC` order for "recent first" indexes (e.g., `MarkedAtUtc DESC`) where the UI sorts newest first.
7. **Don't over-index OLTP tables.** Each index slows down `INSERT`/`UPDATE`. Aim for ≤ 6 NC indexes per write-heavy table; merge similar predicates into covering indexes.
8. **Use columnstore on warm/cold archives.** After 12 months, partitions are switched to a columnstore-only history table for analytics.

### 13.2 Partition Functions and Schemes

```sql
-- Monthly partitions for high-volume tables
CREATE PARTITION FUNCTION PF_Att_ByMonth (DATE)
AS RANGE RIGHT FOR VALUES (
    '2024-01-01','2024-02-01','2024-03-01', ... ,'2030-12-01'
);

CREATE PARTITION SCHEME PS_Att_ByMonth
AS PARTITION PF_Att_ByMonth ALL TO ([PRIMARY]);  -- map to filegroups in real deploy
GO

CREATE PARTITION FUNCTION PF_Audit_ByMonth (DATE) AS RANGE RIGHT FOR VALUES ( ... );
CREATE PARTITION SCHEME  PS_Audit_ByMonth AS PARTITION PF_Audit_ByMonth ALL TO ([PRIMARY]);
GO

CREATE PARTITION FUNCTION PF_Fees_ByYear (DATE) AS RANGE RIGHT FOR VALUES (
    '2024-04-01','2025-04-01','2026-04-01','2027-04-01','2028-04-01'
);
CREATE PARTITION SCHEME  PS_Fees_ByYear AS PARTITION PF_Fees_ByYear ALL TO ([PRIMARY]);
GO

CREATE PARTITION FUNCTION PF_Gps_ByMonth (DATE) AS RANGE RIGHT FOR VALUES ( ... );
CREATE PARTITION SCHEME  PS_Gps_ByMonth AS PARTITION PF_Gps_ByMonth ALL TO ([PRIMARY]);
GO
```

### 13.3 Index Catalog (rationale per index)

#### Tenant Module
```sql
CREATE UNIQUE NONCLUSTERED INDEX UX_Tenants_Code
    ON tenant.Tenants (Code)
    WHERE IsDeleted = 0;
-- WHY: tenant resolution by code is on the auth hot path; filtered to skip tombstones.

CREATE NONCLUSTERED INDEX IX_Tenants_Status
    ON tenant.Tenants (Status, IsDeleted)
    INCLUDE (Name, PlanId);
-- WHY: admin lists ("Active tenants on Plan X") use this.

CREATE UNIQUE NONCLUSTERED INDEX UX_TenantDomains_Host
    ON tenant.TenantDomains (Host)
    WHERE IsDeleted = 0;
-- WHY: domain → tenant lookup is per-request from the host header.
```

#### Identity Module
```sql
CREATE UNIQUE NONCLUSTERED INDEX UX_Users_Tenant_Email
    ON identity_.Users (TenantId, NormalizedEmail)
    WHERE IsDeleted = 0;
-- WHY: login by email; tenant is known from host. Unique inside tenant.

CREATE UNIQUE NONCLUSTERED INDEX UX_Users_Tenant_UserName
    ON identity_.Users (TenantId, NormalizedUserName)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Users_Tenant_Type_Active
    ON identity_.Users (TenantId, UserType, IsActive)
    INCLUDE (Email, FirstName, LastName, LastLoginAtUtc)
    WHERE IsDeleted = 0;
-- WHY: admin "list staff/parents/students" pages.

CREATE UNIQUE NONCLUSTERED INDEX UX_RefreshTokens_Hash
    ON identity_.RefreshTokens (TokenHash)
    WHERE RevokedAtUtc IS NULL;
-- WHY: token rotation lookup; filtered to skip revoked.

CREATE NONCLUSTERED INDEX IX_RefreshTokens_User_Exp
    ON identity_.RefreshTokens (TenantId, UserId, ExpiresAtUtc DESC);
-- WHY: cleanup job + "active sessions" UI.

CREATE NONCLUSTERED INDEX IX_LoginHistory_Tenant_Date
    ON identity_.LoginHistory (TenantId, OccurredAtUtc DESC)
    INCLUDE (UserId, Success, IpAddress);
-- WHY: security audit "recent logins for tenant".
```

#### Billing Module
```sql
CREATE NONCLUSTERED INDEX IX_Subscriptions_Tenant_Status
    ON billing.Subscriptions (TenantId, Status)
    INCLUDE (PlanId, CurrentPeriodEndUtc)
    WHERE IsDeleted = 0;

CREATE UNIQUE NONCLUSTERED INDEX UX_Subscriptions_Active
    ON billing.Subscriptions (TenantId)
    WHERE Status IN (0,1) AND IsDeleted = 0;
-- WHY: at most one active/trialing subscription per tenant; a filtered unique index enforces the rule cheaply.

CREATE NONCLUSTERED INDEX IX_Subscriptions_PeriodEnd
    ON billing.Subscriptions (CurrentPeriodEndUtc, Status)
    WHERE Status IN (0,1);
-- WHY: nightly billing job: "subs whose period ended yesterday".

CREATE NONCLUSTERED INDEX IX_SubInv_Status_Due
    ON billing.SubscriptionInvoices (TenantId, Status, DueAtUtc)
    INCLUDE (Total, AmountPaid);
-- WHY: dunning report.

CREATE NONCLUSTERED INDEX IX_WebhookEvents_Status
    ON billing.WebhookEvents (Status, ReceivedAtUtc)
    WHERE Status IN (0,2);
-- WHY: retry job picks up unprocessed/failed events.
```

#### Academic Module
```sql
CREATE NONCLUSTERED INDEX IX_AY_Tenant_Current
    ON academic.AcademicYears (TenantId, IsCurrent)
    WHERE IsDeleted = 0 AND IsCurrent = 1;
-- WHY: most queries first resolve "current academic year" — keep that index tiny.

CREATE NONCLUSTERED INDEX IX_Classes_Tenant_AY
    ON academic.Classes (TenantId, AcademicYearId, GradeLevel)
    INCLUDE (Name, DisplayOrder)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Sections_Tenant_Class
    ON academic.Sections (TenantId, ClassId)
    INCLUDE (Name, Capacity, ClassTeacherEmployeeId)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Periods_Section_Day
    ON academic.Periods (TenantId, TimetableId, DayOfWeek, PeriodNumber)
    INCLUDE (StartTime, EndTime, SubjectId, TeacherEmployeeId);
-- WHY: rendering weekly timetable for a section.
```

#### Student Module
```sql
CREATE UNIQUE NONCLUSTERED INDEX UX_Students_Adm
    ON student.Students (TenantId, AdmissionNumber)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Students_Tenant_Status
    ON student.Students (TenantId, Status, IsDeleted)
    INCLUDE (FirstName, LastName, AdmissionNumber);
-- WHY: status-filtered student lists ("Active students on Plan X").

CREATE NONCLUSTERED INDEX IX_Students_Name
    ON student.Students (TenantId, LastName, FirstName)
    INCLUDE (AdmissionNumber, Status, PhotoBlobKey)
    WHERE IsDeleted = 0;
-- WHY: search/sort by name on lists.

CREATE NONCLUSTERED INDEX IX_StudentEnroll_Section_AY
    ON student.StudentEnrollments (TenantId, AcademicYearId, ClassId, SectionId)
    INCLUDE (StudentId, RollNumber, Status)
    WHERE IsDeleted = 0;
-- WHY: "list students of section X in current AY" is the single most frequent query in the system.

CREATE UNIQUE NONCLUSTERED INDEX UX_StudentEnroll_Active
    ON student.StudentEnrollments (TenantId, StudentId, AcademicYearId)
    WHERE IsDeleted = 0;
-- WHY: enforce one enrollment per student per AY.

CREATE NONCLUSTERED INDEX IX_StudentParents_Parent
    ON student.StudentParents (TenantId, ParentId)
    INCLUDE (StudentId, IsPrimary);
```

#### Employee Module
```sql
CREATE UNIQUE NONCLUSTERED INDEX UX_Emp_Number
    ON employee.Employees (TenantId, EmployeeNumber)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Emp_Dept_Status
    ON employee.Employees (TenantId, DepartmentId, Status)
    INCLUDE (FirstName, LastName, DesignationId);

CREATE NONCLUSTERED INDEX IX_EmpAttendance_Date
    ON employee.EmployeeAttendance (TenantId, AttendanceDate, Status)
    INCLUDE (EmployeeId, HoursWorked);
```

#### Attendance Module (highest write volume)
```sql
-- Clustered PK is partition-aligned: (SessionDate, TenantId, SessionId, StudentId)
-- which is monotonically increasing with daily inserts.

CREATE NONCLUSTERED INDEX IX_StudentAttendance_Student
    ON attendance.StudentAttendance (TenantId, StudentId, SessionDate DESC)
    INCLUDE (Status, SessionId)
    ON PS_Att_ByMonth(SessionDate);
-- WHY: parent dashboard "my child's attendance for last 30 days" — uses partition elimination + ordered index.

CREATE NONCLUSTERED INDEX IX_StudentAttendance_Section_Date
    ON attendance.StudentAttendance (TenantId, SessionId)
    INCLUDE (StudentId, Status, MarkedAtUtc)
    ON PS_Att_ByMonth(SessionDate);
-- WHY: "show all marks for session" — class teacher scoring page.

CREATE NONCLUSTERED INDEX IX_AttSessions_Tenant_Date
    ON attendance.AttendanceSessions (TenantId, SessionDate, Status)
    INCLUDE (SectionId, PeriodId);
-- WHY: dashboard "today's open sessions".
```

#### Examination Module
```sql
CREATE NONCLUSTERED INDEX IX_Exams_Tenant_AY_Status
    ON exam.Exams (TenantId, AcademicYearId, Status)
    INCLUDE (Name, StartDate, EndDate);

CREATE NONCLUSTERED INDEX IX_ExamSchedules_Date
    ON exam.ExamSchedules (TenantId, ExamDate, ClassId)
    INCLUDE (SectionId, StartTime, EndTime, RoomCode);

CREATE NONCLUSTERED INDEX IX_ExamResults_Exam_Student
    ON exam.ExamResults (TenantId, ExamId, StudentId)
    INCLUDE (ExamSubjectId, MarksObtained, GradeId, IsAbsent);
-- WHY: report card generation reads all subject results for one (Exam, Student).

CREATE NONCLUSTERED INDEX IX_ExamResults_Student_Hist
    ON exam.ExamResults (TenantId, StudentId, ExamId)
    INCLUDE (ExamSubjectId, MarksObtained);
-- WHY: parent dashboard "all exams of my child".

CREATE NONCLUSTERED INDEX IX_ReportCards_Status
    ON exam.ReportCards (TenantId, ExamId, Result)
    INCLUDE (StudentId, Percentage, Rank);
```

#### Fees Module (heavy reads + writes, partitioned by IssuedOn)
```sql
CREATE NONCLUSTERED INDEX IX_FInv_Status_Due
    ON fees.FeeInvoices (TenantId, Status, DueOn)
    INCLUDE (StudentId, Total, AmountDue)
    WHERE IsDeleted = 0
    ON PS_Fees_ByYear(IssuedOn);
-- WHY: dunning report, "overdue invoices for tenant", uses partition pruning by year + status.

CREATE NONCLUSTERED INDEX IX_FInv_Student
    ON fees.FeeInvoices (TenantId, StudentId, IssuedOn DESC)
    INCLUDE (Number, Status, Total, AmountDue)
    ON PS_Fees_ByYear(IssuedOn);
-- WHY: parent dashboard "my child's invoices".

CREATE NONCLUSTERED INDEX IX_FPay_Inv
    ON fees.FeePayments (TenantId, InvoiceId)
    INCLUDE (Amount, Status, ReceivedAtUtc);
-- WHY: invoice page "show payments".

CREATE NONCLUSTERED INDEX IX_FPay_Status_Date
    ON fees.FeePayments (TenantId, Status, ReceivedAtUtc DESC)
    INCLUDE (InvoiceId, Amount, Method);
-- WHY: collection reports.

CREATE UNIQUE NONCLUSTERED INDEX UX_FPay_Gateway
    ON fees.FeePayments (GatewayProvider, GatewayPaymentId)
    WHERE GatewayPaymentId IS NOT NULL;
-- WHY: webhook idempotency — prevent double-credit on retried PG webhooks.
```

#### Transport Module
```sql
CREATE NONCLUSTERED INDEX IX_Stops_Route_Seq
    ON transport.Stops (TenantId, RouteId, Sequence)
    INCLUDE (Name, Latitude, Longitude, PickupTime, DropTime);

CREATE NONCLUSTERED INDEX IX_StudentTransport_Student
    ON transport.StudentTransportAssignments (TenantId, StudentId, AcademicYearId)
    INCLUDE (RouteId, PickupStopId, DropStopId, Status);

-- GPSPings: clustered PK already partition-aligned; add seek for "vehicle history"
CREATE NONCLUSTERED INDEX IX_GPSPings_Vehicle
    ON transport.GPSPings (TenantId, VehicleId, PingedAtUtc DESC)
    INCLUDE (Latitude, Longitude, SpeedKph)
    ON PS_Gps_ByMonth(PingDate);
```

#### Communication Module
```sql
CREATE NONCLUSTERED INDEX IX_Notifications_Status
    ON comm.Notifications (TenantId, Status, CreatedAt DESC)
    INCLUDE (TemplateId, Channel)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_NotifRecip_Status
    ON comm.NotificationRecipients (TenantId, Status, NotificationId)
    INCLUDE (Address, SentAtUtc, FailureReason);
-- WHY: "show failed recipients of notification X".

-- Provider message id lookups (webhook DLR updates):
CREATE NONCLUSTERED INDEX IX_EmailLogs_Provider
    ON comm.EmailLogs (Provider, ProviderMessageId)
    WHERE ProviderMessageId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_SmsLogs_Provider
    ON comm.SmsLogs (Provider, ProviderMessageId)
    WHERE ProviderMessageId IS NOT NULL;
```

#### Library + Inventory
```sql
CREATE NONCLUSTERED INDEX IX_BookCopies_Status
    ON library_.BookCopies (TenantId, BookId, Status)
    INCLUDE (AccessionNumber);

CREATE NONCLUSTERED INDEX IX_BookIssues_Borrower
    ON library_.BookIssues (TenantId, BorrowerType, BorrowerId, Status)
    INCLUDE (CopyId, IssuedOn, DueOn);

CREATE NONCLUSTERED INDEX IX_InvItems_Reorder
    ON inventory.InventoryItems (TenantId)
    INCLUDE (Sku, StockOnHand, ReorderLevel)
    WHERE IsDeleted = 0;
-- "items below reorder level" report.

CREATE NONCLUSTERED INDEX IX_PO_Status_Date
    ON inventory.PurchaseOrders (TenantId, Status, OrderDate DESC)
    INCLUDE (Number, VendorId, Total);
```

#### Audit / Outbox / Inbox
```sql
-- Outbox processor must never table-scan. The PK is on Identity (sequential int)
-- but the worker query is "SELECT TOP 200 ... WHERE Status = 0 AND AvailableAtUtc <= now()".
CREATE NONCLUSTERED INDEX IX_Outbox_Pending
    ON audit_.OutboxMessages (Status, AvailableAtUtc)
    INCLUDE (TenantId, Exchange, RoutingKey, EventType, Payload, Headers, Attempts)
    WHERE Status IN (0, 3);  -- Pending / Failed
-- WHY: covering index lets the worker page through pending messages without back-key lookups.

CREATE NONCLUSTERED INDEX IX_Outbox_Tenant_Time
    ON audit_.OutboxMessages (TenantId, OccurredOnUtc DESC)
    INCLUDE (Status, EventType);
-- WHY: per-tenant diagnostic UI.

-- Inbox idempotency check is on (Consumer, MessageId). The PK already covers Consumer-only
-- reads via the unique index UX_IB_Idem.

CREATE NONCLUSTERED INDEX IX_Inbox_Pending
    ON audit_.InboxMessages (Status, ReceivedAtUtc)
    INCLUDE (TenantId, EventType, Module, Attempts)
    WHERE Status IN (0, 1, 3);

CREATE NONCLUSTERED INDEX IX_Audit_EntityLookup
    ON audit_.AuditLogs (TenantId, EntityType, EntityId, OccurredAtUtc DESC)
    ON PS_Audit_ByMonth(AuditDate);
-- WHY: "history of changes for record X".
```

### 13.4 Filtered Indexes Summary

| Filter | Tables | Reason |
|--------|--------|--------|
| `IsDeleted = 0` | Most business tables | Skip soft-deleted rows; reduces index size by ~5-10% |
| `Status IN (0,1)` | `Subscriptions`, `Outbox`, `Inbox` | Targeted queries always filter on these statuses |
| `Status IN (0,3)` | `Outbox`, `Inbox` | Worker only cares about Pending and Failed |
| `RevokedAtUtc IS NULL` | `RefreshTokens` | Active tokens only |
| `IsCurrent = 1` | `AcademicYears` | Single row per tenant becomes a 1-row index |
| `GatewayPaymentId IS NOT NULL` | Payments | Webhook idempotency only matters for online PG payments |

### 13.5 Columnstore (Analytics)

```sql
-- Cold partitions of attendance, after 12 months, switched to:
CREATE TABLE attendance.StudentAttendance_Archive (
    -- same shape as live table
);
CREATE CLUSTERED COLUMNSTORE INDEX CCI_StudentAttendance_Archive
    ON attendance.StudentAttendance_Archive
    WITH (DATA_COMPRESSION = COLUMNSTORE_ARCHIVE);

-- Analytics queries (per-class %, year-over-year) hit the archive table from a
-- read-only replica; live OLTP only touches recent partitions.
```

### 13.6 Index Maintenance

| Cadence | Action | Owner |
|---------|--------|-------|
| Daily | `sys.dm_db_index_usage_stats` snapshot to `audit_.IndexUsageHistory` | Hangfire `IndexUsageSnapshotJob` |
| Weekly | `ALTER INDEX REORGANIZE` on indexes 5–30 % fragmented | DBA agent job |
| Weekly | `ALTER INDEX REBUILD WITH (ONLINE = ON, MAXDOP = 4)` on > 30 % fragmented | DBA agent job |
| Weekly | Update statistics with `FULLSCAN` on tables > 10 GB | DBA agent job |
| Monthly | Review `sys.dm_db_missing_index_details` + `sys.dm_db_index_usage_stats` to add/drop | DB platform team |

### 13.7 Anti-Patterns We Avoid

- **No `WITH (NOLOCK)` hints**. We use `READ COMMITTED SNAPSHOT` on the database to get non-blocking consistent reads.
- **No GUID-clustered tables without sequential GUIDs**. We use `NEWSEQUENTIALID()` for inserts; truly random GUIDs would fragment heavily.
- **No `IDENTITY` PKs on cross-shard tables**. We use sequential GUIDs so future sharding is transparent.
- **No nullable columns in composite key positions** (SQL Server forbids it on PK; we extend the rule to unique indexes).
- **No leading `IsDeleted` in indexes** — it has only two distinct values; not selective. Use it as a *filter*, not a key.
