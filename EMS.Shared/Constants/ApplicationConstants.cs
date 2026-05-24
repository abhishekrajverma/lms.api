namespace EMS.Shared.Constants;

/// <summary>
/// Application-wide constants for the EMS (Educational Management System)
/// These are default values, configuration values, and enumeration constants
/// used across all layers of the application.
/// 
/// Benefits:
/// - Single source of truth for configuration
/// - Easy to modify without searching code
/// - Type-safe constants (no magic strings/numbers)
/// - Centralized configuration management
/// </summary>
public static class ApplicationConstants
{
    #region API Configuration
    
    /// <summary>
    /// Current API version
    /// Used in: /api/v1/endpoints
    /// </summary>
    public const string ApiVersion = "v1";

    /// <summary>
    /// Application name
    /// </summary>
    public const string ApplicationName = "Educational Management System (EMS)";

    /// <summary>
    /// Default time zone for the application (UTC)
    /// All timestamps stored in UTC, displayed in local timezone on client
    /// </summary>
    public const string DefaultTimeZone = "UTC";

    #endregion

    #region Pagination Configuration
    
    /// <summary>
    /// Default page size when not specified
    /// Used when client doesn't provide pageSize parameter
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum allowed page size
    /// Prevents clients from requesting 10,000 records at once
    /// Safety measure against performance issues
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Minimum page size allowed
    /// </summary>
    public const int MinPageSize = 1;

    #endregion

    #region Caching Configuration
    
    /// <summary>
    /// Default cache duration in minutes
    /// Used for: Student lists, Course lists, Grade summaries
    /// </summary>
    public const int CacheDurationMinutes = 30;

    /// <summary>
    /// Cache duration for frequently accessed data (in minutes)
    /// Used for: Student grades, Course details, Schedule
    /// </summary>
    public const int FrequentAccessCacheDurationMinutes = 15;

    /// <summary>
    /// Cache duration for rarely changed data (in minutes)
    /// Used for: School settings, Grade scales, Academic calendars
    /// </summary>
    public const int RareCacheDurationMinutes = 120;

    /// <summary>
    /// Cache key prefix for student data
    /// Example: "student_1001" for student with ID 1001
    /// </summary>
    public const string StudentCacheKeyPrefix = "student_";

    /// <summary>
    /// Cache key prefix for course data
    /// </summary>
    public const string CourseCacheKeyPrefix = "course_";

    /// <summary>
    /// Cache key prefix for grade data
    /// </summary>
    public const string GradeCacheKeyPrefix = "grade_";

    #endregion

    #region User Roles
    
    /// <summary>
    /// Admin role - Full system access
    /// Can: Create users, manage courses, view reports, modify settings
    /// </summary>
    public const string RoleAdmin = "Admin";

    /// <summary>
    /// Faculty/Teacher role
    /// Can: Submit grades, mark attendance, view assigned courses
    /// </summary>
    public const string RoleFaculty = "Faculty";

    /// <summary>
    /// Student role
    /// Can: View own grades, download documents, view schedule
    /// </summary>
    public const string RoleStudent = "Student";

    /// <summary>
    /// Parent/Guardian role
    /// Can: View child's grades and attendance
    /// </summary>
    public const string RoleParent = "Parent";

    /// <summary>
    /// Department Head role
    /// Can: Manage department, view department reports, approve courses
    /// </summary>
    public const string RoleDepartmentHead = "DepartmentHead";

    #endregion

    #region Student Status
    
    /// <summary>
    /// Student is active and enrolled
    /// </summary>
    public const string StudentStatusActive = "Active";

    /// <summary>
    /// Student is inactive or not currently enrolled
    /// </summary>
    public const string StudentStatusInactive = "Inactive";

    /// <summary>
    /// Student is on leave
    /// </summary>
    public const string StudentStatusOnLeave = "OnLeave";

    /// <summary>
    /// Student has graduated
    /// </summary>
    public const string StudentStatusGraduated = "Graduated";

    /// <summary>
    /// Student enrollment is suspended
    /// </summary>
    public const string StudentStatusSuspended = "Suspended";

    #endregion

    #region Course Status
    
    /// <summary>
    /// Course is active and accepting enrollments
    /// </summary>
    public const string CourseStatusActive = "Active";

    /// <summary>
    /// Course enrollment is closed
    /// </summary>
    public const string CourseStatusClosed = "Closed";

    /// <summary>
    /// Course has ended for this semester
    /// </summary>
    public const string CourseStatusCompleted = "Completed";

    /// <summary>
    /// Course is temporarily suspended
    /// </summary>
    public const string CourseStatusSuspended = "Suspended";

    #endregion

    #region Attendance Status
    
    /// <summary>
    /// Student was present
    /// </summary>
    public const string AttendanceStatusPresent = "Present";

    /// <summary>
    /// Student was absent
    /// </summary>
    public const string AttendanceStatusAbsent = "Absent";

    /// <summary>
    /// Student was late
    /// </summary>
    public const string AttendanceStatusLate = "Late";

    /// <summary>
    /// Student was excused/on approved leave
    /// </summary>
    public const string AttendanceStatusExcused = "Excused";

    #endregion

    #region Grade Configuration
    
    /// <summary>
    /// Scale used for grades (4.0 GPA scale)
    /// </summary>
    public const decimal MaxGradePoint = 4.0m;

    /// <summary>
    /// Minimum passing GPA
    /// </summary>
    public const decimal MinimumPassingGpa = 2.0m;

    /// <summary>
    /// Default grade scale (out of 100)
    /// </summary>
    public const int DefaultGradeScale = 100;

    /// <summary>
    /// Weight for internal assessment (40%)
    /// </summary>
    public const decimal InternalAssessmentWeight = 0.40m;

    /// <summary>
    /// Weight for final examination (60%)
    /// </summary>
    public const decimal FinalExamWeight = 0.60m;

    #endregion

    #region Academic Calendar
    
    /// <summary>
    /// Number of semesters per academic year
    /// </summary>
    public const int SemestersPerYear = 2;

    /// <summary>
    /// Standard academic year format
    /// Example: "2024-2025"
    /// </summary>
    public const string AcademicYearFormat = "yyyy-yyyy";

    #endregion

    #region File Configuration
    
    /// <summary>
    /// Maximum file size for documents (in MB)
    /// </summary>
    public const int MaxFileUploadSizeMb = 10;

    /// <summary>
    /// Allowed file extensions for document uploads
    /// </summary>
    public static readonly List<string> AllowedFileExtensions = new() { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };

    #endregion

    #region Validation Rules
    
    /// <summary>
    /// Minimum length for student name
    /// </summary>
    public const int MinNameLength = 2;

    /// <summary>
    /// Maximum length for student name
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Standard enrollment number format length
    /// Example: "20240001" (8 characters)
    /// </summary>
    public const int EnrollmentNumberLength = 8;

    /// <summary>
    /// Minimum password length
    /// </summary>
    public const int MinPasswordLength = 8;

    /// <summary>
    /// Maximum password length
    /// </summary>
    public const int MaxPasswordLength = 128;

    #endregion

    #region Email Configuration
    
    /// <summary>
    /// From email address for system emails
    /// </summary>
    public const string SystemEmailAddress = "noreply@ems.edu";

    /// <summary>
    /// Support email address
    /// </summary>
    public const string SupportEmailAddress = "support@ems.edu";

    #endregion

    #region DateTime Configuration
    
    /// <summary>
    /// Standard date format for API responses
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Standard datetime format for API responses (ISO 8601)
    /// </summary>
    public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    #endregion

    #region Performance & Database
    
    /// <summary>
    /// Slow query threshold in milliseconds
    /// Queries taking longer than this are logged as warnings
    /// </summary>
    public const long SlowQueryThresholdMs = 5000;

    /// <summary>
    /// Request timeout duration in seconds
    /// </summary>
    public const int RequestTimeoutSeconds = 30;

    /// <summary>
    /// Database connection timeout in seconds
    /// </summary>
    public const int DatabaseConnectionTimeoutSeconds = 15;

    #endregion

    #region AI/ML Integration (Future)
    
    /// <summary>
    /// Enable AI features (for future integration)
    /// </summary>
    public const bool AiFeatureEnabled = false;

    /// <summary>
    /// AI service endpoint (for future use)
    /// </summary>
    public const string AiServiceEndpoint = "https://api.ai-service.com";

    #endregion

    #region Helper Methods
    
    /// <summary>
    /// Gets all available student statuses
    /// </summary>
    public static List<string> GetAllStudentStatuses() => new()
    {
        StudentStatusActive,
        StudentStatusInactive,
        StudentStatusOnLeave,
        StudentStatusGraduated,
        StudentStatusSuspended
    };

    /// <summary>
    /// Gets all available course statuses
    /// </summary>
    public static List<string> GetAllCourseStatuses() => new()
    {
        CourseStatusActive,
        CourseStatusClosed,
        CourseStatusCompleted,
        CourseStatusSuspended
    };

    /// <summary>
    /// Gets all available user roles
    /// </summary>
    public static List<string> GetAllUserRoles() => new()
    {
        RoleAdmin,
        RoleFaculty,
        RoleStudent,
        RoleParent,
        RoleDepartmentHead
    };

    #endregion
}
