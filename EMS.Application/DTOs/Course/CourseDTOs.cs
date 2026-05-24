namespace EMS.Application.DTOs.Course;

/// <summary>
/// Request DTO for creating a new course
/// POST /api/v1/courses
/// </summary>
public class CreateCourseRequest
{
    /// <summary>
    /// Unique course code (e.g., CS101)
    /// </summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>
    /// Course name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Course description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Credit hours (typically 1-4)
    /// </summary>
    public int CreditHours { get; set; } = 3;

    /// <summary>
    /// Department offering the course
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Primary instructor user ID
    /// </summary>
    public int? InstructorUserId { get; set; }

    /// <summary>
    /// Maximum number of students
    /// </summary>
    public int Capacity { get; set; } = 50;

    /// <summary>
    /// Course start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Course end date
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Academic year (e.g., "2024-2025")
    /// </summary>
    public string AcademicYear { get; set; } = string.Empty;

    /// <summary>
    /// Semester type (Fall, Spring, Summer)
    /// </summary>
    public string SemesterType { get; set; } = "Fall";

    /// <summary>
    /// Course level (1, 2, 3, 4)
    /// </summary>
    public int CourseLevel { get; set; } = 1;

    /// <summary>
    /// Is course mandatory?
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Prerequisites (comma-separated course codes)
    /// </summary>
    public string? Prerequisites { get; set; }

    /// <summary>
    /// Classroom location
    /// </summary>
    public string? ClassRoom { get; set; }

    /// <summary>
    /// Meeting times (e.g., "MWF 10:00 AM - 11:00 AM")
    /// </summary>
    public string? ScheduleTime { get; set; }

    /// <summary>
    /// Grading policy description
    /// </summary>
    public string? GradingPolicy { get; set; }

    /// <summary>
    /// Syllabus document URL
    /// </summary>
    public string? SyllabusUrl { get; set; }

    /// <summary>
    /// Course materials/textbook info
    /// </summary>
    public string? Materials { get; set; }

    /// <summary>
    /// Delivery mode (Online, InPerson, Hybrid)
    /// </summary>
    public string? DeliveryMode { get; set; } = "InPerson";

    /// <summary>
    /// Online meeting link (for remote courses)
    /// </summary>
    public string? OnlineMeetingLink { get; set; }

    /// <summary>
    /// Passing grade percentage
    /// </summary>
    public decimal PassingGradePercentage { get; set; } = 40.0m;
}

/// <summary>
/// Request DTO for updating an existing course
/// PUT /api/v1/courses/{id}
/// </summary>
public class UpdateCourseRequest
{
    /// <summary>
    /// Course name (optional)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Course description (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Credit hours (optional)
    /// </summary>
    public int? CreditHours { get; set; }

    /// <summary>
    /// Instructor user ID (optional)
    /// </summary>
    public int? InstructorUserId { get; set; }

    /// <summary>
    /// Course capacity (optional)
    /// </summary>
    public int? Capacity { get; set; }

    /// <summary>
    /// Course status (optional)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// End date (optional)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Prerequisites (optional)
    /// </summary>
    public string? Prerequisites { get; set; }

    /// <summary>
    /// Classroom (optional)
    /// </summary>
    public string? ClassRoom { get; set; }

    /// <summary>
    /// Schedule time (optional)
    /// </summary>
    public string? ScheduleTime { get; set; }

    /// <summary>
    /// Grading policy (optional)
    /// </summary>
    public string? GradingPolicy { get; set; }

    /// <summary>
    /// Syllabus URL (optional)
    /// </summary>
    public string? SyllabusUrl { get; set; }

    /// <summary>
    /// Materials/textbook info (optional)
    /// </summary>
    public string? Materials { get; set; }

    /// <summary>
    /// Online meeting link (optional)
    /// </summary>
    public string? OnlineMeetingLink { get; set; }

    /// <summary>
    /// Passing grade percentage (optional)
    /// </summary>
    public decimal? PassingGradePercentage { get; set; }
}

/// <summary>
/// Response DTO for returning course data
/// GET /api/v1/courses/{id}
/// </summary>
public class CourseResponse
{
    /// <summary>
    /// Course ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Course code
    /// </summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>
    /// Course name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full course identifier
    /// </summary>
    public string FullIdentifier => $"{CourseCode} - {Name}";

    /// <summary>
    /// Course description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Credit hours
    /// </summary>
    public int CreditHours { get; set; }

    /// <summary>
    /// Department ID
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Department name
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Instructor user ID
    /// </summary>
    public int? InstructorUserId { get; set; }

    /// <summary>
    /// Instructor name
    /// </summary>
    public string? InstructorName { get; set; }

    /// <summary>
    /// Course capacity
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Currently enrolled students
    /// </summary>
    public int EnrolledStudentCount { get; set; }

    /// <summary>
    /// Available seats
    /// </summary>
    public int AvailableSeats => Capacity - EnrolledStudentCount;

    /// <summary>
    /// Course status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Days until start
    /// </summary>
    public int DaysUntilStart => (int)(StartDate - DateTime.UtcNow).TotalDays;

    /// <summary>
    /// Has course started?
    /// </summary>
    public bool HasStarted { get; set; }

    /// <summary>
    /// Has course ended?
    /// </summary>
    public bool HasEnded { get; set; }

    /// <summary>
    /// Academic year
    /// </summary>
    public string AcademicYear { get; set; } = string.Empty;

    /// <summary>
    /// Semester type
    /// </summary>
    public string SemesterType { get; set; } = string.Empty;

    /// <summary>
    /// Course level
    /// </summary>
    public int CourseLevel { get; set; }

    /// <summary>
    /// Is mandatory?
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Prerequisites
    /// </summary>
    public string? Prerequisites { get; set; }

    /// <summary>
    /// Classroom location
    /// </summary>
    public string? ClassRoom { get; set; }

    /// <summary>
    /// Schedule time
    /// </summary>
    public string? ScheduleTime { get; set; }

    /// <summary>
    /// Grading policy
    /// </summary>
    public string? GradingPolicy { get; set; }

    /// <summary>
    /// Syllabus URL
    /// </summary>
    public string? SyllabusUrl { get; set; }

    /// <summary>
    /// Materials/textbooks
    /// </summary>
    public string? Materials { get; set; }

    /// <summary>
    /// Delivery mode
    /// </summary>
    public string? DeliveryMode { get; set; }

    /// <summary>
    /// Online meeting link
    /// </summary>
    public string? OnlineMeetingLink { get; set; }

    /// <summary>
    /// Passing grade percentage
    /// </summary>
    public decimal PassingGradePercentage { get; set; }

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
/// Response DTO for paginated course list
/// GET /api/v1/courses?pageNumber=1&pageSize=10
/// </summary>
public class CourseListResponse
{
    /// <summary>
    /// List of courses
    /// </summary>
    public List<CourseResponse> Items { get; set; } = [];

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
