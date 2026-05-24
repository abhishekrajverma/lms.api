namespace EMS.Application.DTOs.Student;

/// <summary>
/// Request DTO for creating a new student
/// Client sends this to POST /api/v1/students
/// </summary>
public class CreateStudentRequest
{
    /// <summary>
    /// Student's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Student's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Student's date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Student's gender (Male, Female, Other)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Student's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Personal email address
    /// </summary>
    public string? PersonalEmail { get; set; }

    /// <summary>
    /// Student's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Student's city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Student's state/province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Student's country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Student's postal code
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Department ID student belongs to
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Year of admission
    /// </summary>
    public int AdmissionYear { get; set; }

    /// <summary>
    /// Parent/Guardian first name
    /// </summary>
    public string? ParentFirstName { get; set; }

    /// <summary>
    /// Parent/Guardian last name
    /// </summary>
    public string? ParentLastName { get; set; }

    /// <summary>
    /// Parent/Guardian phone number
    /// </summary>
    public string? ParentPhoneNumber { get; set; }

    /// <summary>
    /// Parent/Guardian email address
    /// </summary>
    public string? ParentEmail { get; set; }

    /// <summary>
    /// Relationship of parent/guardian
    /// </summary>
    public string? ParentRelationship { get; set; }

    /// <summary>
    /// Emergency contact name
    /// </summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone
    /// </summary>
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// Username for login (sent to User creation)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email for login account (sent to User creation)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for login account (will be hashed in service)
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating an existing student
/// Client sends this to PUT /api/v1/students/{id}
/// All fields are optional (only provided fields will be updated)
/// </summary>
public class UpdateStudentRequest
{
    /// <summary>
    /// Student's first name (optional)
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Student's last name (optional)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Student's gender (optional)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Student's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Personal email (optional)
    /// </summary>
    public string? PersonalEmail { get; set; }

    /// <summary>
    /// Student's address (optional)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Student's city (optional)
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Student's state (optional)
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Student's country (optional)
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Student's postal code (optional)
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Parent first name (optional)
    /// </summary>
    public string? ParentFirstName { get; set; }

    /// <summary>
    /// Parent last name (optional)
    /// </summary>
    public string? ParentLastName { get; set; }

    /// <summary>
    /// Parent phone number (optional)
    /// </summary>
    public string? ParentPhoneNumber { get; set; }

    /// <summary>
    /// Parent email (optional)
    /// </summary>
    public string? ParentEmail { get; set; }

    /// <summary>
    /// Parent relationship (optional)
    /// </summary>
    public string? ParentRelationship { get; set; }

    /// <summary>
    /// Emergency contact name (optional)
    /// </summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone (optional)
    /// </summary>
    public string? EmergencyContactPhone { get; set; }
}

/// <summary>
/// Response DTO for returning student data to client
/// GET /api/v1/students/{id}
/// Contains only data needed by client, excludes sensitive info
/// </summary>
public class StudentResponse
{
    /// <summary>
    /// Student ID (database primary key)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique enrollment number
    /// </summary>
    public string EnrollmentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Student's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Student's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Student's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Student's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Student's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Student's age (calculated from DOB)
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Student's gender
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Student's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Department ID
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Department name
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Current semester
    /// </summary>
    public int CurrentSemester { get; set; }

    /// <summary>
    /// Current academic year
    /// </summary>
    public string CurrentAcademicYear { get; set; } = string.Empty;

    /// <summary>
    /// Admission year
    /// </summary>
    public int AdmissionYear { get; set; }

    /// <summary>
    /// Current GPA (0.0 to 4.0)
    /// </summary>
    public decimal CurrentGPA { get; set; }

    /// <summary>
    /// Total credit hours completed
    /// </summary>
    public int TotalCreditHoursCompleted { get; set; }

    /// <summary>
    /// Student status (Active, Inactive, OnLeave, Graduated, Suspended)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Is on academic probation?
    /// </summary>
    public bool IsOnAcademicProbation { get; set; }

    /// <summary>
    /// Expected graduation year
    /// </summary>
    public int? ExpectedGraduationYear { get; set; }

    /// <summary>
    /// Parent name
    /// </summary>
    public string? ParentName { get; set; }

    /// <summary>
    /// Parent email
    /// </summary>
    public string? ParentEmail { get; set; }

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
/// Response DTO for paginated student list
/// GET /api/v1/students?pageNumber=1&pageSize=10
/// </summary>
public class StudentListResponse
{
    /// <summary>
    /// List of student responses
    /// </summary>
    public List<StudentResponse> Items { get; set; } = [];

    /// <summary>
    /// Total count of students
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total pages available
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Has previous page?
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Has next page?
    /// </summary>
    public bool HasNextPage { get; set; }
}
