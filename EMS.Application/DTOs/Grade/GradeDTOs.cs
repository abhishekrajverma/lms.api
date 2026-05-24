namespace EMS.Application.DTOs.Grade;

/// <summary>
/// Request DTO for creating/submitting a grade
/// POST /api/v1/grades
/// </summary>
public class CreateGradeRequest
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
    /// Semester (e.g., "Fall 2024")
    /// </summary>
    public string Semester { get; set; } = string.Empty;

    /// <summary>
    /// Internal assessment score (0-100)
    /// Represents 40% of final grade
    /// </summary>
    public decimal? InternalAssessmentScore { get; set; }

    /// <summary>
    /// Final exam score (0-100)
    /// Represents 60% of final grade
    /// </summary>
    public decimal? FinalExamScore { get; set; }

    /// <summary>
    /// Midterm score (optional, 0-100)
    /// </summary>
    public decimal? MidtermScore { get; set; }

    /// <summary>
    /// Project score (optional, 0-100)
    /// </summary>
    public decimal? ProjectScore { get; set; }

    /// <summary>
    /// Practical/Lab score (optional, 0-100)
    /// </summary>
    public decimal? PracticalScore { get; set; }

    /// <summary>
    /// Instructor comments/feedback
    /// </summary>
    public string? InstructorComments { get; set; }
}

/// <summary>
/// Request DTO for updating a grade
/// PUT /api/v1/grades/{id}
/// </summary>
public class UpdateGradeRequest
{
    /// <summary>
    /// Internal assessment score (optional)
    /// </summary>
    public decimal? InternalAssessmentScore { get; set; }

    /// <summary>
    /// Final exam score (optional)
    /// </summary>
    public decimal? FinalExamScore { get; set; }

    /// <summary>
    /// Midterm score (optional)
    /// </summary>
    public decimal? MidtermScore { get; set; }

    /// <summary>
    /// Project score (optional)
    /// </summary>
    public decimal? ProjectScore { get; set; }

    /// <summary>
    /// Practical score (optional)
    /// </summary>
    public decimal? PracticalScore { get; set; }

    /// <summary>
    /// Instructor comments (optional)
    /// </summary>
    public string? InstructorComments { get; set; }
}

/// <summary>
/// Response DTO for grade data
/// GET /api/v1/grades/{id}
/// </summary>
public class GradeResponse
{
    /// <summary>
    /// Grade record ID
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
    /// Semester
    /// </summary>
    public string Semester { get; set; } = string.Empty;

    /// <summary>
    /// Internal assessment score
    /// </summary>
    public decimal? InternalAssessmentScore { get; set; }

    /// <summary>
    /// Final exam score
    /// </summary>
    public decimal? FinalExamScore { get; set; }

    /// <summary>
    /// Midterm score
    /// </summary>
    public decimal? MidtermScore { get; set; }

    /// <summary>
    /// Project score
    /// </summary>
    public decimal? ProjectScore { get; set; }

    /// <summary>
    /// Practical score
    /// </summary>
    public decimal? PracticalScore { get; set; }

    /// <summary>
    /// Final percentage score (auto-calculated)
    /// </summary>
    public decimal FinalPercentageScore { get; set; }

    /// <summary>
    /// Letter grade (A, B, C, D, F)
    /// </summary>
    public string LetterGrade { get; set; } = string.Empty;

    /// <summary>
    /// GPA points (4.0, 3.0, 2.0, 1.0, 0.0)
    /// </summary>
    public decimal GpaPoints { get; set; }

    /// <summary>
    /// Is student passing?
    /// </summary>
    public bool IsPassing { get; set; }

    /// <summary>
    /// Instructor comments
    /// </summary>
    public string? InstructorComments { get; set; }

    /// <summary>
    /// Is grade submitted?
    /// </summary>
    public bool IsSubmitted { get; set; }

    /// <summary>
    /// When was grade submitted?
    /// </summary>
    public DateTime? SubmittedDate { get; set; }

    /// <summary>
    /// Is grade approved?
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// When was grade approved?
    /// </summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>
    /// Is grade disputed?
    /// </summary>
    public bool IsDisputed { get; set; }

    /// <summary>
    /// Is grade incomplete?
    /// </summary>
    public bool IsIncomplete { get; set; }

    /// <summary>
    /// Is grade excused?
    /// </summary>
    public bool IsExcused { get; set; }

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
/// Response DTO for paginated grade list
/// GET /api/v1/grades?pageNumber=1&pageSize=10
/// </summary>
public class GradeListResponse
{
    /// <summary>
    /// List of grades
    /// </summary>
    public List<GradeResponse> Items { get; set; } = [];

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
