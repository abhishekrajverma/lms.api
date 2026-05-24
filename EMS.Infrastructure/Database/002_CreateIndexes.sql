-- ============================================================================
-- EMS Database Indexes - SQL Server
-- Improves query performance for commonly accessed columns
-- ============================================================================

USE [EMS_DB]
GO

-- ============================================================================
-- Users Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users]([Username])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Role')
    CREATE NONCLUSTERED INDEX [IX_Users_Role] ON [dbo].[Users]([Role])
GO

-- ============================================================================
-- Students Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Students_EnrollmentNumber')
    CREATE NONCLUSTERED INDEX [IX_Students_EnrollmentNumber] ON [dbo].[Students]([EnrollmentNumber])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Students_DepartmentId')
    CREATE NONCLUSTERED INDEX [IX_Students_DepartmentId] ON [dbo].[Students]([DepartmentId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Students_Status')
    CREATE NONCLUSTERED INDEX [IX_Students_Status] ON [dbo].[Students]([Status])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Students_UserId')
    CREATE NONCLUSTERED INDEX [IX_Students_UserId] ON [dbo].[Students]([UserId])
GO

-- ============================================================================
-- Courses Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_CourseCode')
    CREATE NONCLUSTERED INDEX [IX_Courses_CourseCode] ON [dbo].[Courses]([CourseCode])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_DepartmentId')
    CREATE NONCLUSTERED INDEX [IX_Courses_DepartmentId] ON [dbo].[Courses]([DepartmentId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_InstructorUserId')
    CREATE NONCLUSTERED INDEX [IX_Courses_InstructorUserId] ON [dbo].[Courses]([InstructorUserId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_Status')
    CREATE NONCLUSTERED INDEX [IX_Courses_Status] ON [dbo].[Courses]([Status])
GO

-- ============================================================================
-- Enrollments Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId')
    CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId] ON [dbo].[Enrollments]([StudentId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId')
    CREATE NONCLUSTERED INDEX [IX_Enrollments_CourseId] ON [dbo].[Enrollments]([CourseId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Enrollments_IsActive] ON [dbo].[Enrollments]([IsActive])
GO

-- ============================================================================
-- Grades Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Grades_StudentId')
    CREATE NONCLUSTERED INDEX [IX_Grades_StudentId] ON [dbo].[Grades]([StudentId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Grades_CourseId')
    CREATE NONCLUSTERED INDEX [IX_Grades_CourseId] ON [dbo].[Grades]([CourseId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Grades_IsApproved')
    CREATE NONCLUSTERED INDEX [IX_Grades_IsApproved] ON [dbo].[Grades]([IsApproved])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Grades_Semester')
    CREATE NONCLUSTERED INDEX [IX_Grades_Semester] ON [dbo].[Grades]([Semester])
GO

-- ============================================================================
-- Attendance Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Attendance_StudentId')
    CREATE NONCLUSTERED INDEX [IX_Attendance_StudentId] ON [dbo].[Attendance]([StudentId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Attendance_CourseId')
    CREATE NONCLUSTERED INDEX [IX_Attendance_CourseId] ON [dbo].[Attendance]([CourseId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Attendance_ClassDate')
    CREATE NONCLUSTERED INDEX [IX_Attendance_ClassDate] ON [dbo].[Attendance]([ClassDate])
GO

-- ============================================================================
-- AttendanceSummaries Table Indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AttendanceSummaries_StudentId')
    CREATE NONCLUSTERED INDEX [IX_AttendanceSummaries_StudentId] ON [dbo].[AttendanceSummaries]([StudentId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AttendanceSummaries_CourseId')
    CREATE NONCLUSTERED INDEX [IX_AttendanceSummaries_CourseId] ON [dbo].[AttendanceSummaries]([CourseId])
GO

PRINT 'All indexes created successfully!'
