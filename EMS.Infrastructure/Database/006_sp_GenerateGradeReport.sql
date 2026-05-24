-- ============================================================================
-- Stored Procedure: sp_GenerateGradeReport
-- Purpose: Generate course grade report with statistics
-- ============================================================================

USE [EMS_DB]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'sp_GenerateGradeReport' AND type = 'P')
    DROP PROCEDURE [dbo].[sp_GenerateGradeReport]
GO

CREATE PROCEDURE [dbo].[sp_GenerateGradeReport]
    @CourseId INT
AS
BEGIN
    SET NOCOUNT ON
    
    BEGIN TRY
        -- Check if course exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Courses] WHERE [Id] = @CourseId)
        BEGIN
            RAISERROR ('Course not found', 16, 1)
            RETURN
        END
        
        -- Return grade report
        SELECT 
            c.[CourseCode],
            c.[Name] AS CourseName,
            s.[EnrollmentNumber],
            s.[FirstName],
            s.[LastName],
            g.[InternalAssessmentScore],
            g.[FinalExamScore],
            g.[FinalPercentageScore],
            g.[LetterGrade],
            g.[GpaPoints],
            g.[Semester],
            CASE 
                WHEN g.[IsSubmitted] = 1 AND g.[IsApproved] = 1 THEN 'Approved'
                WHEN g.[IsSubmitted] = 1 THEN 'Submitted'
                ELSE 'Draft'
            END AS Status,
            g.[IsApproved],
            g.[IsSubmitted]
        FROM [dbo].[Grades] g
        INNER JOIN [dbo].[Students] s ON g.[StudentId] = s.[Id]
        INNER JOIN [dbo].[Courses] c ON g.[CourseId] = c.[Id]
        WHERE g.[CourseId] = @CourseId 
          AND g.[IsDeleted] = 0
          AND s.[IsDeleted] = 0
        ORDER BY s.[LastName], s.[FirstName]
        
        -- Return statistics
        SELECT 
            'Statistics' AS Category,
            COUNT(DISTINCT g.[StudentId]) AS TotalStudents,
            ROUND(AVG(CAST(g.[FinalPercentageScore] AS DECIMAL(5,2))), 2) AS AvgPercentage,
            MIN(g.[FinalPercentageScore]) AS MinPercentage,
            MAX(g.[FinalPercentageScore]) AS MaxPercentage,
            SUM(CASE WHEN g.[LetterGrade] = 'A' THEN 1 ELSE 0 END) AS GradeA_Count,
            SUM(CASE WHEN g.[LetterGrade] = 'B' THEN 1 ELSE 0 END) AS GradeB_Count,
            SUM(CASE WHEN g.[LetterGrade] = 'C' THEN 1 ELSE 0 END) AS GradeC_Count,
            SUM(CASE WHEN g.[LetterGrade] = 'D' THEN 1 ELSE 0 END) AS GradeD_Count,
            SUM(CASE WHEN g.[LetterGrade] = 'F' THEN 1 ELSE 0 END) AS GradeF_Count,
            SUM(CASE WHEN g.[IsApproved] = 1 THEN 1 ELSE 0 END) AS ApprovedCount
        FROM [dbo].[Grades] g
        WHERE g.[CourseId] = @CourseId 
          AND g.[IsDeleted] = 0
    END TRY
    BEGIN CATCH
        PRINT 'Error: ' + ERROR_MESSAGE()
        RAISERROR (ERROR_MESSAGE(), 16, 1)
    END CATCH
END
GO

PRINT 'Stored procedure sp_GenerateGradeReport created successfully'
