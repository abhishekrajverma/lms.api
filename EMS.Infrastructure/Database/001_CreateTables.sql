-- ============================================================================
-- EMS Database Schema - SQL Server
-- Creates all tables for the Educational Management System
-- ============================================================================

USE [EMS_DB]
GO

-- ============================================================================
-- 1. USERS TABLE (Authentication & Authorization)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Username] NVARCHAR(50) NOT NULL UNIQUE,
        [Email] NVARCHAR(256) NOT NULL UNIQUE,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [ProfilePhotoUrl] NVARCHAR(MAX) NULL,
        [Role] NVARCHAR(50) NOT NULL DEFAULT 'Student', -- Admin, Faculty, Student, Parent, DepartmentHead, Finance
        [AccountStatus] NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Locked, Disabled, PendingVerification, Suspended
        [IsEmailVerified] BIT NOT NULL DEFAULT 0,
        [EmailVerificationSentAt] DATETIME2 NULL,
        [LastLoginAt] DATETIME2 NULL,
        [LastLoginIpAddress] NVARCHAR(50) NULL,
        [FailedLoginAttempts] INT NOT NULL DEFAULT 0,
        [LockedUntil] DATETIME2 NULL,
        [PasswordChangedAt] DATETIME2 NULL,
        [IsMfaEnabled] BIT NOT NULL DEFAULT 0,
        [MfaSecret] NVARCHAR(MAX) NULL,
        [MfaBackupCodes] NVARCHAR(MAX) NULL,
        [DepartmentId] INT NULL,
        [EmployeeId] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL
    )
END
GO

-- ============================================================================
-- 2. DEPARTMENTS TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departments')
BEGIN
    CREATE TABLE [dbo].[Departments]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(200) NOT NULL,
        [Code] NVARCHAR(20) NOT NULL UNIQUE,
        [Description] NVARCHAR(MAX) NULL,
        [HeadUserId] INT NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [Email] NVARCHAR(256) NULL,
        [Location] NVARCHAR(200) NULL,
        [Address] NVARCHAR(500) NULL,
        [AnnualBudget] DECIMAL(18, 2) NULL,
        [FacultyCount] INT NOT NULL DEFAULT 0,
        [CourseCount] INT NOT NULL DEFAULT 0,
        [WebsiteUrl] NVARCHAR(MAX) NULL,
        [EstablishedYear] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([HeadUserId]) REFERENCES [dbo].[Users]([Id])
    )
END
GO

-- ============================================================================
-- 3. STUDENTS TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Students')
BEGIN
    CREATE TABLE [dbo].[Students]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [EnrollmentNumber] NVARCHAR(20) NOT NULL UNIQUE,
        [RollNumber] NVARCHAR(20) NULL,
        [UserId] INT NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [DateOfBirth] DATETIME NOT NULL,
        [Gender] NVARCHAR(20) NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [PersonalEmail] NVARCHAR(256) NULL,
        [Address] NVARCHAR(500) NULL,
        [City] NVARCHAR(100) NULL,
        [State] NVARCHAR(100) NULL,
        [Country] NVARCHAR(100) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [DepartmentId] INT NOT NULL,
        [CurrentSemester] INT NOT NULL DEFAULT 1,
        [CurrentAcademicYear] NVARCHAR(20) NOT NULL,
        [AdmissionYear] INT NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Inactive, OnLeave, Graduated, Suspended
        [CurrentGPA] DECIMAL(3, 2) NOT NULL DEFAULT 0.00,
        [TotalCreditHoursCompleted] INT NOT NULL DEFAULT 0,
        [ParentFirstName] NVARCHAR(100) NULL,
        [ParentLastName] NVARCHAR(100) NULL,
        [ParentPhoneNumber] NVARCHAR(20) NULL,
        [ParentEmail] NVARCHAR(256) NULL,
        [ParentRelationship] NVARCHAR(50) NULL,
        [EmergencyContactName] NVARCHAR(100) NULL,
        [EmergencyContactPhone] NVARCHAR(20) NULL,
        [PhotoUrl] NVARCHAR(MAX) NULL,
        [IsOnAcademicProbation] BIT NOT NULL DEFAULT 0,
        [ProbationStartDate] DATETIME2 NULL,
        [LastSemesterCompletedDate] DATETIME2 NULL,
        [ExpectedGraduationYear] INT NULL,
        [GraduationDate] DATETIME2 NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
        FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id])
    )
END
GO

-- ============================================================================
-- 4. COURSES TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Courses')
BEGIN
    CREATE TABLE [dbo].[Courses]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CourseCode] NVARCHAR(20) NOT NULL UNIQUE,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [CreditHours] INT NOT NULL DEFAULT 3,
        [DepartmentId] INT NOT NULL,
        [InstructorUserId] INT NULL,
        [Capacity] INT NOT NULL DEFAULT 50,
        [EnrolledStudentCount] INT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Closed, Completed, Suspended
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        [AcademicYear] NVARCHAR(20) NOT NULL,
        [SemesterType] NVARCHAR(50) NOT NULL DEFAULT 'Fall', -- Fall, Spring, Summer
        [CourseLevel] INT NOT NULL DEFAULT 1,
        [IsMandatory] BIT NOT NULL DEFAULT 1,
        [Prerequisites] NVARCHAR(MAX) NULL,
        [ClassRoom] NVARCHAR(100) NULL,
        [ScheduleTime] NVARCHAR(100) NULL,
        [GradingPolicy] NVARCHAR(MAX) NULL,
        [SyllabusUrl] NVARCHAR(MAX) NULL,
        [Materials] NVARCHAR(MAX) NULL,
        [DeliveryMode] NVARCHAR(50) NULL DEFAULT 'InPerson', -- Online, InPerson, Hybrid
        [OnlineMeetingLink] NVARCHAR(MAX) NULL,
        [MaxGradePoints] INT NOT NULL DEFAULT 100,
        [PassingGradePercentage] DECIMAL(5, 2) NOT NULL DEFAULT 40.00,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id]),
        FOREIGN KEY ([InstructorUserId]) REFERENCES [dbo].[Users]([Id])
    )
END
GO

-- ============================================================================
-- 5. ENROLLMENTS TABLE (Student-Course junction)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Enrollments')
BEGIN
    CREATE TABLE [dbo].[Enrollments]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [StudentId] INT NOT NULL,
        [CourseId] INT NOT NULL,
        [EnrollmentDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [WithdrawalDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [Semester] NVARCHAR(50) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]),
        FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]),
        UNIQUE ([StudentId], [CourseId])
    )
END
GO

-- ============================================================================
-- 6. GRADES TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Grades')
BEGIN
    CREATE TABLE [dbo].[Grades]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [StudentId] INT NOT NULL,
        [CourseId] INT NOT NULL,
        [Semester] NVARCHAR(50) NOT NULL,
        [InternalAssessmentScore] DECIMAL(5, 2) NULL,
        [FinalExamScore] DECIMAL(5, 2) NULL,
        [MidtermScore] DECIMAL(5, 2) NULL,
        [ProjectScore] DECIMAL(5, 2) NULL,
        [PracticalScore] DECIMAL(5, 2) NULL,
        [FinalPercentageScore] DECIMAL(5, 2) NOT NULL DEFAULT 0.00,
        [LetterGrade] NVARCHAR(2) NOT NULL DEFAULT 'F', -- A, B, C, D, F, NotGraded, Incomplete, Withdrawn
        [GpaPoints] DECIMAL(3, 2) NOT NULL DEFAULT 0.00,
        [InstructorComments] NVARCHAR(MAX) NULL,
        [IsSubmitted] BIT NOT NULL DEFAULT 0,
        [SubmittedDate] DATETIME2 NULL,
        [SubmittedByUserId] INT NULL,
        [IsApproved] BIT NOT NULL DEFAULT 0,
        [ApprovedDate] DATETIME2 NULL,
        [ApprovedByUserId] INT NULL,
        [IsDisputed] BIT NOT NULL DEFAULT 0,
        [DisputeReason] NVARCHAR(MAX) NULL,
        [DisputedDate] DATETIME2 NULL,
        [DisputeResult] NVARCHAR(50) NULL,
        [IsIncomplete] BIT NOT NULL DEFAULT 0,
        [IncompleteDeadline] DATETIME2 NULL,
        [IncompleteReason] NVARCHAR(MAX) NULL,
        [IsExcused] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]),
        FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]),
        FOREIGN KEY ([SubmittedByUserId]) REFERENCES [dbo].[Users]([Id]),
        FOREIGN KEY ([ApprovedByUserId]) REFERENCES [dbo].[Users]([Id])
    )
END
GO

-- ============================================================================
-- 7. ATTENDANCE TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Attendance')
BEGIN
    CREATE TABLE [dbo].[Attendance]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [StudentId] INT NOT NULL,
        [CourseId] INT NOT NULL,
        [ClassDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Absent', -- Present, Absent, Late, Excused
        [TimeMarked] TIME NULL,
        [Reason] NVARCHAR(500) NULL,
        [IsApproved] BIT NOT NULL DEFAULT 0,
        [AttachmentUrl] NVARCHAR(MAX) NULL,
        [AttachmentType] NVARCHAR(100) NULL,
        [MarkedByUserId] INT NULL,
        [MarkedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsLocked] BIT NOT NULL DEFAULT 0,
        [Remarks] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]),
        FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]),
        FOREIGN KEY ([MarkedByUserId]) REFERENCES [dbo].[Users]([Id])
    )
END
GO

-- ============================================================================
-- 8. CLASS SCHEDULES TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClassSchedules')
BEGIN
    CREATE TABLE [dbo].[ClassSchedules]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CourseId] INT NOT NULL,
        [DayOfWeek] NVARCHAR(20) NOT NULL,
        [StartTime] TIME NOT NULL,
        [EndTime] TIME NOT NULL,
        [Classroom] NVARCHAR(100) NOT NULL,
        [InstructorUserId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]),
        FOREIGN KEY ([InstructorUserId]) REFERENCES [dbo].[Users]([Id])
    )
END
GO

-- ============================================================================
-- 9. ATTENDANCE SUMMARY TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AttendanceSummaries')
BEGIN
    CREATE TABLE [dbo].[AttendanceSummaries]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [StudentId] INT NOT NULL,
        [CourseId] INT NOT NULL,
        [Semester] NVARCHAR(50) NOT NULL,
        [TotalClassesHeld] INT NOT NULL DEFAULT 0,
        [PresentCount] INT NOT NULL DEFAULT 0,
        [AbsentCount] INT NOT NULL DEFAULT 0,
        [LateCount] INT NOT NULL DEFAULT 0,
        [ExcusedCount] INT NOT NULL DEFAULT 0,
        [AttendancePercentage] DECIMAL(5, 2) NOT NULL DEFAULT 0.00,
        [IsBelowMinimum] BIT NOT NULL DEFAULT 0,
        [Remarks] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]),
        FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]),
        UNIQUE ([StudentId], [CourseId], [Semester])
    )
END
GO

-- ============================================================================
-- 10. LOGS TABLE (for Serilog)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Logs')
BEGIN
    CREATE TABLE [dbo].[Logs]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [MessageTemplate] NVARCHAR(MAX) NULL,
        [Level] VARCHAR(128) NULL,
        [TimeStamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Exception] NVARCHAR(MAX) NULL,
        [LogEvent] NVARCHAR(MAX) NULL
    )
END
GO

PRINT 'EMS Database schema created successfully!'
