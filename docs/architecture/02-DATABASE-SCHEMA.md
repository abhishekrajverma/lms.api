# 02 — SQL Server Schema Definitions

> Production DDL for SQL Server 2022. Schemas split per module: `tenant`, `identity`, `billing`, `academic`, `student`, `employee`, `attendance`, `exam`, `fees`, `transport`, `comm`, `library`, `inventory`, `audit`. Common audit columns shown once below; **every** business table has them.

## 8.0 Schemas, Common Conventions, Data Types

```sql
-- =========================================================
-- 8.0  Schemas
-- =========================================================
CREATE SCHEMA tenant      AUTHORIZATION dbo;
CREATE SCHEMA identity_   AUTHORIZATION dbo;   -- 'identity' is reserved-ish in SQL Server
CREATE SCHEMA billing     AUTHORIZATION dbo;
CREATE SCHEMA academic    AUTHORIZATION dbo;
CREATE SCHEMA student     AUTHORIZATION dbo;
CREATE SCHEMA employee    AUTHORIZATION dbo;
CREATE SCHEMA attendance  AUTHORIZATION dbo;
CREATE SCHEMA exam        AUTHORIZATION dbo;
CREATE SCHEMA fees        AUTHORIZATION dbo;
CREATE SCHEMA transport   AUTHORIZATION dbo;
CREATE SCHEMA comm        AUTHORIZATION dbo;
CREATE SCHEMA library_    AUTHORIZATION dbo;
CREATE SCHEMA inventory   AUTHORIZATION dbo;
CREATE SCHEMA audit_      AUTHORIZATION dbo;
GO
```

### Audit Column Set (applies to every business table)

```sql
-- The following columns are present on every business table
[TenantId]    UNIQUEIDENTIFIER NOT NULL,
[CreatedAt]   DATETIME2(3)     NOT NULL CONSTRAINT DF_<Table>_CreatedAt DEFAULT (SYSUTCDATETIME()),
[CreatedBy]   UNIQUEIDENTIFIER NULL,
[UpdatedAt]   DATETIME2(3)     NULL,
[UpdatedBy]   UNIQUEIDENTIFIER NULL,
[IsDeleted]   BIT              NOT NULL CONSTRAINT DF_<Table>_IsDeleted DEFAULT (0),
[DeletedAt]   DATETIME2(3)     NULL,
[DeletedBy]   UNIQUEIDENTIFIER NULL,
[RowVersion]  ROWVERSION       NOT NULL
```

## 8.1 Tenant Management

```sql
CREATE TABLE tenant.Tenants (
    TenantId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Tenants_Id DEFAULT (NEWSEQUENTIALID()),
    Code                VARCHAR(64)      NOT NULL,
    Name                NVARCHAR(200)    NOT NULL,
    LegalName           NVARCHAR(200)    NULL,
    Status              TINYINT          NOT NULL CONSTRAINT DF_Tenants_Status DEFAULT (0),
    PlanId              UNIQUEIDENTIFIER NULL,
    TimeZone            VARCHAR(64)      NOT NULL CONSTRAINT DF_Tenants_TZ   DEFAULT ('UTC'),
    Locale              VARCHAR(16)      NOT NULL CONSTRAINT DF_Tenants_Loc  DEFAULT ('en-US'),
    Currency            CHAR(3)          NOT NULL CONSTRAINT DF_Tenants_Cur  DEFAULT ('USD'),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Tenants_CA   DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Tenants_Del  DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Tenants  PRIMARY KEY CLUSTERED (TenantId),
    CONSTRAINT UX_Tenants_Code UNIQUE (Code),
    CONSTRAINT CK_Tenants_Status CHECK (Status IN (0,1,2,3))
);

CREATE TABLE tenant.TenantSettings (
    TenantSettingId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TS_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    [Key]               VARCHAR(128)     NOT NULL,
    [Value]             NVARCHAR(MAX)    NULL,
    ValueType           VARCHAR(32)      NOT NULL CONSTRAINT DF_TS_VT DEFAULT ('string'),
    IsEncrypted         BIT              NOT NULL CONSTRAINT DF_TS_Enc DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TS_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TS_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_TenantSettings PRIMARY KEY CLUSTERED (TenantId, TenantSettingId),
    CONSTRAINT UX_TenantSettings_Key UNIQUE (TenantId, [Key]),
    CONSTRAINT FK_TS_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId) ON DELETE NO ACTION
);

CREATE TABLE tenant.TenantBranding (
    TenantBrandingId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TB_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    LogoBlobKey         VARCHAR(512)     NULL,
    FaviconBlobKey      VARCHAR(512)     NULL,
    PrimaryColor        VARCHAR(9)       NULL,
    SecondaryColor     VARCHAR(9)        NULL,
    AccentColor        VARCHAR(9)        NULL,
    Theme               VARCHAR(32)      NOT NULL CONSTRAINT DF_TB_Theme DEFAULT ('default'),
    CustomCss           NVARCHAR(MAX)    NULL,
    EmailHeaderHtml     NVARCHAR(MAX)    NULL,
    EmailFooterHtml     NVARCHAR(MAX)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TB_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TB_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_TenantBranding PRIMARY KEY CLUSTERED (TenantId, TenantBrandingId),
    CONSTRAINT UX_TenantBranding_Tenant UNIQUE (TenantId),
    CONSTRAINT FK_TB_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE tenant.TenantDomains (
    TenantDomainId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TD_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Host                VARCHAR(253)     NOT NULL,
    IsPrimary           BIT              NOT NULL CONSTRAINT DF_TD_Prim DEFAULT (0),
    IsVerified          BIT              NOT NULL CONSTRAINT DF_TD_Ver  DEFAULT (0),
    VerifiedAtUtc       DATETIME2(3)     NULL,
    SslCertExpiresAtUtc DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TD_CA   DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TD_Del  DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_TenantDomains PRIMARY KEY CLUSTERED (TenantId, TenantDomainId),
    CONSTRAINT UX_TenantDomains_Host UNIQUE (Host),
    CONSTRAINT FK_TD_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE tenant.TenantStorageSettings (
    SettingId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TSS_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Provider            VARCHAR(32)      NOT NULL,         -- 'AzureBlob','S3','Local'
    BucketOrContainer   VARCHAR(256)     NOT NULL,
    Region              VARCHAR(64)      NULL,
    Endpoint            VARCHAR(512)     NULL,
    AccessKeyEncrypted  VARBINARY(2048)  NULL,
    SecretKeyEncrypted  VARBINARY(2048)  NULL,
    CdnBaseUrl          VARCHAR(512)     NULL,
    QuotaBytes          BIGINT           NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TSS_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TSS_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_TenantStorageSettings PRIMARY KEY CLUSTERED (TenantId, SettingId),
    CONSTRAINT UX_TSS_Tenant UNIQUE (TenantId),
    CONSTRAINT FK_TSS_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE tenant.TenantEmailSettings (
    SettingId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TES_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Provider            VARCHAR(32)      NOT NULL,         -- 'SendGrid','SES','SMTP'
    FromAddress         NVARCHAR(256)    NOT NULL,
    FromName            NVARCHAR(200)    NULL,
    ReplyTo             NVARCHAR(256)    NULL,
    SmtpHost            VARCHAR(256)     NULL,
    SmtpPort            INT              NULL,
    SmtpUser            NVARCHAR(256)    NULL,
    SmtpPassEncrypted   VARBINARY(2048)  NULL,
    ApiKeyEncrypted     VARBINARY(2048)  NULL,
    UseSsl              BIT              NOT NULL CONSTRAINT DF_TES_Ssl DEFAULT (1),
    DailyQuota          INT              NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TES_CA  DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TES_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_TenantEmailSettings PRIMARY KEY CLUSTERED (TenantId, SettingId),
    CONSTRAINT UX_TES_Tenant UNIQUE (TenantId),
    CONSTRAINT FK_TES_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE tenant.TenantSmsSettings (
    SettingId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TSM_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Provider            VARCHAR(32)      NOT NULL,         -- 'Twilio','MSG91','Vonage'
    SenderId            VARCHAR(32)      NULL,
    AccountSidEncrypted VARBINARY(2048)  NULL,
    AuthTokenEncrypted  VARBINARY(2048)  NULL,
    DailyQuota          INT              NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TSM_CA  DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TSM_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_TenantSmsSettings PRIMARY KEY CLUSTERED (TenantId, SettingId),
    CONSTRAINT UX_TSM_Tenant UNIQUE (TenantId),
    CONSTRAINT FK_TSM_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);
```

## 8.2 Identity & Access

```sql
CREATE TABLE identity_.Users (
    UserId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Users_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Email               NVARCHAR(256)    NOT NULL,
    NormalizedEmail     NVARCHAR(256)    NOT NULL,
    UserName            NVARCHAR(128)    NOT NULL,
    NormalizedUserName  NVARCHAR(128)    NOT NULL,
    PasswordHash        VARBINARY(512)   NULL,
    PasswordSalt        VARBINARY(64)    NULL,
    SecurityStamp       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Users_Stamp DEFAULT (NEWID()),
    FirstName           NVARCHAR(100)    NULL,
    LastName            NVARCHAR(100)    NULL,
    DisplayName AS (CONCAT(FirstName, N' ', LastName)) PERSISTED,
    PhoneNumber         VARCHAR(32)      NULL,
    PhoneConfirmed      BIT              NOT NULL CONSTRAINT DF_Users_PC DEFAULT (0),
    EmailConfirmed      BIT              NOT NULL CONSTRAINT DF_Users_EC DEFAULT (0),
    TwoFactorEnabled    BIT              NOT NULL CONSTRAINT DF_Users_2FA DEFAULT (0),
    LockoutEndUtc       DATETIME2(3)     NULL,
    AccessFailedCount   INT              NOT NULL CONSTRAINT DF_Users_AFC DEFAULT (0),
    LastLoginAtUtc      DATETIME2(3)     NULL,
    LastPasswordChangeUtc DATETIME2(3)   NULL,
    PreferredLocale     VARCHAR(16)      NULL,
    AvatarBlobKey       VARCHAR(512)     NULL,
    UserType            TINYINT          NOT NULL CONSTRAINT DF_Users_Type DEFAULT (0), -- 0=Staff,1=Parent,2=Student,3=System
    IsActive            BIT              NOT NULL CONSTRAINT DF_Users_Act DEFAULT (1),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Users_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Users_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (TenantId, UserId),
    CONSTRAINT UX_Users_Tenant_Email   UNIQUE (TenantId, NormalizedEmail),
    CONSTRAINT UX_Users_Tenant_UserName UNIQUE (TenantId, NormalizedUserName),
    CONSTRAINT FK_Users_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE identity_.Roles (
    RoleId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Roles_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(128)    NOT NULL,
    NormalizedName      NVARCHAR(128)    NOT NULL,
    Description         NVARCHAR(500)    NULL,
    IsSystem            BIT              NOT NULL CONSTRAINT DF_Roles_Sys DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Roles_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Roles_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (TenantId, RoleId),
    CONSTRAINT UX_Roles_Tenant_Name UNIQUE (TenantId, NormalizedName),
    CONSTRAINT FK_Roles_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE identity_.Permissions (
    PermissionId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Perm_Id DEFAULT (NEWSEQUENTIALID()),
    Code                VARCHAR(128)     NOT NULL,
    Name                NVARCHAR(200)    NOT NULL,
    Module              VARCHAR(64)      NOT NULL,
    Description         NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Perm_CA DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED (PermissionId),
    CONSTRAINT UX_Permissions_Code UNIQUE (Code)
);

CREATE TABLE identity_.UserRoles (
    UserRoleId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_UR_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NOT NULL,
    RoleId              UNIQUEIDENTIFIER NOT NULL,
    AssignedAt          DATETIME2(3)     NOT NULL CONSTRAINT DF_UR_AA DEFAULT (SYSUTCDATETIME()),
    AssignedBy          UNIQUEIDENTIFIER NULL,
    ExpiresAtUtc        DATETIME2(3)     NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (TenantId, UserId, RoleId),
    CONSTRAINT FK_UR_User FOREIGN KEY (TenantId, UserId) REFERENCES identity_.Users(TenantId, UserId),
    CONSTRAINT FK_UR_Role FOREIGN KEY (TenantId, RoleId) REFERENCES identity_.Roles(TenantId, RoleId)
);

CREATE TABLE identity_.RolePermissions (
    RolePermissionId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RP_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    RoleId              UNIQUEIDENTIFIER NOT NULL,
    PermissionId        UNIQUEIDENTIFIER NOT NULL,
    GrantedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_RP_GA DEFAULT (SYSUTCDATETIME()),
    GrantedBy           UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (TenantId, RoleId, PermissionId),
    CONSTRAINT FK_RP_Role FOREIGN KEY (TenantId, RoleId) REFERENCES identity_.Roles(TenantId, RoleId),
    CONSTRAINT FK_RP_Perm FOREIGN KEY (PermissionId) REFERENCES identity_.Permissions(PermissionId)
);

CREATE TABLE identity_.RefreshTokens (
    RefreshTokenId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RT_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NOT NULL,
    TokenHash           BINARY(32)       NOT NULL,         -- SHA-256 of token
    JwtId               VARCHAR(64)      NOT NULL,
    ExpiresAtUtc        DATETIME2(3)     NOT NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_RT_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    RevokedAtUtc        DATETIME2(3)     NULL,
    ReplacedByTokenId   UNIQUEIDENTIFIER NULL,
    ClientIp            VARCHAR(45)      NULL,
    UserAgent           NVARCHAR(400)    NULL,
    DeviceFingerprint   VARCHAR(128)     NULL,
    CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (TenantId, RefreshTokenId),
    CONSTRAINT UX_RT_Hash UNIQUE (TokenHash),
    CONSTRAINT FK_RT_User FOREIGN KEY (TenantId, UserId) REFERENCES identity_.Users(TenantId, UserId)
);

CREATE TABLE identity_.LoginHistory (
    LoginHistoryId      BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    AttemptedEmail      NVARCHAR(256)    NULL,
    Success             BIT              NOT NULL,
    FailureReason       VARCHAR(64)      NULL,
    IpAddress           VARCHAR(45)      NULL,
    UserAgent           NVARCHAR(400)    NULL,
    GeoCountry          CHAR(2)          NULL,
    OccurredAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_LH_OA DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_LoginHistory PRIMARY KEY CLUSTERED (TenantId, OccurredAtUtc, LoginHistoryId)
);

CREATE TABLE identity_.PasswordHistory (
    PasswordHistoryId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PH_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NOT NULL,
    PasswordHash        VARBINARY(512)   NOT NULL,
    PasswordSalt        VARBINARY(64)    NOT NULL,
    ChangedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_PH_CA DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_PasswordHistory PRIMARY KEY CLUSTERED (TenantId, UserId, ChangedAtUtc),
    CONSTRAINT FK_PH_User FOREIGN KEY (TenantId, UserId) REFERENCES identity_.Users(TenantId, UserId)
);

CREATE TABLE identity_.UserSessions (
    SessionId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_US_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NOT NULL,
    StartedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_US_SA DEFAULT (SYSUTCDATETIME()),
    LastActivityAtUtc   DATETIME2(3)     NOT NULL CONSTRAINT DF_US_LA DEFAULT (SYSUTCDATETIME()),
    EndedAtUtc          DATETIME2(3)     NULL,
    IpAddress           VARCHAR(45)      NULL,
    UserAgent           NVARCHAR(400)    NULL,
    DeviceFingerprint   VARCHAR(128)     NULL,
    EndReason           VARCHAR(32)      NULL,             -- 'Logout','Timeout','Forced'
    CONSTRAINT PK_UserSessions PRIMARY KEY CLUSTERED (TenantId, UserId, SessionId),
    CONSTRAINT FK_US_User FOREIGN KEY (TenantId, UserId) REFERENCES identity_.Users(TenantId, UserId)
);
```

## 8.3 Subscription & Billing

```sql
CREATE TABLE billing.Plans (
    PlanId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Plans_Id DEFAULT (NEWSEQUENTIALID()),
    Code                VARCHAR(64)      NOT NULL,
    Name                NVARCHAR(200)    NOT NULL,
    Description         NVARCHAR(2000)   NULL,
    BillingCycle        TINYINT          NOT NULL,         -- 1=Monthly,2=Quarterly,3=Annual
    Price               DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL CONSTRAINT DF_Plans_Cur DEFAULT ('USD'),
    TrialDays           INT              NOT NULL CONSTRAINT DF_Plans_Tr DEFAULT (0),
    MaxStudents         INT              NULL,
    MaxStaff            INT              NULL,
    MaxStorageGb        INT              NULL,
    IsActive            BIT              NOT NULL CONSTRAINT DF_Plans_Act DEFAULT (1),
    IsPublic            BIT              NOT NULL CONSTRAINT DF_Plans_Pub DEFAULT (1),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Plans_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Plans_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Plans PRIMARY KEY CLUSTERED (PlanId),
    CONSTRAINT UX_Plans_Code UNIQUE (Code),
    CONSTRAINT CK_Plans_Cycle CHECK (BillingCycle IN (1,2,3))
);

CREATE TABLE billing.Features (
    FeatureId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Feat_Id DEFAULT (NEWSEQUENTIALID()),
    Code                VARCHAR(64)      NOT NULL,
    Name                NVARCHAR(200)    NOT NULL,
    Description         NVARCHAR(2000)   NULL,
    ValueType           VARCHAR(16)      NOT NULL,         -- 'bool','int','string'
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Feat_CA DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Features PRIMARY KEY CLUSTERED (FeatureId),
    CONSTRAINT UX_Features_Code UNIQUE (Code)
);

CREATE TABLE billing.PlanFeatures (
    PlanFeatureId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PF_Id DEFAULT (NEWSEQUENTIALID()),
    PlanId              UNIQUEIDENTIFIER NOT NULL,
    FeatureId           UNIQUEIDENTIFIER NOT NULL,
    [Value]             NVARCHAR(200)    NOT NULL,
    CONSTRAINT PK_PlanFeatures PRIMARY KEY CLUSTERED (PlanId, FeatureId),
    CONSTRAINT FK_PF_Plan FOREIGN KEY (PlanId) REFERENCES billing.Plans(PlanId),
    CONSTRAINT FK_PF_Feat FOREIGN KEY (FeatureId) REFERENCES billing.Features(FeatureId)
);

CREATE TABLE billing.Subscriptions (
    SubscriptionId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Sub_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    PlanId              UNIQUEIDENTIFIER NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Trialing,1=Active,2=PastDue,3=Cancelled,4=Expired
    TrialEndsAtUtc      DATETIME2(3)     NULL,
    CurrentPeriodStartUtc DATETIME2(3)   NOT NULL,
    CurrentPeriodEndUtc   DATETIME2(3)   NOT NULL,
    CancelAtPeriodEnd   BIT              NOT NULL CONSTRAINT DF_Sub_CAPE DEFAULT (0),
    CancelledAtUtc      DATETIME2(3)     NULL,
    CouponId            UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Sub_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Sub_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Subscriptions PRIMARY KEY CLUSTERED (TenantId, SubscriptionId),
    CONSTRAINT FK_Sub_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId),
    CONSTRAINT FK_Sub_Plan   FOREIGN KEY (PlanId)   REFERENCES billing.Plans(PlanId),
    CONSTRAINT CK_Sub_Status CHECK (Status IN (0,1,2,3,4)),
    CONSTRAINT CK_Sub_Period CHECK (CurrentPeriodEndUtc > CurrentPeriodStartUtc)
);

CREATE TABLE billing.SubscriptionInvoices (
    InvoiceId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SI_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId      UNIQUEIDENTIFIER NOT NULL,
    Number              VARCHAR(40)      NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Draft,1=Open,2=Paid,3=Void,4=Uncollectible
    Subtotal            DECIMAL(18,4)    NOT NULL,
    TaxTotal            DECIMAL(18,4)    NOT NULL CONSTRAINT DF_SI_Tax DEFAULT (0),
    DiscountTotal       DECIMAL(18,4)    NOT NULL CONSTRAINT DF_SI_Dis DEFAULT (0),
    Total               DECIMAL(18,4)    NOT NULL,
    AmountPaid          DECIMAL(18,4)    NOT NULL CONSTRAINT DF_SI_AP DEFAULT (0),
    AmountDue           AS (Total - AmountPaid) PERSISTED,
    Currency            CHAR(3)          NOT NULL,
    IssuedAtUtc         DATETIME2(3)     NOT NULL,
    DueAtUtc            DATETIME2(3)     NOT NULL,
    PaidAtUtc           DATETIME2(3)     NULL,
    PdfBlobKey          VARCHAR(512)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SI_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SI_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_SubInvoices PRIMARY KEY CLUSTERED (TenantId, InvoiceId),
    CONSTRAINT UX_SI_Number UNIQUE (TenantId, Number),
    CONSTRAINT FK_SI_Sub FOREIGN KEY (TenantId, SubscriptionId) REFERENCES billing.Subscriptions(TenantId, SubscriptionId),
    CONSTRAINT CK_SI_Status CHECK (Status IN (0,1,2,3,4)),
    CONSTRAINT CK_SI_Total  CHECK (Total = Subtotal + TaxTotal - DiscountTotal)
);

CREATE TABLE billing.SubscriptionPayments (
    PaymentId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SP_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    InvoiceId           UNIQUEIDENTIFIER NOT NULL,
    Amount              DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL,
    Method              TINYINT          NOT NULL,         -- 0=Card,1=NetBanking,2=UPI,3=Wire,4=Cheque
    GatewayProvider     VARCHAR(64)      NULL,
    GatewayPaymentId    VARCHAR(128)     NULL,
    GatewayRefundId     VARCHAR(128)     NULL,
    Status              TINYINT          NOT NULL,         -- 0=Init,1=Succeeded,2=Failed,3=Refunded,4=PartRefund
    ReceivedAtUtc       DATETIME2(3)     NOT NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SP_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SP_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_SubPayments PRIMARY KEY CLUSTERED (TenantId, PaymentId),
    CONSTRAINT UX_SP_Gateway UNIQUE (GatewayProvider, GatewayPaymentId),
    CONSTRAINT FK_SP_Inv FOREIGN KEY (TenantId, InvoiceId) REFERENCES billing.SubscriptionInvoices(TenantId, InvoiceId),
    CONSTRAINT CK_SP_Status CHECK (Status IN (0,1,2,3,4))
);

CREATE TABLE billing.SubscriptionUsage (
    UsageId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SU_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId      UNIQUEIDENTIFIER NOT NULL,
    FeatureCode         VARCHAR(64)      NOT NULL,
    PeriodStartUtc      DATETIME2(3)     NOT NULL,
    PeriodEndUtc        DATETIME2(3)     NOT NULL,
    Quantity            BIGINT           NOT NULL CONSTRAINT DF_SU_Q DEFAULT (0),
    Limit_              BIGINT           NULL,
    LastUpdatedAtUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_SU_LU DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_SubUsage PRIMARY KEY CLUSTERED (TenantId, SubscriptionId, FeatureCode, PeriodStartUtc),
    CONSTRAINT FK_SU_Sub FOREIGN KEY (TenantId, SubscriptionId) REFERENCES billing.Subscriptions(TenantId, SubscriptionId)
);

CREATE TABLE billing.Coupons (
    CouponId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_C_Id DEFAULT (NEWSEQUENTIALID()),
    Code                VARCHAR(40)      NOT NULL,
    DiscountType        TINYINT          NOT NULL,         -- 0=Percent,1=Amount
    DiscountValue       DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NULL,
    MaxRedemptions      INT              NULL,
    Redemptions         INT              NOT NULL CONSTRAINT DF_C_R DEFAULT (0),
    ValidFromUtc        DATETIME2(3)     NULL,
    ValidUntilUtc       DATETIME2(3)     NULL,
    IsActive            BIT              NOT NULL CONSTRAINT DF_C_Act DEFAULT (1),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_C_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_C_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Coupons PRIMARY KEY CLUSTERED (CouponId),
    CONSTRAINT UX_Coupons_Code UNIQUE (Code),
    CONSTRAINT CK_Coupons_Type CHECK (DiscountType IN (0,1)),
    CONSTRAINT CK_Coupons_Red CHECK (Redemptions <= ISNULL(MaxRedemptions, Redemptions))
);

CREATE TABLE billing.Refunds (
    RefundId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_R_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    PaymentId           UNIQUEIDENTIFIER NOT NULL,
    Amount              DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL,
    Reason              NVARCHAR(500)    NULL,
    Status              TINYINT          NOT NULL,         -- 0=Init,1=Succeeded,2=Failed
    GatewayRefundId     VARCHAR(128)     NULL,
    ProcessedAtUtc      DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_R_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_R_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Refunds PRIMARY KEY CLUSTERED (TenantId, RefundId),
    CONSTRAINT FK_R_Pay FOREIGN KEY (TenantId, PaymentId) REFERENCES billing.SubscriptionPayments(TenantId, PaymentId)
);

CREATE TABLE billing.WebhookEvents (
    WebhookEventId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_WE_Id DEFAULT (NEWSEQUENTIALID()),
    Provider            VARCHAR(64)      NOT NULL,
    EventId             VARCHAR(128)     NOT NULL,
    EventType           VARCHAR(128)     NOT NULL,
    Payload             NVARCHAR(MAX)    NOT NULL,
    Signature           NVARCHAR(512)    NULL,
    ReceivedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_WE_RA DEFAULT (SYSUTCDATETIME()),
    ProcessedAtUtc      DATETIME2(3)     NULL,
    Status              TINYINT          NOT NULL CONSTRAINT DF_WE_St DEFAULT (0),  -- 0=Received,1=Processed,2=Failed
    Attempts            INT              NOT NULL CONSTRAINT DF_WE_At DEFAULT (0),
    LastError           NVARCHAR(MAX)    NULL,
    CONSTRAINT PK_WebhookEvents PRIMARY KEY CLUSTERED (WebhookEventId),
    CONSTRAINT UX_WE_Provider_Event UNIQUE (Provider, EventId)
);

CREATE TABLE billing.BillingAddresses (
    BillingAddressId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BA_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Line1               NVARCHAR(200)    NOT NULL,
    Line2               NVARCHAR(200)    NULL,
    City                NVARCHAR(100)    NOT NULL,
    State               NVARCHAR(100)    NULL,
    PostalCode          VARCHAR(20)      NULL,
    CountryCode         CHAR(2)          NOT NULL,
    TaxId               VARCHAR(50)      NULL,
    IsDefault           BIT              NOT NULL CONSTRAINT DF_BA_Def DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_BA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_BA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_BillingAddr PRIMARY KEY CLUSTERED (TenantId, BillingAddressId),
    CONSTRAINT FK_BA_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId)
);

CREATE TABLE billing.PaymentMethods (
    PaymentMethodId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PM_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Provider            VARCHAR(64)      NOT NULL,
    GatewayCustomerId   VARCHAR(128)     NULL,
    GatewayMethodId     VARCHAR(128)     NULL,
    Brand               VARCHAR(32)      NULL,             -- 'Visa', 'Mastercard'
    Last4               CHAR(4)          NULL,
    ExpMonth            TINYINT          NULL,
    ExpYear             SMALLINT         NULL,
    IsDefault           BIT              NOT NULL CONSTRAINT DF_PM_Def DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_PM_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_PM_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_PaymentMethods PRIMARY KEY CLUSTERED (TenantId, PaymentMethodId),
    CONSTRAINT UX_PM_Gateway UNIQUE (Provider, GatewayMethodId)
);

CREATE TABLE billing.TaxConfigurations (
    TaxConfigId         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TC_Id DEFAULT (NEWSEQUENTIALID()),
    CountryCode         CHAR(2)          NOT NULL,
    StateCode           VARCHAR(8)       NULL,
    TaxName             NVARCHAR(64)     NOT NULL,
    Rate                DECIMAL(9,6)     NOT NULL,
    EffectiveFromUtc    DATETIME2(3)     NOT NULL,
    EffectiveToUtc      DATETIME2(3)     NULL,
    CONSTRAINT PK_TaxConfig PRIMARY KEY CLUSTERED (TaxConfigId),
    CONSTRAINT UX_TaxConfig UNIQUE (CountryCode, StateCode, TaxName, EffectiveFromUtc)
);
```

## 8.4 Academic Management

```sql
CREATE TABLE academic.AcademicYears (
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AY_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(20)      NOT NULL,
    Name                NVARCHAR(100)    NOT NULL,
    StartDate           DATE             NOT NULL,
    EndDate             DATE             NOT NULL,
    IsCurrent           BIT              NOT NULL CONSTRAINT DF_AY_Cur DEFAULT (0),
    Status              TINYINT          NOT NULL,         -- 0=Planned,1=Active,2=Closed
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_AY_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_AY_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_AcademicYears PRIMARY KEY CLUSTERED (TenantId, AcademicYearId),
    CONSTRAINT UX_AY_Code UNIQUE (TenantId, Code),
    CONSTRAINT CK_AY_Range CHECK (EndDate > StartDate),
    CONSTRAINT CK_AY_Status CHECK (Status IN (0,1,2))
);

CREATE TABLE academic.Terms (
    TermId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_T_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(50)     NOT NULL,
    StartDate           DATE             NOT NULL,
    EndDate             DATE             NOT NULL,
    Sequence            TINYINT          NOT NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_T_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_T_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Terms PRIMARY KEY CLUSTERED (TenantId, TermId),
    CONSTRAINT UX_Terms_Seq UNIQUE (TenantId, AcademicYearId, Sequence),
    CONSTRAINT FK_Terms_AY FOREIGN KEY (TenantId, AcademicYearId) REFERENCES academic.AcademicYears(TenantId, AcademicYearId),
    CONSTRAINT CK_Terms_Range CHECK (EndDate > StartDate)
);

CREATE TABLE academic.Classes (
    ClassId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Cls_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(50)     NOT NULL,
    GradeLevel          SMALLINT         NOT NULL,
    DisplayOrder        SMALLINT         NOT NULL CONSTRAINT DF_Cls_Ord DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Cls_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Cls_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Classes PRIMARY KEY CLUSTERED (TenantId, ClassId),
    CONSTRAINT UX_Classes UNIQUE (TenantId, AcademicYearId, Name),
    CONSTRAINT FK_Cls_AY FOREIGN KEY (TenantId, AcademicYearId) REFERENCES academic.AcademicYears(TenantId, AcademicYearId)
);

CREATE TABLE academic.Sections (
    SectionId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Sec_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ClassId             UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(20)     NOT NULL,
    Capacity            SMALLINT         NOT NULL CONSTRAINT DF_Sec_Cap DEFAULT (40),
    ClassTeacherEmployeeId UNIQUEIDENTIFIER NULL,
    RoomCode            NVARCHAR(40)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Sec_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Sec_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Sections PRIMARY KEY CLUSTERED (TenantId, SectionId),
    CONSTRAINT UX_Sections UNIQUE (TenantId, ClassId, Name),
    CONSTRAINT FK_Sec_Cls FOREIGN KEY (TenantId, ClassId) REFERENCES academic.Classes(TenantId, ClassId)
);

CREATE TABLE academic.Subjects (
    SubjectId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Sub_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(100)    NOT NULL,
    IsElective          BIT              NOT NULL CONSTRAINT DF_Sub_Ele DEFAULT (0),
    DefaultMaxMarks     DECIMAL(9,2)     NOT NULL CONSTRAINT DF_Sub_MM DEFAULT (100),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Sub_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Sub_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Subjects PRIMARY KEY CLUSTERED (TenantId, SubjectId),
    CONSTRAINT UX_Subjects_Code UNIQUE (TenantId, Code)
);

CREATE TABLE academic.SubjectAssignments (
    SubjectAssignmentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SA_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    ClassId             UNIQUEIDENTIFIER NOT NULL,
    SectionId           UNIQUEIDENTIFIER NULL,
    SubjectId           UNIQUEIDENTIFIER NOT NULL,
    TeacherEmployeeId   UNIQUEIDENTIFIER NULL,
    HoursPerWeek        TINYINT          NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_SubjectAssignments PRIMARY KEY CLUSTERED (TenantId, SubjectAssignmentId),
    CONSTRAINT UX_SA UNIQUE (TenantId, AcademicYearId, ClassId, SectionId, SubjectId),
    CONSTRAINT FK_SA_AY  FOREIGN KEY (TenantId, AcademicYearId) REFERENCES academic.AcademicYears(TenantId, AcademicYearId),
    CONSTRAINT FK_SA_Cls FOREIGN KEY (TenantId, ClassId)        REFERENCES academic.Classes(TenantId, ClassId),
    CONSTRAINT FK_SA_Sub FOREIGN KEY (TenantId, SubjectId)      REFERENCES academic.Subjects(TenantId, SubjectId)
);

CREATE TABLE academic.Timetables (
    TimetableId         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TT_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    SectionId           UNIQUEIDENTIFIER NOT NULL,
    EffectiveFrom       DATE             NOT NULL,
    EffectiveTo         DATE             NULL,
    Status              TINYINT          NOT NULL CONSTRAINT DF_TT_St DEFAULT (0),  -- 0=Draft,1=Published,2=Archived
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_TT_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_TT_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Timetables PRIMARY KEY CLUSTERED (TenantId, TimetableId),
    CONSTRAINT FK_TT_Sec FOREIGN KEY (TenantId, SectionId) REFERENCES academic.Sections(TenantId, SectionId)
);

CREATE TABLE academic.Periods (
    PeriodId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_P_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    TimetableId         UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek           TINYINT          NOT NULL,         -- 1=Mon..7=Sun
    PeriodNumber        TINYINT          NOT NULL,
    StartTime           TIME(0)          NOT NULL,
    EndTime             TIME(0)          NOT NULL,
    SubjectId           UNIQUEIDENTIFIER NULL,
    TeacherEmployeeId   UNIQUEIDENTIFIER NULL,
    RoomCode            NVARCHAR(40)     NULL,
    IsBreak             BIT              NOT NULL CONSTRAINT DF_P_Br DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_P_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_P_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Periods PRIMARY KEY CLUSTERED (TenantId, PeriodId),
    CONSTRAINT UX_Periods UNIQUE (TenantId, TimetableId, DayOfWeek, PeriodNumber),
    CONSTRAINT FK_P_TT FOREIGN KEY (TenantId, TimetableId) REFERENCES academic.Timetables(TenantId, TimetableId),
    CONSTRAINT CK_Periods_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_Periods_Day CHECK (DayOfWeek BETWEEN 1 AND 7)
);

CREATE TABLE academic.ClassTeachers (
    ClassTeacherId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CT_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    SectionId           UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    AssignedFrom        DATE             NOT NULL,
    AssignedTo          DATE             NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_CT_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_CT_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_ClassTeachers PRIMARY KEY CLUSTERED (TenantId, ClassTeacherId),
    CONSTRAINT FK_CT_Sec FOREIGN KEY (TenantId, SectionId) REFERENCES academic.Sections(TenantId, SectionId)
);
```

## 8.5 Student Management

```sql
CREATE TABLE student.Parents (
    ParentId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Par_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,             -- optional self-service login
    FirstName           NVARCHAR(100)    NOT NULL,
    LastName            NVARCHAR(100)    NULL,
    Relationship        VARCHAR(20)      NOT NULL,         -- Father/Mother/Guardian
    Email               NVARCHAR(256)    NULL,
    PhoneNumber         VARCHAR(32)      NULL,
    AlternatePhone      VARCHAR(32)      NULL,
    Occupation          NVARCHAR(150)    NULL,
    AnnualIncome        DECIMAL(18,2)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Par_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Par_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Parents PRIMARY KEY CLUSTERED (TenantId, ParentId)
);

CREATE TABLE student.Students (
    StudentId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Stu_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AdmissionNumber     VARCHAR(40)      NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    FirstName           NVARCHAR(100)    NOT NULL,
    MiddleName          NVARCHAR(100)    NULL,
    LastName            NVARCHAR(100)    NULL,
    FullName            AS (CONCAT(FirstName, N' ', ISNULL(MiddleName + N' ', N''), ISNULL(LastName, N''))) PERSISTED,
    Gender              TINYINT          NOT NULL,         -- 0=Female,1=Male,2=Other
    DateOfBirth         DATE             NOT NULL,
    Nationality         NVARCHAR(80)     NULL,
    BloodGroup          VARCHAR(8)       NULL,
    Religion            NVARCHAR(64)     NULL,
    Category            NVARCHAR(64)     NULL,
    MotherTongue        NVARCHAR(64)     NULL,
    AadharNumberHash    BINARY(32)       NULL,             -- one-way hash of Aadhar
    PassportNumber      VARCHAR(32)      NULL,
    PhotoBlobKey        VARCHAR(512)     NULL,
    Status              TINYINT          NOT NULL,         -- 0=Applicant,1=Enrolled,2=Graduated,3=Withdrawn,4=Suspended
    AdmissionDate       DATE             NULL,
    ExitDate            DATE             NULL,
    Email               NVARCHAR(256)    NULL,
    PhoneNumber         VARCHAR(32)      NULL,
    EmergencyContact    VARCHAR(32)      NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Stu_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Stu_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (TenantId, StudentId),
    CONSTRAINT UX_Students_Adm UNIQUE (TenantId, AdmissionNumber),
    CONSTRAINT FK_Stu_Tenant FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(TenantId),
    CONSTRAINT CK_Stu_Status CHECK (Status IN (0,1,2,3,4))
);

CREATE TABLE student.StudentEnrollments (
    EnrollmentId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SE_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    ClassId             UNIQUEIDENTIFIER NOT NULL,
    SectionId           UNIQUEIDENTIFIER NOT NULL,
    RollNumber          VARCHAR(20)      NULL,
    EnrolledOn          DATE             NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Promoted,2=Detained,3=TC,4=Cancelled
    HouseName           NVARCHAR(50)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SE_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SE_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentEnrollments PRIMARY KEY CLUSTERED (TenantId, EnrollmentId),
    CONSTRAINT UX_SE_Active UNIQUE (TenantId, StudentId, AcademicYearId),
    CONSTRAINT UX_SE_Roll UNIQUE (TenantId, AcademicYearId, ClassId, SectionId, RollNumber),
    CONSTRAINT FK_SE_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId),
    CONSTRAINT FK_SE_AY  FOREIGN KEY (TenantId, AcademicYearId) REFERENCES academic.AcademicYears(TenantId, AcademicYearId),
    CONSTRAINT FK_SE_Cls FOREIGN KEY (TenantId, ClassId) REFERENCES academic.Classes(TenantId, ClassId),
    CONSTRAINT FK_SE_Sec FOREIGN KEY (TenantId, SectionId) REFERENCES academic.Sections(TenantId, SectionId)
);

CREATE TABLE student.StudentParents (
    StudentParentId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SP_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    ParentId            UNIQUEIDENTIFIER NOT NULL,
    IsPrimary           BIT              NOT NULL CONSTRAINT DF_SP_Prim DEFAULT (0),
    HasCustody          BIT              NOT NULL CONSTRAINT DF_SP_Cus DEFAULT (1),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SP_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SP_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentParents PRIMARY KEY CLUSTERED (TenantId, StudentId, ParentId),
    CONSTRAINT FK_SPx_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId),
    CONSTRAINT FK_SPx_Par FOREIGN KEY (TenantId, ParentId)  REFERENCES student.Parents(TenantId, ParentId)
);

CREATE TABLE student.StudentAddresses (
    AddressId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SAD_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    AddressType         TINYINT          NOT NULL,         -- 0=Permanent,1=Current,2=Other
    Line1               NVARCHAR(200)    NOT NULL,
    Line2               NVARCHAR(200)    NULL,
    City                NVARCHAR(100)    NOT NULL,
    State               NVARCHAR(100)    NULL,
    PostalCode          VARCHAR(20)      NULL,
    CountryCode         CHAR(2)          NOT NULL,
    IsDefault           BIT              NOT NULL CONSTRAINT DF_SAD_Def DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SAD_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SAD_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentAddresses PRIMARY KEY CLUSTERED (TenantId, StudentId, AddressId),
    CONSTRAINT FK_SAD_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);

CREATE TABLE student.StudentDocuments (
    DocumentId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SD_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    DocumentType        VARCHAR(40)      NOT NULL,         -- 'BirthCert','TC','Aadhar', etc.
    FileName            NVARCHAR(256)    NOT NULL,
    BlobKey             VARCHAR(512)     NOT NULL,
    MimeType            VARCHAR(100)     NULL,
    SizeBytes           BIGINT           NOT NULL,
    Sha256              BINARY(32)       NULL,
    ExpiresOn           DATE             NULL,
    UploadedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_SD_UA DEFAULT (SYSUTCDATETIME()),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SD_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SD_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentDocuments PRIMARY KEY CLUSTERED (TenantId, StudentId, DocumentId),
    CONSTRAINT FK_SD_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);

CREATE TABLE student.StudentMedicalRecords (
    MedicalRecordId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SMR_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    Allergies           NVARCHAR(MAX)    NULL,
    ChronicConditions   NVARCHAR(MAX)    NULL,
    Medications         NVARCHAR(MAX)    NULL,
    DoctorName          NVARCHAR(150)    NULL,
    DoctorPhone         VARCHAR(32)      NULL,
    InsuranceProvider   NVARCHAR(150)    NULL,
    InsuranceNumber     VARCHAR(64)      NULL,
    UpdatedAtUtc        DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SMR_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SMR_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentMedicalRecords PRIMARY KEY CLUSTERED (TenantId, StudentId, MedicalRecordId),
    CONSTRAINT FK_SMR_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);

CREATE TABLE student.StudentEmergencyContacts (
    ContactId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SEC_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Relationship        VARCHAR(40)      NOT NULL,
    PhoneNumber         VARCHAR(32)      NOT NULL,
    AlternatePhone      VARCHAR(32)      NULL,
    Priority            TINYINT          NOT NULL CONSTRAINT DF_SEC_Pr DEFAULT (1),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SEC_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SEC_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentEmergency PRIMARY KEY CLUSTERED (TenantId, StudentId, ContactId),
    CONSTRAINT FK_SEC_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);

CREATE TABLE student.StudentPromotionHistory (
    PromotionId         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SPH_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    FromAcademicYearId  UNIQUEIDENTIFIER NOT NULL,
    FromClassId         UNIQUEIDENTIFIER NOT NULL,
    FromSectionId       UNIQUEIDENTIFIER NOT NULL,
    ToAcademicYearId    UNIQUEIDENTIFIER NOT NULL,
    ToClassId           UNIQUEIDENTIFIER NOT NULL,
    ToSectionId         UNIQUEIDENTIFIER NOT NULL,
    PromotedOn          DATE             NOT NULL,
    Result              TINYINT          NOT NULL,         -- 0=Promoted,1=Detained,2=Withdrawn
    Notes               NVARCHAR(1000)   NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SPH_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SPH_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentPromotionHistory PRIMARY KEY CLUSTERED (TenantId, StudentId, PromotionId),
    CONSTRAINT FK_SPH_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);

CREATE TABLE student.StudentStatusHistory (
    StatusHistoryId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SSH_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    FromStatus          TINYINT          NOT NULL,
    ToStatus            TINYINT          NOT NULL,
    Reason              NVARCHAR(500)    NULL,
    OccurredAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_SSH_OA DEFAULT (SYSUTCDATETIME()),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SSH_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SSH_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentStatusHistory PRIMARY KEY CLUSTERED (TenantId, StudentId, StatusHistoryId),
    CONSTRAINT FK_SSH_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);
```

## 8.6 Employee Management

```sql
CREATE TABLE employee.Departments (
    DepartmentId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Dep_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    ParentDepartmentId  UNIQUEIDENTIFIER NULL,
    HeadEmployeeId      UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Dep_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Dep_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (TenantId, DepartmentId),
    CONSTRAINT UX_Dep_Code UNIQUE (TenantId, Code)
);

CREATE TABLE employee.Designations (
    DesignationId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Des_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Level               SMALLINT         NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Des_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Des_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Designations PRIMARY KEY CLUSTERED (TenantId, DesignationId),
    CONSTRAINT UX_Des_Code UNIQUE (TenantId, Code)
);

CREATE TABLE employee.Employees (
    EmployeeId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Emp_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeNumber      VARCHAR(40)      NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    FirstName           NVARCHAR(100)    NOT NULL,
    MiddleName          NVARCHAR(100)    NULL,
    LastName            NVARCHAR(100)    NULL,
    Gender              TINYINT          NOT NULL,
    DateOfBirth         DATE             NULL,
    Email               NVARCHAR(256)    NULL,
    PhoneNumber         VARCHAR(32)      NULL,
    DepartmentId        UNIQUEIDENTIFIER NULL,
    DesignationId       UNIQUEIDENTIFIER NULL,
    EmploymentType      TINYINT          NOT NULL,         -- 0=FullTime,1=PartTime,2=Contract,3=Intern
    HireDate            DATE             NOT NULL,
    TerminationDate     DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=OnLeave,2=Suspended,3=Terminated
    PhotoBlobKey        VARCHAR(512)     NULL,
    BasicSalary         DECIMAL(18,4)    NULL,
    Currency            CHAR(3)          NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_Emp_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Emp_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (TenantId, EmployeeId),
    CONSTRAINT UX_Emp_Number UNIQUE (TenantId, EmployeeNumber),
    CONSTRAINT FK_Emp_Dep FOREIGN KEY (TenantId, DepartmentId) REFERENCES employee.Departments(TenantId, DepartmentId),
    CONSTRAINT FK_Emp_Des FOREIGN KEY (TenantId, DesignationId) REFERENCES employee.Designations(TenantId, DesignationId)
);

CREATE TABLE employee.EmployeeDocuments (
    DocumentId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ED_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    DocumentType        VARCHAR(40)      NOT NULL,
    FileName            NVARCHAR(256)    NOT NULL,
    BlobKey             VARCHAR(512)     NOT NULL,
    SizeBytes           BIGINT           NOT NULL,
    Sha256              BINARY(32)       NULL,
    ExpiresOn           DATE             NULL,
    UploadedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ED_UA DEFAULT (SYSUTCDATETIME()),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_ED_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_ED_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_EmployeeDocuments PRIMARY KEY CLUSTERED (TenantId, EmployeeId, DocumentId),
    CONSTRAINT FK_ED_Emp FOREIGN KEY (TenantId, EmployeeId) REFERENCES employee.Employees(TenantId, EmployeeId)
);

CREATE TABLE employee.EmployeeAttendance (
    EmployeeAttendanceId BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    AttendanceDate      DATE             NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Present,1=Absent,2=Leave,3=Holiday,4=Half
    CheckInUtc          DATETIME2(3)     NULL,
    CheckOutUtc         DATETIME2(3)     NULL,
    HoursWorked         DECIMAL(5,2)     NULL,
    Source              VARCHAR(32)      NULL,             -- 'Biometric','Manual','Mobile'
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_EA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_EA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_EmployeeAttendance PRIMARY KEY CLUSTERED (TenantId, EmployeeId, AttendanceDate),
    CONSTRAINT UX_EA_Identity UNIQUE (EmployeeAttendanceId),
    CONSTRAINT FK_EA_Emp FOREIGN KEY (TenantId, EmployeeId) REFERENCES employee.Employees(TenantId, EmployeeId)
);

CREATE TABLE employee.EmployeePayroll (
    PayrollId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EP_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    PayrollMonth        TINYINT          NOT NULL,
    PayrollYear         SMALLINT         NOT NULL,
    BasicSalary         DECIMAL(18,4)    NOT NULL,
    Allowances          DECIMAL(18,4)    NOT NULL CONSTRAINT DF_EP_Allow DEFAULT (0),
    Deductions          DECIMAL(18,4)    NOT NULL CONSTRAINT DF_EP_Ded DEFAULT (0),
    Tax                 DECIMAL(18,4)    NOT NULL CONSTRAINT DF_EP_Tax DEFAULT (0),
    Bonus               DECIMAL(18,4)    NOT NULL CONSTRAINT DF_EP_Bon DEFAULT (0),
    Net                 AS (BasicSalary + Allowances + Bonus - Deductions - Tax) PERSISTED,
    Currency            CHAR(3)          NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Draft,1=Approved,2=Paid
    PaidAtUtc           DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_EP_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_EP_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_EmployeePayroll PRIMARY KEY CLUSTERED (TenantId, EmployeeId, PayrollYear, PayrollMonth),
    CONSTRAINT UX_EP_PK UNIQUE (PayrollId),
    CONSTRAINT FK_EP_Emp FOREIGN KEY (TenantId, EmployeeId) REFERENCES employee.Employees(TenantId, EmployeeId),
    CONSTRAINT CK_EP_Month CHECK (PayrollMonth BETWEEN 1 AND 12)
);

CREATE TABLE employee.EmployeeBankAccounts (
    BankAccountId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EBA_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    BankName            NVARCHAR(150)    NOT NULL,
    BranchName          NVARCHAR(150)    NULL,
    AccountNumberHash   BINARY(32)       NOT NULL,
    AccountNumberMasked VARCHAR(40)      NOT NULL,
    IfscOrSwiftCode     VARCHAR(32)      NULL,
    IsDefault           BIT              NOT NULL CONSTRAINT DF_EBA_Def DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_EBA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_EBA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_EmployeeBankAccounts PRIMARY KEY CLUSTERED (TenantId, EmployeeId, BankAccountId),
    CONSTRAINT FK_EBA_Emp FOREIGN KEY (TenantId, EmployeeId) REFERENCES employee.Employees(TenantId, EmployeeId)
);
```

## 8.7 Attendance Management

```sql
CREATE TABLE attendance.AttendanceSessions (
    SessionId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AS_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    SectionId           UNIQUEIDENTIFIER NOT NULL,
    PeriodId            UNIQUEIDENTIFIER NULL,
    SessionDate         DATE             NOT NULL,
    SessionType         TINYINT          NOT NULL,         -- 0=Daily,1=Period,2=Event
    Status              TINYINT          NOT NULL,         -- 0=Open,1=Finalized
    TakenByUserId       UNIQUEIDENTIFIER NULL,
    FinalizedAtUtc      DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_AS_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_AS_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_AttSessions PRIMARY KEY CLUSTERED (TenantId, SessionDate, SectionId, SessionId),
    CONSTRAINT UX_AttSessions UNIQUE (TenantId, SectionId, SessionDate, PeriodId),
    CONSTRAINT FK_AS_Sec FOREIGN KEY (TenantId, SectionId) REFERENCES academic.Sections(TenantId, SectionId)
);

-- Partitioned (see 04-INDEXING for partition function PF_Att_ByMonth)
CREATE TABLE attendance.StudentAttendance (
    StudentAttendanceId BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    SessionId           UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    SessionDate         DATE             NOT NULL,         -- denormalized for partitioning
    Status              TINYINT          NOT NULL,         -- 0=Present,1=Absent,2=Late,3=Leave,4=Excused
    Remarks             NVARCHAR(400)    NULL,
    MarkedAtUtc         DATETIME2(3)     NOT NULL CONSTRAINT DF_SA_MA DEFAULT (SYSUTCDATETIME()),
    MarkedByUserId      UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentAttendance PRIMARY KEY CLUSTERED (SessionDate, TenantId, SessionId, StudentId)
        ON PS_Att_ByMonth(SessionDate),
    CONSTRAINT UX_SA_Identity UNIQUE (StudentAttendanceId),
    CONSTRAINT FK_SA_Sess FOREIGN KEY (TenantId, SessionDate, SessionId)
        REFERENCES attendance.AttendanceSessions(TenantId, SessionDate, SessionId) -- composite ref to align partitioning
);
```

## 8.8 Examination Management

```sql
CREATE TABLE exam.Grades (
    GradeId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_G_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ScaleName           NVARCHAR(60)     NOT NULL,         -- 'CBSE-2024'
    GradeName           NVARCHAR(10)     NOT NULL,         -- 'A+','A','B'
    MinPercent          DECIMAL(5,2)     NOT NULL,
    MaxPercent          DECIMAL(5,2)     NOT NULL,
    GpaPoint            DECIMAL(4,2)     NULL,
    Remarks             NVARCHAR(200)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_G_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_G_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Grades PRIMARY KEY CLUSTERED (TenantId, GradeId),
    CONSTRAINT UX_Grades UNIQUE (TenantId, ScaleName, GradeName),
    CONSTRAINT CK_Grades_Range CHECK (MaxPercent > MinPercent AND MinPercent >= 0 AND MaxPercent <= 100)
);

CREATE TABLE exam.Exams (
    ExamId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_E_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    TermId              UNIQUEIDENTIFIER NULL,
    Name                NVARCHAR(150)    NOT NULL,
    ExamType            TINYINT          NOT NULL,         -- 0=Unit,1=Mid,2=Final,3=Practical
    GradeScale          NVARCHAR(60)     NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Draft,1=Scheduled,2=InProgress,3=Completed,4=Published
    StartDate           DATE             NOT NULL,
    EndDate             DATE             NOT NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_E_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_E_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Exams PRIMARY KEY CLUSTERED (TenantId, ExamId),
    CONSTRAINT FK_E_AY FOREIGN KEY (TenantId, AcademicYearId) REFERENCES academic.AcademicYears(TenantId, AcademicYearId),
    CONSTRAINT CK_E_Range CHECK (EndDate >= StartDate)
);

CREATE TABLE exam.ExamSchedules (
    ExamScheduleId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ES_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ExamId              UNIQUEIDENTIFIER NOT NULL,
    ClassId             UNIQUEIDENTIFIER NOT NULL,
    SectionId           UNIQUEIDENTIFIER NULL,
    ExamDate            DATE             NOT NULL,
    StartTime           TIME(0)          NOT NULL,
    EndTime             TIME(0)          NOT NULL,
    RoomCode            NVARCHAR(40)     NULL,
    InvigilatorEmployeeId UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_ES_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_ES_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_ExamSchedules PRIMARY KEY CLUSTERED (TenantId, ExamScheduleId),
    CONSTRAINT FK_ES_E FOREIGN KEY (TenantId, ExamId) REFERENCES exam.Exams(TenantId, ExamId),
    CONSTRAINT CK_ES_Time CHECK (EndTime > StartTime)
);

CREATE TABLE exam.ExamSubjects (
    ExamSubjectId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ESu_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ExamScheduleId      UNIQUEIDENTIFIER NOT NULL,
    SubjectId           UNIQUEIDENTIFIER NOT NULL,
    MaxMarks            DECIMAL(9,2)     NOT NULL,
    PassMarks           DECIMAL(9,2)     NOT NULL,
    Weightage           DECIMAL(5,2)     NOT NULL CONSTRAINT DF_ESu_W DEFAULT (100),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_ESu_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_ESu_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_ExamSubjects PRIMARY KEY CLUSTERED (TenantId, ExamSubjectId),
    CONSTRAINT UX_ExamSubjects UNIQUE (TenantId, ExamScheduleId, SubjectId),
    CONSTRAINT FK_ESu_Sched FOREIGN KEY (TenantId, ExamScheduleId) REFERENCES exam.ExamSchedules(TenantId, ExamScheduleId),
    CONSTRAINT CK_ESu_Marks CHECK (PassMarks <= MaxMarks)
);

CREATE TABLE exam.ExamResults (
    ExamResultId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ER_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ExamId              UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    ExamSubjectId       UNIQUEIDENTIFIER NOT NULL,
    MarksObtained       DECIMAL(9,2)     NULL,
    Percentage          DECIMAL(6,3)     NULL,
    GradeId             UNIQUEIDENTIFIER NULL,
    IsAbsent            BIT              NOT NULL CONSTRAINT DF_ER_Abs DEFAULT (0),
    Remarks             NVARCHAR(400)    NULL,
    EvaluatedAtUtc      DATETIME2(3)     NULL,
    EvaluatedByEmployeeId UNIQUEIDENTIFIER NULL,
    PublishedAtUtc      DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_ER_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_ER_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_ExamResults PRIMARY KEY CLUSTERED (TenantId, ExamResultId),
    CONSTRAINT UX_ExamResults UNIQUE (TenantId, ExamId, StudentId, ExamSubjectId),
    CONSTRAINT FK_ER_E FOREIGN KEY (TenantId, ExamId) REFERENCES exam.Exams(TenantId, ExamId),
    CONSTRAINT FK_ER_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId),
    CONSTRAINT FK_ER_ESu FOREIGN KEY (TenantId, ExamSubjectId) REFERENCES exam.ExamSubjects(TenantId, ExamSubjectId)
);

CREATE TABLE exam.ReportCards (
    ReportCardId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RC_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ExamId              UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    TotalMarks          DECIMAL(12,2)    NULL,
    MaxTotal            DECIMAL(12,2)    NULL,
    Percentage          DECIMAL(6,3)     NULL,
    OverallGradeId      UNIQUEIDENTIFIER NULL,
    Rank                INT              NULL,
    Result              TINYINT          NULL,             -- 0=Pass,1=Fail,2=Compartment,3=Absent
    PdfBlobKey          VARCHAR(512)     NULL,
    PublishedAtUtc      DATETIME2(3)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_RC_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_RC_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_ReportCards PRIMARY KEY CLUSTERED (TenantId, ReportCardId),
    CONSTRAINT UX_RC UNIQUE (TenantId, ExamId, StudentId),
    CONSTRAINT FK_RC_E FOREIGN KEY (TenantId, ExamId) REFERENCES exam.Exams(TenantId, ExamId),
    CONSTRAINT FK_RC_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);
```

## 8.9 Fee Management

```sql
CREATE TABLE fees.FeeTypes (
    FeeTypeId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FT_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Frequency           TINYINT          NOT NULL,         -- 0=OneTime,1=Monthly,2=Quarterly,3=Annual
    IsTaxable           BIT              NOT NULL CONSTRAINT DF_FT_Tx DEFAULT (0),
    IsRefundable        BIT              NOT NULL CONSTRAINT DF_FT_Re DEFAULT (1),
    GlAccountCode       VARCHAR(40)      NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FT_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FT_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeTypes PRIMARY KEY CLUSTERED (TenantId, FeeTypeId),
    CONSTRAINT UX_FT_Code UNIQUE (TenantId, Code)
);

CREATE TABLE fees.FeeStructures (
    FeeStructureId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FS_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    ClassId             UNIQUEIDENTIFIER NULL,
    Name                NVARCHAR(150)    NOT NULL,
    TotalAmount         DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL CONSTRAINT DF_FS_Cur DEFAULT ('USD'),
    EffectiveFrom       DATE             NOT NULL,
    EffectiveTo         DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Draft,1=Active,2=Archived
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FS_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FS_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeStructures PRIMARY KEY CLUSTERED (TenantId, FeeStructureId),
    CONSTRAINT FK_FS_AY FOREIGN KEY (TenantId, AcademicYearId) REFERENCES academic.AcademicYears(TenantId, AcademicYearId),
    CONSTRAINT FK_FS_Cls FOREIGN KEY (TenantId, ClassId) REFERENCES academic.Classes(TenantId, ClassId)
);

CREATE TABLE fees.FeeInstallments (
    InstallmentId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FI_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    FeeStructureId      UNIQUEIDENTIFIER NOT NULL,
    FeeTypeId           UNIQUEIDENTIFIER NOT NULL,
    Sequence            TINYINT          NOT NULL,
    DueDate             DATE             NOT NULL,
    Amount              DECIMAL(18,4)    NOT NULL,
    LateFeePerDay       DECIMAL(18,4)    NULL,
    GracePeriodDays     INT              NOT NULL CONSTRAINT DF_FI_GP DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FI_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FI_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeInstallments PRIMARY KEY CLUSTERED (TenantId, InstallmentId),
    CONSTRAINT UX_FI UNIQUE (TenantId, FeeStructureId, FeeTypeId, Sequence),
    CONSTRAINT FK_FI_FS FOREIGN KEY (TenantId, FeeStructureId) REFERENCES fees.FeeStructures(TenantId, FeeStructureId),
    CONSTRAINT FK_FI_FT FOREIGN KEY (TenantId, FeeTypeId) REFERENCES fees.FeeTypes(TenantId, FeeTypeId)
);

CREATE TABLE fees.StudentFeeAssignments (
    AssignmentId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SFA_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    FeeStructureId      UNIQUEIDENTIFIER NOT NULL,
    DiscountPercent     DECIMAL(5,2)     NOT NULL CONSTRAINT DF_SFA_Disc DEFAULT (0),
    AssignedOn          DATE             NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Cancelled
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_SFA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_SFA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentFeeAssignments PRIMARY KEY CLUSTERED (TenantId, AssignmentId),
    CONSTRAINT UX_SFA UNIQUE (TenantId, StudentId, AcademicYearId, FeeStructureId),
    CONSTRAINT FK_SFA_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId),
    CONSTRAINT FK_SFA_FS FOREIGN KEY (TenantId, FeeStructureId) REFERENCES fees.FeeStructures(TenantId, FeeStructureId)
);

-- Partitioned by IssuedOn (yearly)
CREATE TABLE fees.FeeInvoices (
    InvoiceId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FInv_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    AssignmentId        UNIQUEIDENTIFIER NULL,
    Number              VARCHAR(40)      NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Draft,1=Issued,2=PartPaid,3=Paid,4=Overdue,5=Cancelled
    Subtotal            DECIMAL(18,4)    NOT NULL,
    DiscountTotal       DECIMAL(18,4)    NOT NULL CONSTRAINT DF_FInv_Disc DEFAULT (0),
    TaxTotal            DECIMAL(18,4)    NOT NULL CONSTRAINT DF_FInv_Tax DEFAULT (0),
    Total               AS (Subtotal - DiscountTotal + TaxTotal) PERSISTED,
    AmountPaid          DECIMAL(18,4)    NOT NULL CONSTRAINT DF_FInv_AP DEFAULT (0),
    AmountDue           AS ((Subtotal - DiscountTotal + TaxTotal) - AmountPaid) PERSISTED,
    Currency            CHAR(3)          NOT NULL,
    IssuedOn            DATE             NOT NULL,
    DueOn               DATE             NOT NULL,
    PdfBlobKey          VARCHAR(512)     NULL,
    Notes               NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FInv_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FInv_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeInvoices PRIMARY KEY CLUSTERED (IssuedOn, TenantId, InvoiceId)
        ON PS_Fees_ByYear(IssuedOn),
    CONSTRAINT UX_FInv_Number UNIQUE (TenantId, Number),
    CONSTRAINT FK_FInv_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId)
);

CREATE TABLE fees.FeeInvoiceLines (
    LineId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FIL_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    InvoiceId           UNIQUEIDENTIFIER NOT NULL,
    IssuedOn            DATE             NOT NULL,
    FeeTypeId           UNIQUEIDENTIFIER NOT NULL,
    InstallmentId       UNIQUEIDENTIFIER NULL,
    Description         NVARCHAR(300)    NOT NULL,
    Quantity            DECIMAL(9,2)     NOT NULL CONSTRAINT DF_FIL_Q DEFAULT (1),
    UnitPrice           DECIMAL(18,4)    NOT NULL,
    DiscountAmount      DECIMAL(18,4)    NOT NULL CONSTRAINT DF_FIL_Dis DEFAULT (0),
    TaxRate             DECIMAL(9,4)     NOT NULL CONSTRAINT DF_FIL_TR DEFAULT (0),
    LineTotal           AS ((Quantity * UnitPrice - DiscountAmount) * (1 + TaxRate / 100.0)) PERSISTED,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FIL_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FIL_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeInvoiceLines PRIMARY KEY CLUSTERED (IssuedOn, TenantId, InvoiceId, LineId)
        ON PS_Fees_ByYear(IssuedOn),
    CONSTRAINT FK_FIL_Inv FOREIGN KEY (IssuedOn, TenantId, InvoiceId) REFERENCES fees.FeeInvoices(IssuedOn, TenantId, InvoiceId),
    CONSTRAINT FK_FIL_FT  FOREIGN KEY (TenantId, FeeTypeId) REFERENCES fees.FeeTypes(TenantId, FeeTypeId)
);

CREATE TABLE fees.FeePayments (
    PaymentId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FP_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    InvoiceId           UNIQUEIDENTIFIER NOT NULL,
    IssuedOn            DATE             NOT NULL,
    Amount              DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL,
    Method              TINYINT          NOT NULL,         -- 0=Cash,1=Card,2=NetBanking,3=UPI,4=Wallet,5=BankTransfer,6=Cheque
    GatewayProvider     VARCHAR(64)      NULL,
    GatewayPaymentId    VARCHAR(128)     NULL,
    GatewayOrderId      VARCHAR(128)     NULL,
    Status              TINYINT          NOT NULL,         -- 0=Initiated,1=Succeeded,2=Failed,3=Refunded,4=PartRefund
    ReceivedAtUtc       DATETIME2(3)     NOT NULL,
    ReceiptNumber       VARCHAR(40)      NULL,
    ReceiptBlobKey      VARCHAR(512)     NULL,
    Notes               NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FP_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FP_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeePayments PRIMARY KEY CLUSTERED (TenantId, PaymentId),
    CONSTRAINT UX_FP_Gateway UNIQUE (GatewayProvider, GatewayPaymentId),
    CONSTRAINT UX_FP_Receipt UNIQUE (TenantId, ReceiptNumber),
    CONSTRAINT FK_FP_Inv FOREIGN KEY (IssuedOn, TenantId, InvoiceId) REFERENCES fees.FeeInvoices(IssuedOn, TenantId, InvoiceId)
);

CREATE TABLE fees.FeeDiscounts (
    DiscountId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FD_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    DiscountType        TINYINT          NOT NULL,         -- 0=Percent,1=Fixed
    Value               DECIMAL(18,4)    NOT NULL,
    Scope               TINYINT          NOT NULL,         -- 0=Student,1=Class,2=Structure,3=Sibling
    StudentId           UNIQUEIDENTIFIER NULL,
    ClassId             UNIQUEIDENTIFIER NULL,
    FeeStructureId      UNIQUEIDENTIFIER NULL,
    ValidFrom           DATE             NOT NULL,
    ValidTo             DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Inactive
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FD_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FD_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeDiscounts PRIMARY KEY CLUSTERED (TenantId, DiscountId),
    CONSTRAINT UX_FD_Code UNIQUE (TenantId, Code)
);

CREATE TABLE fees.FeeRefunds (
    RefundId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FR_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    PaymentId           UNIQUEIDENTIFIER NOT NULL,
    Amount              DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL,
    Reason              NVARCHAR(500)    NULL,
    Status              TINYINT          NOT NULL,         -- 0=Init,1=Approved,2=Rejected,3=Processed
    ApprovedByUserId    UNIQUEIDENTIFIER NULL,
    ProcessedAtUtc      DATETIME2(3)     NULL,
    GatewayRefundId     VARCHAR(128)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_FR_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_FR_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_FeeRefunds PRIMARY KEY CLUSTERED (TenantId, RefundId),
    CONSTRAINT FK_FR_FP FOREIGN KEY (TenantId, PaymentId) REFERENCES fees.FeePayments(TenantId, PaymentId)
);
```

## 8.10 Transport Management

```sql
CREATE TABLE transport.Vehicles (
    VehicleId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_V_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    RegistrationNumber  VARCHAR(40)      NOT NULL,
    VehicleType         TINYINT          NOT NULL,         -- 0=Bus,1=Van,2=Cab
    Make                NVARCHAR(80)     NULL,
    Model               NVARCHAR(80)     NULL,
    Year                SMALLINT         NULL,
    Capacity            SMALLINT         NOT NULL,
    InsuranceExpiresOn  DATE             NULL,
    PucExpiresOn        DATE             NULL,
    FitnessExpiresOn    DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=InMaintenance,2=Retired
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_V_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_V_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Vehicles PRIMARY KEY CLUSTERED (TenantId, VehicleId),
    CONSTRAINT UX_V_Reg UNIQUE (TenantId, RegistrationNumber)
);

CREATE TABLE transport.Drivers (
    DriverId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_D_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NULL,
    FirstName           NVARCHAR(100)    NOT NULL,
    LastName            NVARCHAR(100)    NULL,
    LicenseNumber       VARCHAR(40)      NOT NULL,
    LicenseExpiresOn    DATE             NULL,
    PhoneNumber         VARCHAR(32)      NOT NULL,
    AlternatePhone      VARCHAR(32)      NULL,
    Address             NVARCHAR(400)    NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Suspended,2=Terminated
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_D_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_D_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Drivers PRIMARY KEY CLUSTERED (TenantId, DriverId),
    CONSTRAINT UX_D_License UNIQUE (TenantId, LicenseNumber)
);

CREATE TABLE transport.Routes (
    RouteId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RT_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Description         NVARCHAR(500)    NULL,
    DistanceKm          DECIMAL(8,2)     NULL,
    EstimatedDurationMin INT             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Inactive
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_RT_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_RT_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Routes PRIMARY KEY CLUSTERED (TenantId, RouteId),
    CONSTRAINT UX_RT_Code UNIQUE (TenantId, Code)
);

CREATE TABLE transport.Stops (
    StopId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_S_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    RouteId             UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Sequence            SMALLINT         NOT NULL,
    Latitude            DECIMAL(9,6)     NULL,
    Longitude           DECIMAL(9,6)     NULL,
    PickupTime          TIME(0)          NULL,
    DropTime            TIME(0)          NULL,
    Fee                 DECIMAL(18,4)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_S_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_S_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Stops PRIMARY KEY CLUSTERED (TenantId, StopId),
    CONSTRAINT UX_S UNIQUE (TenantId, RouteId, Sequence),
    CONSTRAINT FK_S_RT FOREIGN KEY (TenantId, RouteId) REFERENCES transport.Routes(TenantId, RouteId)
);

CREATE TABLE transport.VehicleAssignments (
    AssignmentId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_VA_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    VehicleId           UNIQUEIDENTIFIER NOT NULL,
    RouteId             UNIQUEIDENTIFIER NOT NULL,
    DriverId            UNIQUEIDENTIFIER NOT NULL,
    AlternateDriverId   UNIQUEIDENTIFIER NULL,
    AttenderEmployeeId  UNIQUEIDENTIFIER NULL,
    AssignedFrom        DATE             NOT NULL,
    AssignedTo          DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Ended
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_VA_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_VA_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_VehicleAssignments PRIMARY KEY CLUSTERED (TenantId, AssignmentId),
    CONSTRAINT FK_VA_V FOREIGN KEY (TenantId, VehicleId) REFERENCES transport.Vehicles(TenantId, VehicleId),
    CONSTRAINT FK_VA_RT FOREIGN KEY (TenantId, RouteId) REFERENCES transport.Routes(TenantId, RouteId),
    CONSTRAINT FK_VA_D FOREIGN KEY (TenantId, DriverId) REFERENCES transport.Drivers(TenantId, DriverId)
);

CREATE TABLE transport.StudentTransportAssignments (
    StudentTransportId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ST_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    AcademicYearId      UNIQUEIDENTIFIER NOT NULL,
    RouteId             UNIQUEIDENTIFIER NOT NULL,
    PickupStopId        UNIQUEIDENTIFIER NOT NULL,
    DropStopId          UNIQUEIDENTIFIER NOT NULL,
    VehicleId           UNIQUEIDENTIFIER NULL,
    EffectiveFrom       DATE             NOT NULL,
    EffectiveTo         DATE             NULL,
    Fee                 DECIMAL(18,4)    NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Cancelled
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_ST_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_ST_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_StudentTransport PRIMARY KEY CLUSTERED (TenantId, StudentTransportId),
    CONSTRAINT UX_ST UNIQUE (TenantId, StudentId, AcademicYearId),
    CONSTRAINT FK_ST_Stu FOREIGN KEY (TenantId, StudentId) REFERENCES student.Students(TenantId, StudentId),
    CONSTRAINT FK_ST_RT  FOREIGN KEY (TenantId, RouteId)   REFERENCES transport.Routes(TenantId, RouteId)
);

CREATE TABLE transport.GPSDevices (
    GpsDeviceId         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_GD_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    VehicleId           UNIQUEIDENTIFIER NOT NULL,
    Imei                VARCHAR(32)      NOT NULL,
    SimNumber           VARCHAR(32)      NULL,
    Provider            VARCHAR(64)      NULL,
    InstalledOn         DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Faulty,2=Removed
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_GD_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_GD_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_GPSDevices PRIMARY KEY CLUSTERED (TenantId, GpsDeviceId),
    CONSTRAINT UX_GD_Imei UNIQUE (Imei),
    CONSTRAINT FK_GD_V FOREIGN KEY (TenantId, VehicleId) REFERENCES transport.Vehicles(TenantId, VehicleId)
);

-- High-volume time-series; partition by date, archive after 90 days
CREATE TABLE transport.GPSPings (
    GpsPingId           BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    GpsDeviceId         UNIQUEIDENTIFIER NOT NULL,
    VehicleId           UNIQUEIDENTIFIER NOT NULL,
    PingedAtUtc         DATETIME2(0)     NOT NULL,
    Latitude            DECIMAL(9,6)     NOT NULL,
    Longitude           DECIMAL(9,6)     NOT NULL,
    SpeedKph            DECIMAL(6,2)     NULL,
    Heading             SMALLINT         NULL,
    IgnitionOn          BIT              NULL,
    PingDate AS (CAST(PingedAtUtc AS DATE)) PERSISTED,
    CONSTRAINT PK_GPSPings PRIMARY KEY CLUSTERED (PingDate, TenantId, GpsDeviceId, GpsPingId)
        ON PS_Gps_ByMonth(PingDate)
);
```

## 8.11 Communication Management

```sql
CREATE TABLE comm.NotificationTemplates (
    TemplateId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NT_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(64)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    Channel             TINYINT          NOT NULL,         -- 0=Email,1=Sms,2=WhatsApp,3=Push,4=InApp
    Subject             NVARCHAR(400)    NULL,
    BodyTemplate        NVARCHAR(MAX)    NOT NULL,
    Locale              VARCHAR(16)      NOT NULL CONSTRAINT DF_NT_Loc DEFAULT ('en-US'),
    Version             INT              NOT NULL CONSTRAINT DF_NT_Ver DEFAULT (1),
    IsActive            BIT              NOT NULL CONSTRAINT DF_NT_Act DEFAULT (1),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_NT_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_NT_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_NotificationTemplates PRIMARY KEY CLUSTERED (TenantId, TemplateId),
    CONSTRAINT UX_NT UNIQUE (TenantId, Code, Channel, Locale, Version)
);

CREATE TABLE comm.Notifications (
    NotificationId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_N_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    TemplateId          UNIQUEIDENTIFIER NULL,
    Channel             TINYINT          NOT NULL,
    Subject             NVARCHAR(400)    NULL,
    Body                NVARCHAR(MAX)    NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Queued,1=Sending,2=Sent,3=PartFailed,4=Failed,5=Cancelled
    PriorityLevel       TINYINT          NOT NULL CONSTRAINT DF_N_Pri DEFAULT (5),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_N_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_N_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (TenantId, NotificationId),
    CONSTRAINT FK_N_T FOREIGN KEY (TenantId, TemplateId) REFERENCES comm.NotificationTemplates(TenantId, TemplateId)
);

CREATE TABLE comm.NotificationRecipients (
    RecipientId         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NR_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    NotificationId      UNIQUEIDENTIFIER NOT NULL,
    RecipientType       TINYINT          NOT NULL,         -- 0=User,1=Parent,2=Student,3=Staff,4=Adhoc
    RecipientId_        UNIQUEIDENTIFIER NULL,
    Address             NVARCHAR(256)    NOT NULL,         -- email/phone resolved at send-time
    Status              TINYINT          NOT NULL,         -- 0=Pending,1=Sent,2=Delivered,3=Read,4=Failed,5=Bounced
    SentAtUtc           DATETIME2(3)     NULL,
    DeliveredAtUtc      DATETIME2(3)     NULL,
    ReadAtUtc           DATETIME2(3)     NULL,
    FailureReason       NVARCHAR(500)    NULL,
    Attempts            INT              NOT NULL CONSTRAINT DF_NR_Att DEFAULT (0),
    CONSTRAINT PK_NotificationRecipients PRIMARY KEY CLUSTERED (TenantId, NotificationId, RecipientId),
    CONSTRAINT FK_NR_N FOREIGN KEY (TenantId, NotificationId) REFERENCES comm.Notifications(TenantId, NotificationId)
);

CREATE TABLE comm.EmailLogs (
    EmailLogId          BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    NotificationId      UNIQUEIDENTIFIER NULL,
    Provider            VARCHAR(64)      NOT NULL,
    ProviderMessageId   VARCHAR(128)     NULL,
    FromAddress         NVARCHAR(256)    NOT NULL,
    ToAddress           NVARCHAR(256)    NOT NULL,
    Subject             NVARCHAR(400)    NULL,
    Status              TINYINT          NOT NULL,
    SentAtUtc           DATETIME2(3)     NOT NULL,
    DeliveredAtUtc      DATETIME2(3)     NULL,
    OpenedAtUtc         DATETIME2(3)     NULL,
    BouncedAtUtc        DATETIME2(3)     NULL,
    Cost                DECIMAL(18,6)    NULL,
    CONSTRAINT PK_EmailLogs PRIMARY KEY CLUSTERED (TenantId, SentAtUtc, EmailLogId)
);

CREATE TABLE comm.SmsLogs (
    SmsLogId            BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    NotificationId      UNIQUEIDENTIFIER NULL,
    Provider            VARCHAR(64)      NOT NULL,
    ProviderMessageId   VARCHAR(128)     NULL,
    SenderId            VARCHAR(32)      NULL,
    ToNumber            VARCHAR(32)      NOT NULL,
    Body                NVARCHAR(2000)   NOT NULL,
    Status              TINYINT          NOT NULL,
    SentAtUtc           DATETIME2(3)     NOT NULL,
    DeliveredAtUtc      DATETIME2(3)     NULL,
    Segments            TINYINT          NULL,
    Cost                DECIMAL(18,6)    NULL,
    CONSTRAINT PK_SmsLogs PRIMARY KEY CLUSTERED (TenantId, SentAtUtc, SmsLogId)
);

CREATE TABLE comm.WhatsAppLogs (
    WhatsAppLogId       BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    NotificationId      UNIQUEIDENTIFIER NULL,
    Provider            VARCHAR(64)      NOT NULL,
    ProviderMessageId   VARCHAR(128)     NULL,
    ToNumber            VARCHAR(32)      NOT NULL,
    TemplateName        VARCHAR(128)     NULL,
    Status              TINYINT          NOT NULL,
    SentAtUtc           DATETIME2(3)     NOT NULL,
    DeliveredAtUtc      DATETIME2(3)     NULL,
    ReadAtUtc           DATETIME2(3)     NULL,
    Cost                DECIMAL(18,6)    NULL,
    CONSTRAINT PK_WhatsAppLogs PRIMARY KEY CLUSTERED (TenantId, SentAtUtc, WhatsAppLogId)
);

CREATE TABLE comm.PushLogs (
    PushLogId           BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    NotificationId      UNIQUEIDENTIFIER NULL,
    Provider            VARCHAR(64)      NOT NULL,
    DeviceToken         VARCHAR(512)     NOT NULL,
    Platform            TINYINT          NOT NULL,         -- 0=iOS,1=Android,2=Web
    Status              TINYINT          NOT NULL,
    SentAtUtc           DATETIME2(3)     NOT NULL,
    CONSTRAINT PK_PushLogs PRIMARY KEY CLUSTERED (TenantId, SentAtUtc, PushLogId)
);
```

## 8.12 Library Management

```sql
CREATE TABLE library_.BookCategories (
    CategoryId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BC_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    ParentCategoryId    UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_BC_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_BC_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_BookCategories PRIMARY KEY CLUSTERED (TenantId, CategoryId),
    CONSTRAINT UX_BC UNIQUE (TenantId, Code)
);

CREATE TABLE library_.Books (
    BookId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_B_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    CategoryId          UNIQUEIDENTIFIER NULL,
    Isbn                VARCHAR(20)      NULL,
    Title               NVARCHAR(300)    NOT NULL,
    Author              NVARCHAR(200)    NULL,
    Publisher           NVARCHAR(200)    NULL,
    PublishedYear       SMALLINT         NULL,
    Edition             NVARCHAR(40)     NULL,
    Language            VARCHAR(16)      NULL,
    Pages               INT              NULL,
    Price               DECIMAL(18,4)    NULL,
    Currency            CHAR(3)          NULL,
    CoverBlobKey        VARCHAR(512)     NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_B_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_B_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Books PRIMARY KEY CLUSTERED (TenantId, BookId),
    CONSTRAINT UX_B_Isbn UNIQUE (TenantId, Isbn)
);

CREATE TABLE library_.BookCopies (
    CopyId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BCp_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    BookId              UNIQUEIDENTIFIER NOT NULL,
    AccessionNumber     VARCHAR(40)      NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Available,1=Issued,2=Lost,3=Damaged,4=Reserved
    AcquiredOn          DATE             NULL,
    Cost                DECIMAL(18,4)    NULL,
    Currency            CHAR(3)          NULL,
    Location            NVARCHAR(100)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_BCp_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_BCp_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_BookCopies PRIMARY KEY CLUSTERED (TenantId, CopyId),
    CONSTRAINT UX_BCp UNIQUE (TenantId, AccessionNumber),
    CONSTRAINT FK_BCp_B FOREIGN KEY (TenantId, BookId) REFERENCES library_.Books(TenantId, BookId)
);

CREATE TABLE library_.BookIssues (
    IssueId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BI_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    CopyId              UNIQUEIDENTIFIER NOT NULL,
    BorrowerType        TINYINT          NOT NULL,         -- 0=Student,1=Employee
    BorrowerId          UNIQUEIDENTIFIER NOT NULL,
    IssuedOn            DATE             NOT NULL,
    DueOn               DATE             NOT NULL,
    IssuedByEmployeeId  UNIQUEIDENTIFIER NULL,
    Status              TINYINT          NOT NULL,         -- 0=Issued,1=Returned,2=Overdue,3=Lost
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_BI_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_BI_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_BookIssues PRIMARY KEY CLUSTERED (TenantId, IssueId),
    CONSTRAINT FK_BI_BCp FOREIGN KEY (TenantId, CopyId) REFERENCES library_.BookCopies(TenantId, CopyId),
    CONSTRAINT CK_BI_Due CHECK (DueOn >= IssuedOn)
);

CREATE TABLE library_.BookReturns (
    ReturnId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BR_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    IssueId             UNIQUEIDENTIFIER NOT NULL,
    ReturnedOn          DATE             NOT NULL,
    Condition_          TINYINT          NOT NULL,         -- 0=Good,1=Damaged,2=Lost
    ReceivedByEmployeeId UNIQUEIDENTIFIER NULL,
    FineAmount          DECIMAL(18,4)    NULL,
    Notes               NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_BR_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_BR_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_BookReturns PRIMARY KEY CLUSTERED (TenantId, ReturnId),
    CONSTRAINT UX_BR UNIQUE (TenantId, IssueId),
    CONSTRAINT FK_BR_BI FOREIGN KEY (TenantId, IssueId) REFERENCES library_.BookIssues(TenantId, IssueId)
);

CREATE TABLE library_.BookFines (
    FineId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BF_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    IssueId             UNIQUEIDENTIFIER NOT NULL,
    Amount              DECIMAL(18,4)    NOT NULL,
    Currency            CHAR(3)          NOT NULL,
    Reason              NVARCHAR(300)    NULL,
    Status              TINYINT          NOT NULL,         -- 0=Open,1=Paid,2=Waived
    PaidOn              DATE             NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_BF_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_BF_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_BookFines PRIMARY KEY CLUSTERED (TenantId, FineId),
    CONSTRAINT FK_BF_BI FOREIGN KEY (TenantId, IssueId) REFERENCES library_.BookIssues(TenantId, IssueId)
);
```

## 8.13 Inventory Management

```sql
CREATE TABLE inventory.InventoryCategories (
    CategoryId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IC_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(150)    NOT NULL,
    ParentCategoryId    UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_IC_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_IC_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_InvCategories PRIMARY KEY CLUSTERED (TenantId, CategoryId),
    CONSTRAINT UX_IC UNIQUE (TenantId, Code)
);

CREATE TABLE inventory.InventoryItems (
    ItemId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_II_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Sku                 VARCHAR(64)      NOT NULL,
    Name                NVARCHAR(200)    NOT NULL,
    CategoryId          UNIQUEIDENTIFIER NULL,
    Unit                VARCHAR(16)      NOT NULL,         -- 'pcs','kg'
    ReorderLevel        DECIMAL(12,2)    NULL,
    UnitCost            DECIMAL(18,4)    NULL,
    Currency            CHAR(3)          NULL,
    StockOnHand         DECIMAL(18,3)    NOT NULL CONSTRAINT DF_II_Stock DEFAULT (0),
    IsAsset             BIT              NOT NULL CONSTRAINT DF_II_Ast DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_II_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_II_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_InventoryItems PRIMARY KEY CLUSTERED (TenantId, ItemId),
    CONSTRAINT UX_II_Sku UNIQUE (TenantId, Sku)
);

CREATE TABLE inventory.Vendors (
    VendorId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_V2_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Code                VARCHAR(40)      NOT NULL,
    Name                NVARCHAR(200)    NOT NULL,
    ContactPerson       NVARCHAR(150)    NULL,
    Email               NVARCHAR(256)    NULL,
    PhoneNumber         VARCHAR(32)      NULL,
    AddressLine1        NVARCHAR(200)    NULL,
    City                NVARCHAR(100)    NULL,
    CountryCode         CHAR(2)          NULL,
    TaxId               VARCHAR(50)      NULL,
    BankAccountInfo     NVARCHAR(MAX)    NULL,             -- json
    Status              TINYINT          NOT NULL,         -- 0=Active,1=Blacklisted
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_V2_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_V2_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_Vendors PRIMARY KEY CLUSTERED (TenantId, VendorId),
    CONSTRAINT UX_V2_Code UNIQUE (TenantId, Code),
    CONSTRAINT UX_V2_Tax UNIQUE (TenantId, TaxId)
);

CREATE TABLE inventory.PurchaseOrders (
    PurchaseOrderId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PO_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Number              VARCHAR(40)      NOT NULL,
    VendorId            UNIQUEIDENTIFIER NOT NULL,
    OrderDate           DATE             NOT NULL,
    ExpectedDate        DATE             NULL,
    Status              TINYINT          NOT NULL,         -- 0=Draft,1=Approved,2=Sent,3=PartReceived,4=Received,5=Cancelled
    Subtotal            DECIMAL(18,4)    NOT NULL,
    TaxTotal            DECIMAL(18,4)    NOT NULL CONSTRAINT DF_PO_Tax DEFAULT (0),
    DiscountTotal       DECIMAL(18,4)    NOT NULL CONSTRAINT DF_PO_Dis DEFAULT (0),
    Total               AS (Subtotal + TaxTotal - DiscountTotal) PERSISTED,
    Currency            CHAR(3)          NOT NULL,
    Notes               NVARCHAR(MAX)    NULL,
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_PO_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_PO_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_PurchaseOrders PRIMARY KEY CLUSTERED (TenantId, PurchaseOrderId),
    CONSTRAINT UX_PO_Number UNIQUE (TenantId, Number),
    CONSTRAINT FK_PO_V FOREIGN KEY (TenantId, VendorId) REFERENCES inventory.Vendors(TenantId, VendorId)
);

CREATE TABLE inventory.PurchaseOrderItems (
    PoItemId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_POI_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    PurchaseOrderId     UNIQUEIDENTIFIER NOT NULL,
    ItemId              UNIQUEIDENTIFIER NOT NULL,
    Quantity            DECIMAL(18,3)    NOT NULL,
    UnitPrice           DECIMAL(18,4)    NOT NULL,
    DiscountAmount      DECIMAL(18,4)    NOT NULL CONSTRAINT DF_POI_Dis DEFAULT (0),
    TaxRate             DECIMAL(9,4)     NOT NULL CONSTRAINT DF_POI_TR DEFAULT (0),
    LineTotal           AS ((Quantity * UnitPrice - DiscountAmount) * (1 + TaxRate / 100.0)) PERSISTED,
    QuantityReceived    DECIMAL(18,3)    NOT NULL CONSTRAINT DF_POI_QR DEFAULT (0),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_POI_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedAt           DATETIME2(3)     NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT              NOT NULL CONSTRAINT DF_POI_Del DEFAULT (0),
    DeletedAt           DATETIME2(3)     NULL,
    DeletedBy           UNIQUEIDENTIFIER NULL,
    RowVersion          ROWVERSION       NOT NULL,
    CONSTRAINT PK_PurchaseOrderItems PRIMARY KEY CLUSTERED (TenantId, PurchaseOrderId, PoItemId),
    CONSTRAINT FK_POI_PO FOREIGN KEY (TenantId, PurchaseOrderId) REFERENCES inventory.PurchaseOrders(TenantId, PurchaseOrderId),
    CONSTRAINT FK_POI_II FOREIGN KEY (TenantId, ItemId) REFERENCES inventory.InventoryItems(TenantId, ItemId)
);

CREATE TABLE inventory.StockTransactions (
    TransactionId       BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ItemId              UNIQUEIDENTIFIER NOT NULL,
    TransactionType     TINYINT          NOT NULL,         -- 0=Receive,1=Issue,2=Adjustment,3=Return,4=Transfer
    Quantity            DECIMAL(18,3)    NOT NULL,
    ReferenceType       VARCHAR(40)      NULL,             -- 'PO','Issue','Manual'
    ReferenceId         UNIQUEIDENTIFIER NULL,
    BalanceAfter        DECIMAL(18,3)    NOT NULL,
    Notes               NVARCHAR(500)    NULL,
    OccurredAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ST_OA DEFAULT (SYSUTCDATETIME()),
    CreatedAt           DATETIME2(3)     NOT NULL CONSTRAINT DF_ST_CA DEFAULT (SYSUTCDATETIME()),
    CreatedBy           UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_StockTransactions PRIMARY KEY CLUSTERED (TenantId, ItemId, OccurredAtUtc, TransactionId),
    CONSTRAINT UX_ST_Identity UNIQUE (TransactionId)
);
```

## 8.14 Audit, Logging, Outbox, Inbox

```sql
CREATE TABLE audit_.OutboxMessages (
    OutboxMessageId     BIGINT IDENTITY(1,1) NOT NULL,
    MessageId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_OB_Id DEFAULT (NEWID()),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Module              VARCHAR(64)      NOT NULL,
    Exchange            VARCHAR(128)     NOT NULL,
    RoutingKey          VARCHAR(256)     NOT NULL,
    EventType           VARCHAR(256)     NOT NULL,
    ContentType         VARCHAR(64)      NOT NULL CONSTRAINT DF_OB_CT DEFAULT ('application/json'),
    SchemaVersion       VARCHAR(16)      NOT NULL CONSTRAINT DF_OB_SV DEFAULT ('v1'),
    Payload             NVARCHAR(MAX)    NOT NULL,
    Headers             NVARCHAR(MAX)    NULL,             -- json
    CorrelationId       UNIQUEIDENTIFIER NULL,
    CausationId         UNIQUEIDENTIFIER NULL,
    OccurredOnUtc       DATETIME2(3)     NOT NULL,
    AvailableAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_OB_Av DEFAULT (SYSUTCDATETIME()),
    ProcessedAtUtc      DATETIME2(3)     NULL,
    Attempts            INT              NOT NULL CONSTRAINT DF_OB_At DEFAULT (0),
    LastError           NVARCHAR(MAX)    NULL,
    Status              TINYINT          NOT NULL CONSTRAINT DF_OB_St DEFAULT (0),
        -- 0=Pending,1=Processing,2=Published,3=Failed,4=DeadLettered
    CONSTRAINT PK_OutboxMessages PRIMARY KEY CLUSTERED (OutboxMessageId),
    CONSTRAINT UX_OB_MessageId UNIQUE (MessageId)
);

CREATE TABLE audit_.InboxMessages (
    InboxMessageId      BIGINT IDENTITY(1,1) NOT NULL,
    MessageId           UNIQUEIDENTIFIER NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Consumer            VARCHAR(256)     NOT NULL,         -- module + handler full type name
    Module              VARCHAR(64)      NOT NULL,
    EventType           VARCHAR(256)     NOT NULL,
    SchemaVersion       VARCHAR(16)      NOT NULL,
    Payload             NVARCHAR(MAX)    NOT NULL,
    Headers             NVARCHAR(MAX)    NULL,
    CorrelationId       UNIQUEIDENTIFIER NULL,
    CausationId         UNIQUEIDENTIFIER NULL,
    ReceivedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_IB_RA DEFAULT (SYSUTCDATETIME()),
    ProcessedAtUtc      DATETIME2(3)     NULL,
    Attempts            INT              NOT NULL CONSTRAINT DF_IB_At DEFAULT (0),
    LastError           NVARCHAR(MAX)    NULL,
    Status              TINYINT          NOT NULL CONSTRAINT DF_IB_St DEFAULT (0),
        -- 0=Received,1=Processing,2=Processed,3=Failed,4=DeadLettered
    CONSTRAINT PK_InboxMessages PRIMARY KEY CLUSTERED (InboxMessageId),
    CONSTRAINT UX_IB_Idem UNIQUE (Consumer, MessageId)
);

CREATE TABLE audit_.AuditLogs (
    AuditLogId          BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    Module              VARCHAR(64)      NOT NULL,
    [Action]            VARCHAR(64)      NOT NULL,         -- Create/Update/Delete/Login/etc.
    EntityType          VARCHAR(128)     NOT NULL,
    EntityId            VARCHAR(64)      NOT NULL,
    OldValues           NVARCHAR(MAX)    NULL,
    NewValues           NVARCHAR(MAX)    NULL,
    ChangeSet           NVARCHAR(MAX)    NULL,
    IpAddress           VARCHAR(45)      NULL,
    UserAgent           NVARCHAR(400)    NULL,
    CorrelationId       UNIQUEIDENTIFIER NULL,
    OccurredAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_AL_OA DEFAULT (SYSUTCDATETIME()),
    AuditDate AS (CAST(OccurredAtUtc AS DATE)) PERSISTED,
    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (AuditDate, TenantId, AuditLogId)
        ON PS_Audit_ByMonth(AuditDate)
);

CREATE TABLE audit_.ActivityLogs (
    ActivityLogId       BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    [Action]            VARCHAR(64)      NOT NULL,
    Resource            VARCHAR(128)     NULL,
    Description         NVARCHAR(1000)   NULL,
    IpAddress           VARCHAR(45)      NULL,
    UserAgent           NVARCHAR(400)    NULL,
    CorrelationId       UNIQUEIDENTIFIER NULL,
    OccurredAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ACL_OA DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ActivityLogs PRIMARY KEY CLUSTERED (TenantId, OccurredAtUtc, ActivityLogId)
);

CREATE TABLE audit_.ErrorLogs (
    ErrorLogId          BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NULL,             -- can be null for system errors
    Source              VARCHAR(128)     NOT NULL,
    Severity            TINYINT          NOT NULL,         -- 0=Trace..5=Critical
    Message             NVARCHAR(2000)   NOT NULL,
    Exception           NVARCHAR(MAX)    NULL,
    StackTrace          NVARCHAR(MAX)    NULL,
    CorrelationId       UNIQUEIDENTIFIER NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    OccurredAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_EL_OA DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ErrorLogs PRIMARY KEY CLUSTERED (OccurredAtUtc, ErrorLogId)
);

CREATE TABLE audit_.BackgroundJobs (
    BackgroundJobId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BJ_Id DEFAULT (NEWSEQUENTIALID()),
    TenantId            UNIQUEIDENTIFIER NULL,
    JobType             VARCHAR(128)     NOT NULL,
    JobName             VARCHAR(256)     NOT NULL,
    Status              TINYINT          NOT NULL,         -- 0=Queued,1=Running,2=Succeeded,3=Failed
    QueuedAtUtc         DATETIME2(3)     NOT NULL CONSTRAINT DF_BJ_QA DEFAULT (SYSUTCDATETIME()),
    StartedAtUtc        DATETIME2(3)     NULL,
    CompletedAtUtc      DATETIME2(3)     NULL,
    DurationMs          BIGINT           NULL,
    Attempts            INT              NOT NULL CONSTRAINT DF_BJ_Att DEFAULT (0),
    LastError           NVARCHAR(MAX)    NULL,
    Parameters          NVARCHAR(MAX)    NULL,             -- json
    Result              NVARCHAR(MAX)    NULL,             -- json
    CONSTRAINT PK_BackgroundJobs PRIMARY KEY CLUSTERED (BackgroundJobId)
);
```


