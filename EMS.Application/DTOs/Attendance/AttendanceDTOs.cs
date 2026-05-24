namespace EMS.Application.DTOs.Attendance;

/// <summary>
/// Request DTO for marking attendance
/// POST /api/v1/attendance
/// </summary>
public class CreateAttendanceRequest
{
    /// <summary>
    /// Student ID
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// Course ID
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Class date
    /// </summary>
    public DateTime ClassDate { get; set; }

    /// <summary>
    /// Attendance status (Present, Absent, Late, Excused)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Time when attendance was recorded (optional)
    /// </summary>
    public TimeSpan? TimeMarked { get; set; }

    /// <summary>
    /// Reason for absence or lateness (optional)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Document/certificate attachment URL (optional)
    /// </summary>
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// Type of attachment (optional)
    /// </summary>
    public string? AttachmentType { get; set; }
}

/// <summary>
/// Request DTO for updating attendance
/// PUT /api/v1/attendance/{id}
/// </summary>
public class UpdateAttendanceRequest
{
    /// <summary>
    /// Attendance status (optional)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Reason for change (optional)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Approve absence? (optional)
    /// </summary>
    public bool? IsApproved { get; set; }

    /// <summary>
    /// Remarks from faculty (optional)
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Attachment URL (optional)
    /// </summary>
    public string? AttachmentUrl { get; set; }
}

/// <summary>
/// Response DTO for attendance data
/// GET /api/v1/attendance/{id}
/// </summary>
public class AttendanceResponse
{
    /// <summary>
    /// Attendance record ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Student ID
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// Student name
    /// </summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>
    /// Student enrollment number
    /// </summary>
    public string StudentEnrollmentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Course ID
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Course code
    /// </summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>
    /// Course name
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Class date
    /// </summary>
    public DateTime ClassDate { get; set; }

    /// <summary>
    /// Attendance status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Time marked
    /// </summary>
    public TimeSpan? TimeMarked { get; set; }

    /// <summary>
    /// Reason for absence/lateness
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Is approved/excused?
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Attachment URL
    /// </summary>
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// Attachment type
    /// </summary>
    public string? AttachmentType { get; set; }

    /// <summary>
    /// Faculty remarks
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// When was attendance marked?
    /// </summary>
    public DateTime MarkedDate { get; set; }

    /// <summary>
    /// Is attendance locked?
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Response DTO for paginated attendance list
/// GET /api/v1/attendance?pageNumber=1&pageSize=10
/// </summary>
public class AttendanceListResponse
{
    /// <summary>
    /// List of attendance records
    /// </summary>
    public List<AttendanceResponse> Items { get; set; } = [];

    /// <summary>
    /// Total count
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Has previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Has next page
    /// </summary>
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Response DTO for attendance summary
/// GET /api/v1/students/{id}/attendance-summary
/// </summary>
public class AttendanceSummaryResponse
{
    /// <summary>
    /// Student ID
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// Student name
    /// </summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>
    /// Course ID
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Course code
    /// </summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>
    /// Course name
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Semester
    /// </summary>
    public string Semester { get; set; } = string.Empty;

    /// <summary>
    /// Total classes held
    /// </summary>
    public int TotalClassesHeld { get; set; }

    /// <summary>
    /// Number of times present
    /// </summary>
    public int PresentCount { get; set; }

    /// <summary>
    /// Number of times absent
    /// </summary>
    public int AbsentCount { get; set; }

    /// <summary>
    /// Number of times late
    /// </summary>
    public int LateCount { get; set; }

    /// <summary>
    /// Number of excused absences
    /// </summary>
    public int ExcusedCount { get; set; }

    /// <summary>
    /// Attendance percentage
    /// </summary>
    public decimal AttendancePercentage { get; set; }

    /// <summary>
    /// Is below minimum requirement?
    /// </summary>
    public bool IsBelowMinimum { get; set; }

    /// <summary>
    /// Remarks
    /// </summary>
    public string? Remarks { get; set; }
}
