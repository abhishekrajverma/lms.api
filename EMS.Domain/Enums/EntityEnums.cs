namespace EMS.Domain.Enums;

/// <summary>
/// Student enrollment status in the system
/// Determines if student can access courses, submit assignments, etc.
/// </summary>
public enum StudentStatus
{
    /// <summary>Active student currently enrolled</summary>
    Active = 1,

    /// <summary>Student not currently enrolled</summary>
    Inactive = 2,

    /// <summary>Student on approved leave</summary>
    OnLeave = 3,

    /// <summary>Student has completed degree</summary>
    Graduated = 4,

    /// <summary>Student enrollment suspended (disciplinary action)</summary>
    Suspended = 5
}

/// <summary>
/// Course enrollment status
/// Determines if new students can enroll and if grades can be submitted
/// </summary>
public enum CourseStatus
{
    /// <summary>Course is active and accepting enrollments</summary>
    Active = 1,

    /// <summary>Course enrollment is closed (but can still submit grades)</summary>
    Closed = 2,

    /// <summary>Course completed for this semester</summary>
    Completed = 3,

    /// <summary>Course temporarily suspended</summary>
    Suspended = 4
}

/// <summary>
/// Student attendance status for a specific class session
/// Used for marking attendance and calculating attendance percentage
/// </summary>
public enum AttendanceStatus
{
    /// <summary>Student was present</summary>
    Present = 1,

    /// <summary>Student was absent</summary>
    Absent = 2,

    /// <summary>Student arrived late</summary>
    Late = 3,

    /// <summary>Student absence excused/approved</summary>
    Excused = 4
}

/// <summary>
/// Letter grades for courses
/// Mapped to GPA points and percentage ranges
/// </summary>
public enum GradeLetter
{
    /// <summary>A: 90-100% (4.0 GPA)</summary>
    A = 1,

    /// <summary>B: 80-89% (3.0 GPA)</summary>
    B = 2,

    /// <summary>C: 70-79% (2.0 GPA)</summary>
    C = 3,

    /// <summary>D: 60-69% (1.0 GPA)</summary>
    D = 4,

    /// <summary>F: Below 60% (0.0 GPA) - Fail</summary>
    F = 5,

    /// <summary>Grade not yet submitted</summary>
    NotGraded = 6,

    /// <summary>Incomplete - Student needs to finish work</summary>
    Incomplete = 7,

    /// <summary>Grade withdrawn</summary>
    Withdrawn = 8
}

/// <summary>
/// User roles in the EMS system
/// Determines access level and permissions
/// </summary>
public enum UserRole
{
    /// <summary>Administrator - Full system access</summary>
    Admin = 1,

    /// <summary>Faculty/Teacher - Can submit grades, mark attendance</summary>
    Faculty = 2,

    /// <summary>Student - Can view grades, enroll in courses</summary>
    Student = 3,

    /// <summary>Parent/Guardian - Can view child's grades</summary>
    Parent = 4,

    /// <summary>Department Head - Department-level access</summary>
    DepartmentHead = 5,

    /// <summary>Finance staff - Access to fee/payment information</summary>
    Finance = 6
}

/// <summary>
/// Academic semester type within a year
/// </summary>
public enum SemesterType
{
    /// <summary>First semester (typically Aug-Dec)</summary>
    Fall = 1,

    /// <summary>Second semester (typically Jan-May)</summary>
    Spring = 2,

    /// <summary>Summer semester (typically Jun-Jul)</summary>
    Summer = 3
}

/// <summary>
/// User account status in the system
/// Controls login access and functionality
/// </summary>
public enum AccountStatus
{
    /// <summary>Account is active and can login</summary>
    Active = 1,

    /// <summary>Account is locked (too many failed logins)</summary>
    Locked = 2,

    /// <summary>Account is disabled by admin</summary>
    Disabled = 3,

    /// <summary>Account pending email verification</summary>
    PendingVerification = 4,

    /// <summary>Account temporarily suspended</summary>
    Suspended = 5
}
