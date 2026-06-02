-- School ERP core tables (SQL Server / Azure SQL)
-- Run after 001-schemas.sql
-- Recommended run flow: foundation -> academics -> admissions/students/teachers/parents -> modules

-- 0) Foundation
CREATE TABLE TenantManagement.Tenants (
    TenantId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Tenants PRIMARY KEY DEFAULT NEWID(),
    SchoolName          NVARCHAR(200) NOT NULL,
    SchoolCode          NVARCHAR(50)  NOT NULL,
    Slug                NVARCHAR(120) NOT NULL,
    SchoolEmail         NVARCHAR(150) NOT NULL,
    ContactNumber       NVARCHAR(20),
    PrincipalName       NVARCHAR(150),
    Website             NVARCHAR(200),
    Address             NVARCHAR(300),
    City                NVARCHAR(100),
    State               NVARCHAR(100),
    Country             NVARCHAR(100) CONSTRAINT DF_Tenants_Country DEFAULT N'India',
    StudentStrength     INT,
    Status              NVARCHAR(20) CONSTRAINT DF_Tenants_Status DEFAULT N'active',
    CreatedAt           DATETIME2 CONSTRAINT DF_Tenants_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Tenants_SchoolCode UNIQUE (SchoolCode),
    CONSTRAINT UQ_Tenants_Slug UNIQUE (Slug)
);

CREATE TABLE Billing.SubscriptionPlans (
    PlanId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SubscriptionPlans PRIMARY KEY,
    PlanKey             NVARCHAR(50) NOT NULL,
    PlanName            NVARCHAR(100) NOT NULL,
    MonthlyPrice        DECIMAL(12,2),
    QuarterlyPrice      DECIMAL(12,2),
    YearlyPrice         DECIMAL(12,2),
    IsCustom            BIT CONSTRAINT DF_SubscriptionPlans_IsCustom DEFAULT 0,
    Features            NVARCHAR(MAX),
    CONSTRAINT UQ_SubscriptionPlans_PlanKey UNIQUE (PlanKey)
);

CREATE TABLE Billing.Subscriptions (
    SubscriptionId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Subscriptions PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    PlanId              INT NOT NULL,
    BillingCycle        NVARCHAR(20),
    StartDate           DATE NOT NULL,
    RenewalDate         DATE,
    Amount              DECIMAL(12,2),
    GstAmount           DECIMAL(12,2),
    TotalAmount         DECIMAL(12,2),
    Status              NVARCHAR(20) CONSTRAINT DF_Subscriptions_Status DEFAULT N'active',
    CONSTRAINT FK_Subscriptions_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_Subscriptions_Plan FOREIGN KEY (PlanId) REFERENCES Billing.SubscriptionPlans(PlanId),
    CONSTRAINT CK_Subscriptions_BillingCycle CHECK (BillingCycle IN (N'monthly',N'quarterly',N'yearly'))
);

CREATE TABLE Billing.SubscriptionInvoices (
    InvoiceId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SubscriptionInvoices PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId      UNIQUEIDENTIFIER NOT NULL,
    InvoiceNo           NVARCHAR(50) NOT NULL,
    Amount              DECIMAL(12,2),
    GstAmount           DECIMAL(12,2),
    TotalAmount         DECIMAL(12,2),
    PaymentMethod       NVARCHAR(30),
    PaymentStatus       NVARCHAR(20) CONSTRAINT DF_SubscriptionInvoices_PaymentStatus DEFAULT N'pending',
    PaymentRef          NVARCHAR(100),
    InvoiceDate         DATE,
    DueDate             DATE,
    CONSTRAINT FK_SubscriptionInvoices_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_SubscriptionInvoices_Subscription FOREIGN KEY (SubscriptionId) REFERENCES Billing.Subscriptions(SubscriptionId),
    CONSTRAINT UQ_SubscriptionInvoices_InvoiceNo UNIQUE (InvoiceNo)
);

CREATE TABLE [Identity].Users (
    UserId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    FullName            NVARCHAR(150) NOT NULL,
    Email               NVARCHAR(150) NOT NULL,
    PasswordHash        NVARCHAR(300) NOT NULL,
    Role                NVARCHAR(30) NOT NULL,
    Status              NVARCHAR(20) CONSTRAINT DF_Users_Status DEFAULT N'active',
    LastLoginAt         DATETIME2,
    CreatedAt           DATETIME2 CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT UQ_Users_Tenant_Email UNIQUE (TenantId, Email)
);

-- 5) Academics (base entities for many modules)
CREATE TABLE Academics.Classes (
    ClassId             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Classes PRIMARY KEY,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(50) NOT NULL,
    ClassTeacherId      UNIQUEIDENTIFIER NULL,
    TotalStudents       INT CONSTRAINT DF_Classes_TotalStudents DEFAULT 0,
    CONSTRAINT FK_Classes_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Academics.Sections (
    SectionId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sections PRIMARY KEY,
    ClassId             INT NOT NULL,
    Name                NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Sections_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId)
);

CREATE TABLE Employees.Teachers (
    TeacherId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Teachers PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          NVARCHAR(20) NOT NULL,
    FirstName           NVARCHAR(80),
    LastName            NVARCHAR(80),
    Department          NVARCHAR(100),
    Subject             NVARCHAR(100),
    Qualification       NVARCHAR(150),
    Experience          INT,
    Email               NVARCHAR(150),
    Phone               NVARCHAR(20),
    Salary              DECIMAL(12,2),
    JoiningDate         DATE,
    Status              NVARCHAR(20) CONSTRAINT DF_Teachers_Status DEFAULT N'active',
    AvatarUrl           NVARCHAR(500),
    CONSTRAINT FK_Teachers_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT UQ_Teachers_Tenant_Emp UNIQUE (TenantId, EmployeeId)
);

ALTER TABLE Academics.Classes
ADD CONSTRAINT FK_Classes_ClassTeacher FOREIGN KEY (ClassTeacherId) REFERENCES Employees.Teachers(TeacherId);

CREATE TABLE Academics.Subjects (
    SubjectId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Subjects PRIMARY KEY,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(100),
    Code                NVARCHAR(20),
    ClassId             INT,
    SectionId           INT,
    TeacherId           UNIQUEIDENTIFIER,
    WeeklyHours         INT,
    Status              NVARCHAR(20) CONSTRAINT DF_Subjects_Status DEFAULT N'active',
    CONSTRAINT FK_Subjects_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_Subjects_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId),
    CONSTRAINT FK_Subjects_Section FOREIGN KEY (SectionId) REFERENCES Academics.Sections(SectionId),
    CONSTRAINT FK_Subjects_Teacher FOREIGN KEY (TeacherId) REFERENCES Employees.Teachers(TeacherId)
);

CREATE TABLE Academics.TeacherClasses (
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeacherClasses PRIMARY KEY,
    TeacherId           UNIQUEIDENTIFIER NOT NULL,
    ClassId             INT NOT NULL,
    SectionId           INT,
    CONSTRAINT FK_TeacherClasses_Teacher FOREIGN KEY (TeacherId) REFERENCES Employees.Teachers(TeacherId),
    CONSTRAINT FK_TeacherClasses_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId),
    CONSTRAINT FK_TeacherClasses_Section FOREIGN KEY (SectionId) REFERENCES Academics.Sections(SectionId)
);

-- 1) Admission
CREATE TABLE Students.AdmissionApplications (
    AdmissionId         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AdmissionApplications PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AdmissionNo         NVARCHAR(50),
    FirstName           NVARCHAR(80),
    LastName            NVARCHAR(80),
    Gender              NVARCHAR(10),
    DateOfBirth         DATE,
    PlaceOfBirth        NVARCHAR(120),
    Religion            NVARCHAR(60),
    Category            NVARCHAR(20),
    AadhaarNumber       NVARCHAR(20),
    BloodGroup          NVARCHAR(10),
    ClassSought         NVARCHAR(20),
    AcademicSession     NVARCHAR(20),
    PreviousSchoolTransfer NVARCHAR(5),
    HouseNumber         NVARCHAR(50),
    Street              NVARCHAR(150),
    City                NVARCHAR(100),
    State               NVARCHAR(100),
    PinCode             NVARCHAR(10),
    Country             NVARCHAR(50) CONSTRAINT DF_AdmissionApplications_Country DEFAULT N'India',
    PrimaryMobile       NVARCHAR(20),
    AlternateMobile     NVARCHAR(20),
    Email               NVARCHAR(150),
    FatherName          NVARCHAR(150),
    FatherQualification NVARCHAR(150),
    FatherOccupation    NVARCHAR(100),
    FatherOrganization  NVARCHAR(150),
    FatherOfficeAddress NVARCHAR(300),
    FatherOfficePhone   NVARCHAR(20),
    FatherMobile        NVARCHAR(20),
    FatherAnnualIncome  NVARCHAR(50),
    FatherAadhaar       NVARCHAR(20),
    FatherEmail         NVARCHAR(150),
    MotherName          NVARCHAR(150),
    MotherQualification NVARCHAR(150),
    MotherOccupation    NVARCHAR(100),
    MotherOrganization  NVARCHAR(150),
    MotherOfficeAddress NVARCHAR(300),
    MotherOfficePhone   NVARCHAR(20),
    MotherMobile        NVARCHAR(20),
    MotherAnnualIncome  NVARCHAR(50),
    MotherAadhaar       NVARCHAR(20),
    MotherEmail         NVARCHAR(150),
    LivesWithGuardian   NVARCHAR(5) CONSTRAINT DF_AdmissionApplications_LivesWithGuardian DEFAULT N'no',
    GuardianName        NVARCHAR(150),
    GuardianRelationship NVARCHAR(50),
    GuardianOccupation  NVARCHAR(100),
    GuardianMobile      NVARCHAR(20),
    GuardianEmail       NVARCHAR(150),
    GuardianAddress     NVARCHAR(300),
    PassingYear         NVARCHAR(10),
    PreviousSchoolName  NVARCHAR(200),
    PreviousSchoolArea  NVARCHAR(150),
    PreviousBoard       NVARCHAR(50),
    PreviousPercentage  NVARCHAR(20),
    ReasonForLeaving    NVARCHAR(500),
    SiblingInSameSchool NVARCHAR(5) CONSTRAINT DF_AdmissionApplications_SiblingInSameSchool DEFAULT N'no',
    ReferenceName       NVARCHAR(150),
    ReferenceMobile     NVARCHAR(20),
    ReferenceAddress    NVARCHAR(300),
    ReferenceRelationship NVARCHAR(50),
    DeclarationTruth    BIT,
    DeclarationPolicy   BIT,
    ParentSignature     NVARCHAR(150),
    StudentSignature    NVARCHAR(150),
    DeclarationDate     DATE,
    Status              NVARCHAR(30) CONSTRAINT DF_AdmissionApplications_Status DEFAULT N'submitted',
    SubmittedAt         DATETIME2 CONSTRAINT DF_AdmissionApplications_SubmittedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AdmissionApplications_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Students.AdmissionSiblings (
    SiblingId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdmissionSiblings PRIMARY KEY,
    AdmissionId         UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(150),
    AdmissionNumber     NVARCHAR(50),
    Class               NVARCHAR(20),
    Section             NVARCHAR(20),
    CONSTRAINT FK_AdmissionSiblings_Admission FOREIGN KEY (AdmissionId) REFERENCES Students.AdmissionApplications(AdmissionId)
);

CREATE TABLE Students.AdmissionDocuments (
    DocumentId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdmissionDocuments PRIMARY KEY,
    AdmissionId         UNIQUEIDENTIFIER NOT NULL,
    DocType             NVARCHAR(50),
    FileName            NVARCHAR(200),
    FileSize            INT,
    FileType            NVARCHAR(50),
    StoragePath         NVARCHAR(500),
    CONSTRAINT FK_AdmissionDocuments_Admission FOREIGN KEY (AdmissionId) REFERENCES Students.AdmissionApplications(AdmissionId)
);

-- 2) Students / 4) Parents
CREATE TABLE Students.Students (
    StudentId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Students PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    AdmissionId         UNIQUEIDENTIFIER NULL,
    AdmissionNo         NVARCHAR(50),
    RollNo              NVARCHAR(20),
    FirstName           NVARCHAR(80) NOT NULL,
    LastName            NVARCHAR(80) NOT NULL,
    Gender              NVARCHAR(10),
    DateOfBirth         DATE,
    BloodGroup          NVARCHAR(10),
    Email               NVARCHAR(150),
    Phone               NVARCHAR(20),
    Address             NVARCHAR(300),
    ClassId             INT NULL,
    SectionId           INT NULL,
    Status              NVARCHAR(20) CONSTRAINT DF_Students_Status DEFAULT N'active',
    AvatarUrl           NVARCHAR(500),
    CreatedAt           DATETIME2 CONSTRAINT DF_Students_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Students_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_Students_Admission FOREIGN KEY (AdmissionId) REFERENCES Students.AdmissionApplications(AdmissionId)
);

ALTER TABLE Students.Students
ADD CONSTRAINT FK_Students_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId);

ALTER TABLE Students.Students
ADD CONSTRAINT FK_Students_Section FOREIGN KEY (SectionId) REFERENCES Academics.Sections(SectionId);

CREATE INDEX IX_Students_Tenant ON Students.Students(TenantId);
CREATE INDEX IX_Students_Class ON Students.Students(ClassId, SectionId);

CREATE TABLE Students.Parents (
    ParentId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Parents PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    FirstName           NVARCHAR(80),
    LastName            NVARCHAR(80),
    Email               NVARCHAR(150),
    Phone               NVARCHAR(20),
    Occupation          NVARCHAR(100),
    Address             NVARCHAR(300),
    Status              NVARCHAR(20) CONSTRAINT DF_Parents_Status DEFAULT N'active',
    AvatarUrl           NVARCHAR(500),
    CONSTRAINT FK_Parents_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Students.ParentStudents (
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParentStudents PRIMARY KEY,
    ParentId            UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    Relationship        NVARCHAR(30),
    CONSTRAINT FK_ParentStudents_Parent FOREIGN KEY (ParentId) REFERENCES Students.Parents(ParentId),
    CONSTRAINT FK_ParentStudents_Student FOREIGN KEY (StudentId) REFERENCES Students.Students(StudentId)
);

-- 6) Attendance
CREATE TABLE Attendance.AttendanceRecords (
    AttendanceId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AttendanceRecords PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EntityType          NVARCHAR(10),
    EntityId            UNIQUEIDENTIFIER NOT NULL,
    ClassOrDept         NVARCHAR(100),
    [Date]              DATE NOT NULL,
    Status              NVARCHAR(15),
    CheckIn             TIME,
    CheckOut            TIME,
    Remarks             NVARCHAR(300),
    CreatedAt           DATETIME2 CONSTRAINT DF_AttendanceRecords_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AttendanceRecords_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT CK_AttendanceRecords_EntityType CHECK (EntityType IN (N'student',N'teacher')),
    CONSTRAINT CK_AttendanceRecords_Status CHECK (Status IN (N'present',N'absent',N'late',N'on-leave',N'holiday'))
);
CREATE INDEX IX_Attendance_Date ON Attendance.AttendanceRecords(TenantId, [Date]);

-- 7) Fees
CREATE TABLE Fees.FeeStructures (
    FeeStructureId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeStructures PRIMARY KEY,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(100),
    ClassId             INT,
    AcademicSession     NVARCHAR(20),
    AdmissionFee        DECIMAL(12,2),
    TuitionFeeMonthly   DECIMAL(12,2),
    TransportFeeMonthly DECIMAL(12,2),
    LibraryFeeAnnual    DECIMAL(12,2),
    LabFeeAnnual        DECIMAL(12,2),
    CONSTRAINT FK_FeeStructures_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_FeeStructures_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId)
);

CREATE TABLE Fees.FeeInvoices (
    InvoiceId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FeeInvoices PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    InvoiceNo           NVARCHAR(50) NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    ClassId             INT,
    TotalFee            DECIMAL(12,2),
    Discount            DECIMAL(12,2) CONSTRAINT DF_FeeInvoices_Discount DEFAULT 0,
    Fine                DECIMAL(12,2) CONSTRAINT DF_FeeInvoices_Fine DEFAULT 0,
    Paid                DECIMAL(12,2) CONSTRAINT DF_FeeInvoices_Paid DEFAULT 0,
    Pending             DECIMAL(12,2) CONSTRAINT DF_FeeInvoices_Pending DEFAULT 0,
    DueDate             DATE,
    PaidDate            DATE,
    Status              NVARCHAR(20),
    PaymentMethod       NVARCHAR(30),
    CONSTRAINT FK_FeeInvoices_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_FeeInvoices_Student FOREIGN KEY (StudentId) REFERENCES Students.Students(StudentId),
    CONSTRAINT FK_FeeInvoices_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId),
    CONSTRAINT CK_FeeInvoices_Status CHECK (Status IN (N'paid',N'pending',N'overdue',N'partial'))
);

CREATE TABLE Fees.FeeInvoiceItems (
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeInvoiceItems PRIMARY KEY,
    InvoiceId           UNIQUEIDENTIFIER NOT NULL,
    FeeType             NVARCHAR(50),
    Amount              DECIMAL(12,2),
    LineDiscount        DECIMAL(12,2) CONSTRAINT DF_FeeInvoiceItems_LineDiscount DEFAULT 0,
    CONSTRAINT FK_FeeInvoiceItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES Fees.FeeInvoices(InvoiceId)
);

CREATE TABLE Fees.FeePayments (
    PaymentId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FeePayments PRIMARY KEY DEFAULT NEWID(),
    InvoiceId           UNIQUEIDENTIFIER NOT NULL,
    Amount              DECIMAL(12,2),
    PaymentDate         DATETIME2 CONSTRAINT DF_FeePayments_PaymentDate DEFAULT SYSUTCDATETIME(),
    PaymentMethod       NVARCHAR(30),
    Reference           NVARCHAR(100),
    CONSTRAINT FK_FeePayments_Invoice FOREIGN KEY (InvoiceId) REFERENCES Fees.FeeInvoices(InvoiceId)
);

-- 8) Payroll / HR
CREATE TABLE Employees.Employees (
    EmployeeId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Employees PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeCode        NVARCHAR(20),
    FullName            NVARCHAR(150),
    Department          NVARCHAR(100),
    Designation         NVARCHAR(100),
    Email               NVARCHAR(150),
    Phone               NVARCHAR(20),
    JoiningDate         DATE,
    Salary              DECIMAL(12,2),
    Status              NVARCHAR(20) CONSTRAINT DF_Employees_Status DEFAULT N'active',
    CONSTRAINT FK_Employees_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Employees.PayrollRecords (
    PayrollId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PayrollRecords PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    Department          NVARCHAR(100),
    [Month]             NVARCHAR(20),
    [Year]              INT,
    BasicSalary         DECIMAL(12,2),
    HRA                 DECIMAL(12,2),
    DA                  DECIMAL(12,2),
    TA                  DECIMAL(12,2),
    Medical             DECIMAL(12,2),
    Special             DECIMAL(12,2),
    Bonus               DECIMAL(12,2) CONSTRAINT DF_PayrollRecords_Bonus DEFAULT 0,
    PFDeduction         DECIMAL(12,2),
    TaxDeduction        DECIMAL(12,2),
    Insurance           DECIMAL(12,2),
    LoanDeduction       DECIMAL(12,2),
    OtherDeduction      DECIMAL(12,2),
    GrossSalary         DECIMAL(12,2),
    TotalDeductions     DECIMAL(12,2),
    NetSalary           DECIMAL(12,2),
    Status              NVARCHAR(20) CONSTRAINT DF_PayrollRecords_Status DEFAULT N'pending',
    PaymentDate         DATE,
    CONSTRAINT FK_PayrollRecords_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Employees.LeaveRequests (
    LeaveId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_LeaveRequests PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    EmployeeId          UNIQUEIDENTIFIER NOT NULL,
    Department          NVARCHAR(100),
    LeaveType           NVARCHAR(20),
    StartDate           DATE,
    EndDate             DATE,
    Days                INT,
    Reason              NVARCHAR(500),
    Status              NVARCHAR(20) CONSTRAINT DF_LeaveRequests_Status DEFAULT N'pending',
    AppliedOn           DATE,
    ApprovedBy          NVARCHAR(150),
    ApprovedOn          DATE,
    CONSTRAINT FK_LeaveRequests_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

-- 9) Transport
CREATE TABLE Transport.Vehicles (
    VehicleId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Vehicles PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    VehicleNumber       NVARCHAR(30) NOT NULL,
    VehicleType         NVARCHAR(20),
    Capacity            INT,
    DriverName          NVARCHAR(150),
    DriverPhone         NVARCHAR(20),
    DriverLicense       NVARCHAR(50),
    InsuranceExpiry     DATE,
    FitnessExpiry       DATE,
    GpsStatus           NVARCHAR(20),
    LastLocation        NVARCHAR(200),
    Status              NVARCHAR(20) CONSTRAINT DF_Vehicles_Status DEFAULT N'active',
    CONSTRAINT FK_Vehicles_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Transport.Routes (
    RouteId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Routes PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    RouteName           NVARCHAR(150),
    VehicleId           UNIQUEIDENTIFIER,
    StartPoint          NVARCHAR(150),
    EndPoint            NVARCHAR(150),
    TotalStops          INT,
    Distance            NVARCHAR(20),
    MorningTime         NVARCHAR(20),
    EveningTime         NVARCHAR(20),
    Fare                DECIMAL(12,2),
    Status              NVARCHAR(20) CONSTRAINT DF_Routes_Status DEFAULT N'active',
    CONSTRAINT FK_Routes_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_Routes_Vehicle FOREIGN KEY (VehicleId) REFERENCES Transport.Vehicles(VehicleId)
);

CREATE TABLE Transport.RouteStops (
    StopId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RouteStops PRIMARY KEY,
    RouteId             UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(150),
    SequenceNo          INT,
    PickupTime          NVARCHAR(20),
    CONSTRAINT FK_RouteStops_Route FOREIGN KEY (RouteId) REFERENCES Transport.Routes(RouteId)
);

CREATE TABLE Transport.StudentTransport (
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StudentTransport PRIMARY KEY,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    RouteId             UNIQUEIDENTIFIER NOT NULL,
    StopId              INT,
    CONSTRAINT FK_StudentTransport_Student FOREIGN KEY (StudentId) REFERENCES Students.Students(StudentId),
    CONSTRAINT FK_StudentTransport_Route FOREIGN KEY (RouteId) REFERENCES Transport.Routes(RouteId),
    CONSTRAINT FK_StudentTransport_Stop FOREIGN KEY (StopId) REFERENCES Transport.RouteStops(StopId)
);

-- 10) Exams
CREATE TABLE Examinations.Exams (
    ExamId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Exams PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ExamName            NVARCHAR(150),
    ExamType            NVARCHAR(30),
    SubjectId           INT,
    ClassId             INT,
    SectionId           INT,
    [Date]              DATE,
    StartTime           TIME,
    DurationMinutes     INT,
    TotalMarks          INT,
    PassingMarks        INT,
    Room                NVARCHAR(50),
    Status              NVARCHAR(20) CONSTRAINT DF_Exams_Status DEFAULT N'scheduled',
    CONSTRAINT FK_Exams_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_Exams_Subject FOREIGN KEY (SubjectId) REFERENCES Academics.Subjects(SubjectId),
    CONSTRAINT FK_Exams_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId),
    CONSTRAINT FK_Exams_Section FOREIGN KEY (SectionId) REFERENCES Academics.Sections(SectionId)
);

CREATE TABLE Examinations.ExamResults (
    ResultId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ExamResults PRIMARY KEY DEFAULT NEWID(),
    ExamId              UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    MarksObtained       DECIMAL(6,2),
    Grade               NVARCHAR(5),
    Remarks             NVARCHAR(300),
    CONSTRAINT FK_ExamResults_Exam FOREIGN KEY (ExamId) REFERENCES Examinations.Exams(ExamId),
    CONSTRAINT FK_ExamResults_Student FOREIGN KEY (StudentId) REFERENCES Students.Students(StudentId)
);

-- 11) Library
CREATE TABLE Library.Books (
    BookId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Books PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Title               NVARCHAR(250) NOT NULL,
    Author              NVARCHAR(200),
    ISBN                NVARCHAR(20),
    Category            NVARCHAR(50),
    Publisher           NVARCHAR(150),
    PublishYear         INT,
    Quantity            INT,
    Available           INT,
    Issued              INT,
    Location            NVARCHAR(50),
    Description         NVARCHAR(500),
    CONSTRAINT FK_Books_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Library.BookIssues (
    IssueId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BookIssues PRIMARY KEY DEFAULT NEWID(),
    BookId              UNIQUEIDENTIFIER NOT NULL,
    MemberType          NVARCHAR(10),
    MemberId            UNIQUEIDENTIFIER NOT NULL,
    IssueDate           DATE,
    DueDate             DATE,
    ReturnDate          DATE,
    Status              NVARCHAR(20),
    Fine                DECIMAL(10,2) CONSTRAINT DF_BookIssues_Fine DEFAULT 0,
    CONSTRAINT FK_BookIssues_Book FOREIGN KEY (BookId) REFERENCES Library.Books(BookId),
    CONSTRAINT CK_BookIssues_MemberType CHECK (MemberType IN (N'student',N'teacher')),
    CONSTRAINT CK_BookIssues_Status CHECK (Status IN (N'issued',N'returned',N'overdue'))
);

-- 12) Hostel
CREATE TABLE Students.HostelBlocks (
    BlockId             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HostelBlocks PRIMARY KEY,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(100),
    WardenName          NVARCHAR(150),
    GenderType          NVARCHAR(10),
    CONSTRAINT FK_HostelBlocks_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Students.HostelRooms (
    RoomId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HostelRooms PRIMARY KEY,
    BlockId             INT NOT NULL,
    RoomNo              NVARCHAR(20),
    Floor               INT,
    Capacity            INT,
    Occupied            INT CONSTRAINT DF_HostelRooms_Occupied DEFAULT 0,
    MonthlyFee          DECIMAL(12,2),
    Status              NVARCHAR(20) CONSTRAINT DF_HostelRooms_Status DEFAULT N'available',
    CONSTRAINT FK_HostelRooms_Block FOREIGN KEY (BlockId) REFERENCES Students.HostelBlocks(BlockId)
);

CREATE TABLE Students.HostelAllocations (
    AllocationId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HostelAllocations PRIMARY KEY DEFAULT NEWID(),
    RoomId              INT NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    StartDate           DATE,
    EndDate             DATE,
    Status              NVARCHAR(20) CONSTRAINT DF_HostelAllocations_Status DEFAULT N'active',
    CONSTRAINT FK_HostelAllocations_Room FOREIGN KEY (RoomId) REFERENCES Students.HostelRooms(RoomId),
    CONSTRAINT FK_HostelAllocations_Student FOREIGN KEY (StudentId) REFERENCES Students.Students(StudentId)
);

-- 13) Timetable
CREATE TABLE Academics.TimetableSlots (
    SlotId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TimetableSlots PRIMARY KEY,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    ClassId             INT NOT NULL,
    SectionId           INT,
    DayOfWeek           NVARCHAR(10),
    StartTime           TIME,
    EndTime             TIME,
    SubjectId           INT,
    TeacherId           UNIQUEIDENTIFIER,
    Room                NVARCHAR(50),
    CONSTRAINT FK_TimetableSlots_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId),
    CONSTRAINT FK_TimetableSlots_Class FOREIGN KEY (ClassId) REFERENCES Academics.Classes(ClassId),
    CONSTRAINT FK_TimetableSlots_Section FOREIGN KEY (SectionId) REFERENCES Academics.Sections(SectionId),
    CONSTRAINT FK_TimetableSlots_Subject FOREIGN KEY (SubjectId) REFERENCES Academics.Subjects(SubjectId),
    CONSTRAINT FK_TimetableSlots_Teacher FOREIGN KEY (TeacherId) REFERENCES Employees.Teachers(TeacherId)
);

-- 14) Inventory
CREATE TABLE Inventory.InventoryItems (
    ItemId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InventoryItems PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Name                NVARCHAR(150),
    Category            NVARCHAR(50),
    SKU                 NVARCHAR(50),
    Quantity            INT,
    MinStock            INT,
    Unit                NVARCHAR(20),
    Location            NVARCHAR(100),
    Status              NVARCHAR(20),
    LastRestocked       DATE,
    CONSTRAINT FK_InventoryItems_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Inventory.InventoryTransactions (
    TxnId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InventoryTransactions PRIMARY KEY DEFAULT NEWID(),
    ItemId              UNIQUEIDENTIFIER NOT NULL,
    TxnType             NVARCHAR(20),
    Quantity            INT,
    TxnDate             DATETIME2 CONSTRAINT DF_InventoryTransactions_TxnDate DEFAULT SYSUTCDATETIME(),
    Note                NVARCHAR(300),
    CONSTRAINT FK_InventoryTransactions_Item FOREIGN KEY (ItemId) REFERENCES Inventory.InventoryItems(ItemId)
);

-- 16) Notifications
CREATE TABLE Communication.Notifications (
    NotificationId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY DEFAULT NEWID(),
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Title               NVARCHAR(200),
    Message             NVARCHAR(MAX),
    Type                NVARCHAR(20),
    Channel             NVARCHAR(20),
    TargetAudience      NVARCHAR(20),
    SentAt              DATETIME2 CONSTRAINT DF_Notifications_SentAt DEFAULT SYSUTCDATETIME(),
    TotalRecipients     INT,
    ReadCount           INT,
    CONSTRAINT FK_Notifications_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

CREATE TABLE Communication.NotificationRecipients (
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationRecipients PRIMARY KEY,
    NotificationId      UNIQUEIDENTIFIER NOT NULL,
    UserId              UNIQUEIDENTIFIER NULL,
    DeliveryStatus      NVARCHAR(20),
    ReadAt              DATETIME2,
    CONSTRAINT FK_NotificationRecipients_Notification FOREIGN KEY (NotificationId) REFERENCES Communication.Notifications(NotificationId),
    CONSTRAINT FK_NotificationRecipients_User FOREIGN KEY (UserId) REFERENCES [Identity].Users(UserId)
);

-- 17) Settings
CREATE TABLE TenantManagement.SchoolSettings (
    TenantId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SchoolSettings PRIMARY KEY,
    SchoolName          NVARCHAR(200),
    Email               NVARCHAR(150),
    Phone               NVARCHAR(20),
    Address             NVARCHAR(300),
    City                NVARCHAR(100),
    State               NVARCHAR(100),
    Pincode             NVARCHAR(10),
    Website             NVARCHAR(200),
    PrincipalName       NVARCHAR(150),
    EstablishedYear     INT,
    AffiliationNumber   NVARCHAR(100),
    AffiliationBoard    NVARCHAR(50),
    LogoUrl             NVARCHAR(500),
    AcademicYear        NVARCHAR(20),
    CONSTRAINT FK_SchoolSettings_Tenant FOREIGN KEY (TenantId) REFERENCES TenantManagement.Tenants(TenantId)
);

-- Helpful tenant-focused indexes (baseline)
CREATE INDEX IX_Subscriptions_Tenant ON Billing.Subscriptions(TenantId);
CREATE INDEX IX_SubscriptionInvoices_Tenant ON Billing.SubscriptionInvoices(TenantId);
CREATE INDEX IX_Users_Tenant ON [Identity].Users(TenantId);
CREATE INDEX IX_Classes_Tenant ON Academics.Classes(TenantId);
CREATE INDEX IX_Subjects_Tenant ON Academics.Subjects(TenantId);
CREATE INDEX IX_FeeInvoices_Tenant ON Fees.FeeInvoices(TenantId);
CREATE INDEX IX_PayrollRecords_Tenant ON Employees.PayrollRecords(TenantId);
CREATE INDEX IX_Notifications_Tenant ON Communication.Notifications(TenantId);
