# 03 — Keys, Foreign Keys, Relationship Explanations, Cardinality Matrix

## 9. Primary Keys

### 9.1 PK Strategy

| Table category | PK Type | Rationale |
|----------------|---------|-----------|
| Reference / aggregate roots | `UNIQUEIDENTIFIER` (sequential GUID via `NEWSEQUENTIALID()`) | Distributed-friendly, avoids hot pages, hides volume to clients |
| Composite child within tenant | `(TenantId, <ChildId>)` | All reads/writes carry TenantId; short index entries when joined with parent |
| Append-only / high-volume ledgers (`StudentAttendance`, `OutboxMessages`, `InboxMessages`, `AuditLogs`, `EmailLogs`, `SmsLogs`, `WhatsAppLogs`, `PushLogs`, `GPSPings`, `StockTransactions`, `LoginHistory`) | `BIGINT IDENTITY(1,1)` | Compact, sequential — minimizes page splits at write rate; never exposed externally |
| Partitioned tables (`StudentAttendance`, `FeeInvoices`, `AuditLogs`, `GPSPings`) | Composite clustered PK leading with **partition column** (`SessionDate`, `IssuedOn`, `AuditDate`, `PingDate`) | SQL Server partitioned indexes require partition column in the clustered key |

### 9.2 Catalog of Primary Keys

| # | Schema | Table | Primary Key |
|---|--------|-------|-------------|
| 1 | `tenant` | `Tenants` | `TenantId` |
| 2 | `tenant` | `TenantSettings` | `(TenantId, TenantSettingId)` |
| 3 | `tenant` | `TenantBranding` | `(TenantId, TenantBrandingId)` |
| 4 | `tenant` | `TenantDomains` | `(TenantId, TenantDomainId)` |
| 5 | `tenant` | `TenantStorageSettings` | `(TenantId, SettingId)` |
| 6 | `tenant` | `TenantEmailSettings` | `(TenantId, SettingId)` |
| 7 | `tenant` | `TenantSmsSettings` | `(TenantId, SettingId)` |
| 8 | `identity_` | `Users` | `(TenantId, UserId)` |
| 9 | `identity_` | `Roles` | `(TenantId, RoleId)` |
| 10 | `identity_` | `Permissions` | `PermissionId` |
| 11 | `identity_` | `UserRoles` | `(TenantId, UserId, RoleId)` |
| 12 | `identity_` | `RolePermissions` | `(TenantId, RoleId, PermissionId)` |
| 13 | `identity_` | `RefreshTokens` | `(TenantId, RefreshTokenId)` |
| 14 | `identity_` | `LoginHistory` | `(TenantId, OccurredAtUtc, LoginHistoryId)` |
| 15 | `identity_` | `PasswordHistory` | `(TenantId, UserId, ChangedAtUtc)` |
| 16 | `identity_` | `UserSessions` | `(TenantId, UserId, SessionId)` |
| 17 | `billing` | `Plans` | `PlanId` |
| 18 | `billing` | `Features` | `FeatureId` |
| 19 | `billing` | `PlanFeatures` | `(PlanId, FeatureId)` |
| 20 | `billing` | `Subscriptions` | `(TenantId, SubscriptionId)` |
| 21 | `billing` | `SubscriptionInvoices` | `(TenantId, InvoiceId)` |
| 22 | `billing` | `SubscriptionPayments` | `(TenantId, PaymentId)` |
| 23 | `billing` | `SubscriptionUsage` | `(TenantId, SubscriptionId, FeatureCode, PeriodStartUtc)` |
| 24 | `billing` | `Coupons` | `CouponId` |
| 25 | `billing` | `Refunds` | `(TenantId, RefundId)` |
| 26 | `billing` | `WebhookEvents` | `WebhookEventId` |
| 27 | `billing` | `BillingAddresses` | `(TenantId, BillingAddressId)` |
| 28 | `billing` | `PaymentMethods` | `(TenantId, PaymentMethodId)` |
| 29 | `billing` | `TaxConfigurations` | `TaxConfigId` |
| 30 | `academic` | `AcademicYears` | `(TenantId, AcademicYearId)` |
| 31 | `academic` | `Terms` | `(TenantId, TermId)` |
| 32 | `academic` | `Classes` | `(TenantId, ClassId)` |
| 33 | `academic` | `Sections` | `(TenantId, SectionId)` |
| 34 | `academic` | `Subjects` | `(TenantId, SubjectId)` |
| 35 | `academic` | `SubjectAssignments` | `(TenantId, SubjectAssignmentId)` |
| 36 | `academic` | `Timetables` | `(TenantId, TimetableId)` |
| 37 | `academic` | `Periods` | `(TenantId, PeriodId)` |
| 38 | `academic` | `ClassTeachers` | `(TenantId, ClassTeacherId)` |
| 39 | `student` | `Parents` | `(TenantId, ParentId)` |
| 40 | `student` | `Students` | `(TenantId, StudentId)` |
| 41 | `student` | `StudentEnrollments` | `(TenantId, EnrollmentId)` |
| 42 | `student` | `StudentParents` | `(TenantId, StudentId, ParentId)` |
| 43 | `student` | `StudentAddresses` | `(TenantId, StudentId, AddressId)` |
| 44 | `student` | `StudentDocuments` | `(TenantId, StudentId, DocumentId)` |
| 45 | `student` | `StudentMedicalRecords` | `(TenantId, StudentId, MedicalRecordId)` |
| 46 | `student` | `StudentEmergencyContacts` | `(TenantId, StudentId, ContactId)` |
| 47 | `student` | `StudentPromotionHistory` | `(TenantId, StudentId, PromotionId)` |
| 48 | `student` | `StudentStatusHistory` | `(TenantId, StudentId, StatusHistoryId)` |
| 49 | `employee` | `Departments` | `(TenantId, DepartmentId)` |
| 50 | `employee` | `Designations` | `(TenantId, DesignationId)` |
| 51 | `employee` | `Employees` | `(TenantId, EmployeeId)` |
| 52 | `employee` | `EmployeeDocuments` | `(TenantId, EmployeeId, DocumentId)` |
| 53 | `employee` | `EmployeeAttendance` | `(TenantId, EmployeeId, AttendanceDate)` |
| 54 | `employee` | `EmployeePayroll` | `(TenantId, EmployeeId, PayrollYear, PayrollMonth)` |
| 55 | `employee` | `EmployeeBankAccounts` | `(TenantId, EmployeeId, BankAccountId)` |
| 56 | `attendance` | `AttendanceSessions` | `(TenantId, SessionDate, SectionId, SessionId)` |
| 57 | `attendance` | `StudentAttendance` | `(SessionDate, TenantId, SessionId, StudentId)` (partitioned) |
| 58 | `exam` | `Grades` | `(TenantId, GradeId)` |
| 59 | `exam` | `Exams` | `(TenantId, ExamId)` |
| 60 | `exam` | `ExamSchedules` | `(TenantId, ExamScheduleId)` |
| 61 | `exam` | `ExamSubjects` | `(TenantId, ExamSubjectId)` |
| 62 | `exam` | `ExamResults` | `(TenantId, ExamResultId)` |
| 63 | `exam` | `ReportCards` | `(TenantId, ReportCardId)` |
| 64 | `fees` | `FeeTypes` | `(TenantId, FeeTypeId)` |
| 65 | `fees` | `FeeStructures` | `(TenantId, FeeStructureId)` |
| 66 | `fees` | `FeeInstallments` | `(TenantId, InstallmentId)` |
| 67 | `fees` | `StudentFeeAssignments` | `(TenantId, AssignmentId)` |
| 68 | `fees` | `FeeInvoices` | `(IssuedOn, TenantId, InvoiceId)` (partitioned) |
| 69 | `fees` | `FeeInvoiceLines` | `(IssuedOn, TenantId, InvoiceId, LineId)` (partitioned) |
| 70 | `fees` | `FeePayments` | `(TenantId, PaymentId)` |
| 71 | `fees` | `FeeDiscounts` | `(TenantId, DiscountId)` |
| 72 | `fees` | `FeeRefunds` | `(TenantId, RefundId)` |
| 73 | `transport` | `Vehicles` | `(TenantId, VehicleId)` |
| 74 | `transport` | `Drivers` | `(TenantId, DriverId)` |
| 75 | `transport` | `Routes` | `(TenantId, RouteId)` |
| 76 | `transport` | `Stops` | `(TenantId, StopId)` |
| 77 | `transport` | `VehicleAssignments` | `(TenantId, AssignmentId)` |
| 78 | `transport` | `StudentTransportAssignments` | `(TenantId, StudentTransportId)` |
| 79 | `transport` | `GPSDevices` | `(TenantId, GpsDeviceId)` |
| 80 | `transport` | `GPSPings` | `(PingDate, TenantId, GpsDeviceId, GpsPingId)` (partitioned) |
| 81 | `comm` | `NotificationTemplates` | `(TenantId, TemplateId)` |
| 82 | `comm` | `Notifications` | `(TenantId, NotificationId)` |
| 83 | `comm` | `NotificationRecipients` | `(TenantId, NotificationId, RecipientId)` |
| 84 | `comm` | `EmailLogs` | `(TenantId, SentAtUtc, EmailLogId)` |
| 85 | `comm` | `SmsLogs` | `(TenantId, SentAtUtc, SmsLogId)` |
| 86 | `comm` | `WhatsAppLogs` | `(TenantId, SentAtUtc, WhatsAppLogId)` |
| 87 | `comm` | `PushLogs` | `(TenantId, SentAtUtc, PushLogId)` |
| 88 | `library_` | `BookCategories` | `(TenantId, CategoryId)` |
| 89 | `library_` | `Books` | `(TenantId, BookId)` |
| 90 | `library_` | `BookCopies` | `(TenantId, CopyId)` |
| 91 | `library_` | `BookIssues` | `(TenantId, IssueId)` |
| 92 | `library_` | `BookReturns` | `(TenantId, ReturnId)` |
| 93 | `library_` | `BookFines` | `(TenantId, FineId)` |
| 94 | `inventory` | `InventoryCategories` | `(TenantId, CategoryId)` |
| 95 | `inventory` | `InventoryItems` | `(TenantId, ItemId)` |
| 96 | `inventory` | `Vendors` | `(TenantId, VendorId)` |
| 97 | `inventory` | `PurchaseOrders` | `(TenantId, PurchaseOrderId)` |
| 98 | `inventory` | `PurchaseOrderItems` | `(TenantId, PurchaseOrderId, PoItemId)` |
| 99 | `inventory` | `StockTransactions` | `(TenantId, ItemId, OccurredAtUtc, TransactionId)` |
| 100 | `audit_` | `AuditLogs` | `(AuditDate, TenantId, AuditLogId)` (partitioned) |
| 101 | `audit_` | `ActivityLogs` | `(TenantId, OccurredAtUtc, ActivityLogId)` |
| 102 | `audit_` | `ErrorLogs` | `(OccurredAtUtc, ErrorLogId)` |
| 103 | `audit_` | `OutboxMessages` | `OutboxMessageId` |
| 104 | `audit_` | `InboxMessages` | `InboxMessageId` |
| 105 | `audit_` | `BackgroundJobs` | `BackgroundJobId` |

---

## 10. Foreign Keys

> **Convention:** `FK_<short>_<target>`. Cross-context FKs use `ON DELETE NO ACTION` (deletion is logical via `IsDeleted`). Composite FKs always include `TenantId` first to keep them inside the tenant boundary.

### 10.1 FK Catalog (representative — see DDL for full set)

| FK | From → To | On Delete | Notes |
|----|-----------|-----------|-------|
| `FK_Users_Tenant` | `identity_.Users(TenantId)` → `tenant.Tenants(TenantId)` | NO ACTION | Tenant lifecycle gates everything |
| `FK_UR_User` | `identity_.UserRoles(TenantId, UserId)` → `identity_.Users` | CASCADE (logical) | Removing a user removes role mappings |
| `FK_UR_Role` | `identity_.UserRoles(TenantId, RoleId)` → `identity_.Roles` | NO ACTION | Roles deleted only if no users mapped |
| `FK_RP_Role` | `identity_.RolePermissions(TenantId, RoleId)` → `identity_.Roles` | CASCADE (logical) | |
| `FK_RP_Perm` | `identity_.RolePermissions(PermissionId)` → `identity_.Permissions` | NO ACTION | System data |
| `FK_RT_User` | `identity_.RefreshTokens(TenantId, UserId)` → `identity_.Users` | CASCADE | Tokens cleaned with user |
| `FK_Sub_Tenant` | `billing.Subscriptions(TenantId)` → `tenant.Tenants` | NO ACTION | |
| `FK_Sub_Plan` | `billing.Subscriptions(PlanId)` → `billing.Plans` | NO ACTION | |
| `FK_SI_Sub` | `billing.SubscriptionInvoices(TenantId, SubscriptionId)` → `billing.Subscriptions` | NO ACTION | Invoices retained even if sub cancelled |
| `FK_SP_Inv` | `billing.SubscriptionPayments(TenantId, InvoiceId)` → `billing.SubscriptionInvoices` | NO ACTION | |
| `FK_AY` | `academic.Terms(TenantId, AcademicYearId)` → `academic.AcademicYears` | NO ACTION | |
| `FK_Cls_AY` | `academic.Classes(TenantId, AcademicYearId)` → `academic.AcademicYears` | NO ACTION | |
| `FK_Sec_Cls` | `academic.Sections(TenantId, ClassId)` → `academic.Classes` | NO ACTION | |
| `FK_SE_Stu` | `student.StudentEnrollments(TenantId, StudentId)` → `student.Students` | NO ACTION | |
| `FK_SE_AY` | `student.StudentEnrollments(TenantId, AcademicYearId)` → `academic.AcademicYears` | NO ACTION | |
| `FK_SE_Cls` | `student.StudentEnrollments(TenantId, ClassId)` → `academic.Classes` | NO ACTION | |
| `FK_SE_Sec` | `student.StudentEnrollments(TenantId, SectionId)` → `academic.Sections` | NO ACTION | |
| `FK_SPx_Stu` | `student.StudentParents(TenantId, StudentId)` → `student.Students` | CASCADE (logical) | |
| `FK_SPx_Par` | `student.StudentParents(TenantId, ParentId)` → `student.Parents` | NO ACTION | |
| `FK_SD_Stu`, `FK_SAD_Stu`, `FK_SMR_Stu`, `FK_SEC_Stu`, `FK_SSH_Stu`, `FK_SPH_Stu` | Student child tables → `student.Students` | NO ACTION (CASCADE logical) | |
| `FK_AS_Sec` | `attendance.AttendanceSessions(TenantId, SectionId)` → `academic.Sections` | NO ACTION | |
| `FK_SA_Sess` | `attendance.StudentAttendance(TenantId, SessionDate, SessionId)` → `attendance.AttendanceSessions` | NO ACTION | Composite to enable partition alignment |
| `FK_E_AY` | `exam.Exams(TenantId, AcademicYearId)` → `academic.AcademicYears` | NO ACTION | |
| `FK_ES_E` | `exam.ExamSchedules(TenantId, ExamId)` → `exam.Exams` | NO ACTION | |
| `FK_ESu_Sched` | `exam.ExamSubjects(TenantId, ExamScheduleId)` → `exam.ExamSchedules` | NO ACTION | |
| `FK_ER_E`, `FK_ER_Stu`, `FK_ER_ESu` | `exam.ExamResults` → `Exams`/`Students`/`ExamSubjects` | NO ACTION | |
| `FK_RC_E`, `FK_RC_Stu` | `exam.ReportCards` → `Exams`/`Students` | NO ACTION | |
| `FK_FS_AY`, `FK_FS_Cls` | `fees.FeeStructures` → `AcademicYears`/`Classes` | NO ACTION | |
| `FK_FI_FS`, `FK_FI_FT` | `fees.FeeInstallments` → `FeeStructures`/`FeeTypes` | NO ACTION | |
| `FK_SFA_Stu`, `FK_SFA_FS` | `fees.StudentFeeAssignments` → `Students`/`FeeStructures` | NO ACTION | |
| `FK_FInv_Stu` | `fees.FeeInvoices` → `student.Students` | NO ACTION | |
| `FK_FIL_Inv` | `fees.FeeInvoiceLines(IssuedOn, TenantId, InvoiceId)` → `fees.FeeInvoices` | NO ACTION | Composite (partition-aligned) |
| `FK_FP_Inv` | `fees.FeePayments(IssuedOn, TenantId, InvoiceId)` → `fees.FeeInvoices` | NO ACTION | Carries `IssuedOn` denormalized |
| `FK_FR_FP` | `fees.FeeRefunds(TenantId, PaymentId)` → `fees.FeePayments` | NO ACTION | |
| `FK_S_RT` | `transport.Stops(TenantId, RouteId)` → `transport.Routes` | NO ACTION | |
| `FK_VA_V`, `FK_VA_RT`, `FK_VA_D` | `transport.VehicleAssignments` → `Vehicles`/`Routes`/`Drivers` | NO ACTION | |
| `FK_ST_Stu`, `FK_ST_RT` | `transport.StudentTransportAssignments` → `Students`/`Routes` | NO ACTION | |
| `FK_GD_V` | `transport.GPSDevices(TenantId, VehicleId)` → `transport.Vehicles` | NO ACTION | |
| `FK_N_T` | `comm.Notifications(TenantId, TemplateId)` → `comm.NotificationTemplates` | SET NULL | Template can be deleted, notification kept |
| `FK_NR_N` | `comm.NotificationRecipients(TenantId, NotificationId)` → `comm.Notifications` | CASCADE (logical) | |
| `FK_BCp_B` | `library_.BookCopies(TenantId, BookId)` → `library_.Books` | NO ACTION | |
| `FK_BI_BCp` | `library_.BookIssues(TenantId, CopyId)` → `library_.BookCopies` | NO ACTION | |
| `FK_BR_BI` | `library_.BookReturns(TenantId, IssueId)` → `library_.BookIssues` | NO ACTION | |
| `FK_BF_BI` | `library_.BookFines(TenantId, IssueId)` → `library_.BookIssues` | NO ACTION | |
| `FK_PO_V` | `inventory.PurchaseOrders(TenantId, VendorId)` → `inventory.Vendors` | NO ACTION | |
| `FK_POI_PO`, `FK_POI_II` | `inventory.PurchaseOrderItems` → `PurchaseOrders`/`InventoryItems` | NO ACTION | |

---

## 11. Relationship Explanations

### 11.1 Tenant ↔ Everything (1:N)
Every business table has `TenantId UNIQUEIDENTIFIER NOT NULL` referencing `tenant.Tenants(TenantId)`. This is the **tenant isolation root**. The FK is declared, but never used as a join in queries — global query filters in EF Core inject `TenantId = @CurrentTenantId` on every read.

### 11.2 User ↔ Role ↔ Permission (M:N:M)
Classic RBAC.
- `User —< UserRole >— Role`
- `Role —< RolePermission >— Permission`
- Effective permissions for user: `User → UserRoles → Roles → RolePermissions → Permissions`. Cached in Redis at `perm:{TenantId}:{UserId}` after login (TTL 5 min, busted on assignment change via integration event).

### 11.3 Subscription Lifecycle
- `Tenant 1—* Subscription 1—* SubscriptionInvoice 1—* SubscriptionPayment`
- `Subscription *—1 Plan` (Plan is global, not tenant-scoped — system data).
- A tenant has at most **one Active or Trialing** subscription enforced by a unique filtered index `WHERE Status IN (0,1)`.

### 11.4 Academic Year — Class — Section
- `AcademicYear 1—* Class` (a class in 2024-25 is **not** the same row as a class in 2025-26).
- `Class 1—* Section`. This drives **Enrollment** and is what the timetable hangs off.
- Re-creating Classes per year may seem wasteful but lets us track historical capacity, fee structures, and attendance per AY without temporal joins.

### 11.5 Student ↔ Enrollment ↔ Section
- `Student 1—* StudentEnrollment` (many enrollments over a student's lifetime, one per AY).
- `Enrollment *—1 Section`. Reads like attendance/exams join via Enrollment, not Student → Section, so promotion/transfer is a row insert in Enrollment, not a mass UPDATE.

### 11.6 Student ↔ Parent (M:N via `StudentParents`)
- A student can have multiple parents (Father, Mother, Guardian).
- A parent (typically) has multiple children. We enforce **at most one `IsPrimary = 1`** per student via a filtered unique index.

### 11.7 Attendance Session ↔ Student Attendance (1:N)
- One row in `AttendanceSessions` per (Section, Date, Period).
- Many rows in `StudentAttendance` per session (one per enrolled student).
- Modeled as **two aggregates** — write fan-out is too high to keep in one transaction. Domain rule: cannot mark students whose enrollment is not active in the section on that date.

### 11.8 Exam Hierarchy
- `Exam 1—* ExamSchedule 1—* ExamSubject 1—* ExamResult`.
- `ReportCard` materialized per `(Exam, Student)` once results are published; recomputable from `ExamResult` for repair.

### 11.9 Fees Hierarchy
- `FeeStructure 1—* FeeInstallment` (template).
- `Student *—* FeeStructure` via `StudentFeeAssignment`.
- Per assignment, a **billing engine** generates `FeeInvoice 1—* FeeInvoiceLine` on installment due dates (Hangfire recurring).
- `FeeInvoice 1—* FeePayment` (partial payments allowed).
- `FeePayment 1—* FeeRefund`.

### 11.10 Transport
- `Route 1—* Stop` (ordered).
- `Vehicle 1—* VehicleAssignment` over time.
- `Student *—1 StudentTransportAssignment` per AY (composite of Route + Stop + Vehicle).
- `Vehicle 1—* GPSDevice 1—* GPSPing` (high-volume time-series).

### 11.11 Communication
- `NotificationTemplate 1—* Notification 1—* NotificationRecipient`.
- Each channel writes to its own log table (`EmailLogs`, `SmsLogs`, etc.) — providers' webhooks update delivery status.

### 11.12 Library
- `BookCategory 1—* Book 1—* BookCopy 1—* BookIssue 1—1 BookReturn`.
- A `BookIssue` may have multiple `BookFine`s (for damage, lost, late).

### 11.13 Inventory
- `Vendor 1—* PurchaseOrder 1—* PurchaseOrderItem`.
- `InventoryItem 1—* StockTransaction` — running balance is denormalized in `InventoryItem.StockOnHand` for read speed and reconcilable from `StockTransaction.BalanceAfter`.

### 11.14 Audit, Outbox, Inbox
- `AuditLog`, `ActivityLog`, `ErrorLog` are append-only.
- `OutboxMessage` and `InboxMessage` are stateful (status transitions: Pending → Processing → Published/Processed → Failed → DeadLettered).
- They participate in the **same DB transaction** as the business write (Outbox) and the consumer's business write (Inbox).

---

## 12. Cardinality Matrix

> Reads as: **Row** entity has **Column** relation with cardinality `(min..max)` ⇄ `(min..max)`.

### 12.1 Tenant Module

| From \ To | Tenant | TenantSetting | TenantBranding | TenantDomain | TenantStorageSettings | TenantEmailSettings | TenantSmsSettings |
|-----------|:------:|:-------------:|:--------------:|:------------:|:---------------------:|:-------------------:|:-----------------:|
| **Tenant** | — | 1 ⇄ 0..N | 1 ⇄ 0..1 | 1 ⇄ 1..N | 1 ⇄ 0..1 | 1 ⇄ 0..1 | 1 ⇄ 0..1 |

### 12.2 Identity Module

| From \ To | User | Role | Permission | UserRole | RolePerm | RefreshToken | UserSession |
|-----------|:----:|:----:|:----------:|:--------:|:--------:|:------------:|:-----------:|
| **User** | — | M ⇄ N | M ⇄ N (transitive) | 1 ⇄ 0..N | — | 1 ⇄ 0..N | 1 ⇄ 0..N |
| **Role** | M ⇄ N | — | M ⇄ N | 1 ⇄ 0..N | 1 ⇄ 0..N | — | — |
| **Permission** | — | M ⇄ N | — | — | 1 ⇄ 0..N | — | — |

### 12.3 Billing Module

| From \ To | Plan | Subscription | Invoice | Payment | Coupon | Refund |
|-----------|:----:|:------------:|:-------:|:-------:|:------:|:------:|
| **Tenant** | — | 1 ⇄ 1..N | — | — | — | — |
| **Plan** | — | 1 ⇄ 0..N | — | — | — | — |
| **Subscription** | N ⇄ 1 | — | 1 ⇄ 0..N | — | N ⇄ 0..1 | — |
| **Invoice** | — | N ⇄ 1 | — | 1 ⇄ 0..N | — | — |
| **Payment** | — | — | N ⇄ 1 | — | — | 1 ⇄ 0..N |

### 12.4 Academic + Students Modules

| From \ To | AcademicYear | Term | Class | Section | Subject | Student | Enrollment | Parent |
|-----------|:------------:|:----:|:-----:|:-------:|:-------:|:-------:|:----------:|:------:|
| **AcademicYear** | — | 1 ⇄ 1..N | 1 ⇄ 0..N | — | — | — | 1 ⇄ 0..N | — |
| **Class** | N ⇄ 1 | — | — | 1 ⇄ 1..N | M ⇄ N (Assignment) | — | 1 ⇄ 0..N | — |
| **Section** | — | — | N ⇄ 1 | — | — | — | 1 ⇄ 0..N | — |
| **Student** | — | — | — | (via Enrollment) | — | — | 1 ⇄ 1..N | M ⇄ N |
| **Parent** | — | — | — | — | — | M ⇄ N | — | — |

### 12.5 Attendance Module

| From \ To | Section | AttendanceSession | StudentAttendance | Student |
|-----------|:-------:|:-----------------:|:-----------------:|:-------:|
| **Section** | — | 1 ⇄ 0..N | — | — |
| **AttendanceSession** | N ⇄ 1 | — | 1 ⇄ 0..N | — |
| **StudentAttendance** | — | N ⇄ 1 | — | N ⇄ 1 |

### 12.6 Exam Module

| From \ To | Exam | ExamSchedule | ExamSubject | ExamResult | Grade | ReportCard | Student |
|-----------|:----:|:------------:|:-----------:|:----------:|:-----:|:----------:|:-------:|
| **Exam** | — | 1 ⇄ 1..N | — | 1 ⇄ 0..N | — | 1 ⇄ 0..N | — |
| **ExamSchedule** | N ⇄ 1 | — | 1 ⇄ 1..N | — | — | — | — |
| **ExamSubject** | — | N ⇄ 1 | — | 1 ⇄ 0..N | — | — | — |
| **ExamResult** | N ⇄ 1 | — | N ⇄ 1 | — | N ⇄ 0..1 | — | N ⇄ 1 |
| **ReportCard** | N ⇄ 1 | — | — | — | N ⇄ 0..1 | — | N ⇄ 1 |

### 12.7 Fees Module

| From \ To | FeeType | FeeStructure | FeeInstallment | StudentFeeAssign | FeeInvoice | FeeInvoiceLine | FeePayment | FeeRefund | FeeDiscount |
|-----------|:-------:|:------------:|:--------------:|:----------------:|:----------:|:--------------:|:----------:|:---------:|:-----------:|
| **FeeStructure** | — | — | 1 ⇄ 1..N | 1 ⇄ 0..N | — | — | — | — | — |
| **Student** | — | M ⇄ N | — | 1 ⇄ 0..N | 1 ⇄ 0..N | — | — | — | — |
| **FeeInvoice** | — | — | — | N ⇄ 1 | — | 1 ⇄ 1..N | 1 ⇄ 0..N | — | M ⇄ N |
| **FeePayment** | — | — | — | — | N ⇄ 1 | — | — | 1 ⇄ 0..N | — |

### 12.8 Transport Module

| From \ To | Vehicle | Route | Stop | Driver | VehicleAssign | StudentTransport | GPSDevice | GPSPing |
|-----------|:-------:|:-----:|:----:|:------:|:-------------:|:----------------:|:---------:|:-------:|
| **Vehicle** | — | M ⇄ N (assign) | — | M ⇄ N (assign) | 1 ⇄ 0..N | 1 ⇄ 0..N | 1 ⇄ 0..N | — |
| **Route** | — | — | 1 ⇄ 1..N | — | 1 ⇄ 0..N | 1 ⇄ 0..N | — | — |
| **Driver** | — | — | — | — | 1 ⇄ 0..N | — | — | — |
| **GPSDevice** | N ⇄ 1 | — | — | — | — | — | — | 1 ⇄ 0..N |
| **Student** | — | — | — | — | — | 1 ⇄ 0..N | — | — |

### 12.9 Communication Module

| From \ To | Template | Notification | Recipient | EmailLog | SmsLog | WhatsAppLog | PushLog |
|-----------|:--------:|:------------:|:---------:|:--------:|:------:|:-----------:|:-------:|
| **Template** | — | 1 ⇄ 0..N | — | — | — | — | — |
| **Notification** | N ⇄ 0..1 | — | 1 ⇄ 1..N | 1 ⇄ 0..N | 1 ⇄ 0..N | 1 ⇄ 0..N | 1 ⇄ 0..N |
| **Recipient** | — | N ⇄ 1 | — | — | — | — | — |

### 12.10 Library + Inventory Modules

| From \ To | BookCat | Book | BookCopy | BookIssue | BookReturn | BookFine |
|-----------|:-------:|:----:|:--------:|:---------:|:----------:|:--------:|
| **BookCategory** | — | 1 ⇄ 0..N | — | — | — | — |
| **Book** | N ⇄ 0..1 | — | 1 ⇄ 1..N | — | — | — |
| **BookCopy** | — | N ⇄ 1 | — | 1 ⇄ 0..N | — | — |
| **BookIssue** | — | — | N ⇄ 1 | — | 1 ⇄ 0..1 | 1 ⇄ 0..N |

| From \ To | InvCat | InvItem | Vendor | PO | POItem | StockTxn |
|-----------|:------:|:-------:|:------:|:---:|:------:|:--------:|
| **Vendor** | — | — | — | 1 ⇄ 0..N | — | — |
| **PO** | — | — | N ⇄ 1 | — | 1 ⇄ 1..N | — |
| **POItem** | — | N ⇄ 1 | — | N ⇄ 1 | — | — |
| **InvItem** | N ⇄ 0..1 | — | — | — | — | 1 ⇄ 0..N |

### 12.11 Cross-Context Cardinalities (Selected)

| Context A → Context B | Relationship | Cardinality | Mechanism |
|------------------------|--------------|-------------|-----------|
| Identity → Students | User can be a Student | 1 ⇄ 0..1 | `student.Students.UserId` nullable |
| Identity → Employees | User can be an Employee | 1 ⇄ 0..1 | `employee.Employees.UserId` nullable |
| Identity → Parents | User can be a Parent | 1 ⇄ 0..1 | `student.Parents.UserId` nullable |
| Academics → Fees | AcademicYear underpins FeeStructure | 1 ⇄ 0..N | `FeeStructures.AcademicYearId` |
| Academics → Attendance | Section drives sessions | 1 ⇄ 0..N | `AttendanceSessions.SectionId` |
| Students → Fees | Student receives invoices | 1 ⇄ 0..N | `FeeInvoices.StudentId` |
| Students → Exams | Student has results | 1 ⇄ 0..N | `ExamResults.StudentId` |
| Students → Library | Student borrows | 1 ⇄ 0..N | `BookIssues.BorrowerId` (with `BorrowerType = Student`) |
| Students → Transport | Student assigned to route | 1 ⇄ 0..N | `StudentTransportAssignments.StudentId` |
| Audit → Everything | Subscriber to all `*.exchange` | 1 ⇄ 0..N | RabbitMQ binding only — no DB FK |
