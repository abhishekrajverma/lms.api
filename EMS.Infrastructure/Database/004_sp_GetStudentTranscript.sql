-- ============================================================================
-- Stored Procedure: sp_GetStudentTranscript
-- Purpose: Get detailed grade transcript for a student
-- ============================================================================

USE [EMS_DB]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'sp_GetStudentTranscript' AND type = 'P')
    DROP PROCEDURE [dbo].[sp_GetStudentTranscript]
GO

CREATE PROCEDURE [dbo].[sp_GetStudentTranscript]
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON
    
    BEGIN TRY
        -- Check if student exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Students] WHERE [Id] = @StudentId)
        BEGIN
            RAISERROR ('Student not found', 16, 1)
            RETURN
        END
        
        -- Return detailed transcript
        SELECT 
            s.[Id] AS StudentId,
            s.[EnrollmentNumber],
            s.[FirstName],
            s.[LastName],
            c.[Id] AS CourseId,
            c.[CourseCode],
            c.[Name] AS CourseName,
            c.[CreditHours],
            d.[Id] AS DepartmentId,
            d.[Name] AS DepartmentName,
            g.[Id] AS GradeId,
            g.[InternalAssessmentScore],
            g.[FinalExamScore],
            g.[FinalPercentageScore],
            g.[LetterGrade],
            g.[GpaPoints],
            g.[Semester],
            g.[IsApproved],
            g.[IsSubmitted]
        FROM [dbo].[Students] s
        INNER JOIN [dbo].[Grades] g ON s.[Id] = g.[StudentId]
        INNER JOIN [dbo].[Courses] c ON g.[CourseId] = c.[Id]
        INNER JOIN [dbo].[Departments] d ON c.[DepartmentId] = d.[Id]
        WHERE s.[Id] = @StudentId 
          AND g.[IsApproved] = 1
          AND g.[IsDeleted] = 0
          AND s.[IsDeleted] = 0
        ORDER BY g.[Semester] DESC, c.[CourseCode] ASC
    END TRY
    BEGIN CATCH
        PRINT 'Error: ' + ERROR_MESSAGE()
        RAISERROR (ERROR_MESSAGE(), 16, 1)
    END CATCH
END
GO

PRINT 'Stored procedure sp_GetStudentTranscript created successfully'
