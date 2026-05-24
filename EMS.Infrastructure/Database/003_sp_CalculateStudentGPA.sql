-- ============================================================================
-- Stored Procedure: sp_CalculateStudentGPA
-- Purpose: Calculate student's cumulative GPA
-- ============================================================================

USE [EMS_DB]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'sp_CalculateStudentGPA' AND type = 'P')
    DROP PROCEDURE [dbo].[sp_CalculateStudentGPA]
GO

CREATE PROCEDURE [dbo].[sp_CalculateStudentGPA]
    @StudentId INT,
    @GPA DECIMAL(3,2) OUTPUT
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
        
        -- Calculate average GPA points from approved grades
        SELECT @GPA = ISNULL(AVG([GpaPoints]), 0.0)
        FROM [dbo].[Grades]
        WHERE [StudentId] = @StudentId 
          AND [IsApproved] = 1
          AND [IsDeleted] = 0
        
        -- Round to 2 decimal places
        SET @GPA = ROUND(@GPA, 2)
        
        -- Update student's current GPA
        UPDATE [dbo].[Students]
        SET [CurrentGPA] = @GPA,
            [UpdatedAt] = GETUTCDATE()
        WHERE [Id] = @StudentId
        
        PRINT 'GPA calculated successfully for student ' + CAST(@StudentId AS NVARCHAR(10)) + ': ' + CAST(@GPA AS NVARCHAR(10))
    END TRY
    BEGIN CATCH
        PRINT 'Error: ' + ERROR_MESSAGE()
        RAISERROR (ERROR_MESSAGE(), 16, 1)
    END CATCH
END
GO

PRINT 'Stored procedure sp_CalculateStudentGPA created successfully'
