-- ============================================================================
-- Stored Procedure: sp_EnrollStudent
-- Purpose: Enroll student in course with validation and atomic transaction
-- ============================================================================

USE [EMS_DB]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'sp_EnrollStudent' AND type = 'P')
    DROP PROCEDURE [dbo].[sp_EnrollStudent]
GO

CREATE PROCEDURE [dbo].[sp_EnrollStudent]
    @StudentId INT,
    @CourseId INT,
    @Semester NVARCHAR(50),
    @EnrollmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON
    
    BEGIN TRANSACTION
    
    BEGIN TRY
        -- Validate student exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Students] WHERE [Id] = @StudentId AND [IsDeleted] = 0)
        BEGIN
            RAISERROR ('Student not found', 16, 1)
        END
        
        -- Validate course exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Courses] WHERE [Id] = @CourseId AND [IsDeleted] = 0)
        BEGIN
            RAISERROR ('Course not found', 16, 1)
        END
        
        -- Check if already enrolled
        IF EXISTS (SELECT 1 FROM [dbo].[Enrollments] 
                   WHERE [StudentId] = @StudentId 
                     AND [CourseId] = @CourseId 
                     AND [IsActive] = 1
                     AND [IsDeleted] = 0)
        BEGIN
            RAISERROR ('Student is already enrolled in this course', 16, 1)
        END
        
        -- Check course status
        DECLARE @CourseStatus NVARCHAR(50)
        SELECT @CourseStatus = [Status] FROM [dbo].[Courses] WHERE [Id] = @CourseId
        
        IF @CourseStatus != 'Active'
        BEGIN
            RAISERROR ('Course is not accepting new enrollments', 16, 1)
        END
        
        -- Check course capacity
        DECLARE @Capacity INT, @EnrolledCount INT
        SELECT @Capacity = [Capacity], @EnrolledCount = [EnrolledStudentCount]
        FROM [dbo].[Courses]
        WHERE [Id] = @CourseId
        
        IF @EnrolledCount >= @Capacity
        BEGIN
            RAISERROR ('Course is at capacity', 16, 1)
        END
        
        -- All validations passed - insert enrollment
        INSERT INTO [dbo].[Enrollments]
            ([StudentId], [CourseId], [Semester], [IsActive], [EnrollmentDate], [CreatedAt])
        VALUES
            (@StudentId, @CourseId, @Semester, 1, GETUTCDATE(), GETUTCDATE())
        
        SET @EnrollmentId = SCOPE_IDENTITY()
        
        -- Update course enrollment count
        UPDATE [dbo].[Courses]
        SET [EnrolledStudentCount] = [EnrolledStudentCount] + 1,
            [UpdatedAt] = GETUTCDATE()
        WHERE [Id] = @CourseId
        
        -- Commit transaction
        COMMIT TRANSACTION
        
        PRINT 'Student ' + CAST(@StudentId AS NVARCHAR(10)) + 
              ' successfully enrolled in course ' + CAST(@CourseId AS NVARCHAR(10)) +
              ' (Enrollment ID: ' + CAST(@EnrollmentId AS NVARCHAR(10)) + ')'
    END TRY
    BEGIN CATCH
        -- Rollback on error
        ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE()
        PRINT 'Enrollment error: ' + @ErrorMessage
        RAISERROR (@ErrorMessage, 16, 1)
    END CATCH
END
GO

PRINT 'Stored procedure sp_EnrollStudent created successfully'
