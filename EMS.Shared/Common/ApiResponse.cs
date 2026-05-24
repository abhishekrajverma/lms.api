namespace EMS.Shared.Common;

/// <summary>
/// Generic API Response wrapper for all endpoints
/// Ensures consistent response format across the entire EMS application
/// 
/// Usage: All API endpoints must return ApiResponse<T> 
/// This standardizes error handling, success responses, and status codes
/// </summary>
/// <typeparam name="T">The data type being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the response
    /// Example: "Student created successfully" or "Course not found"
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual response data (student, course, list of grades, etc.)
    /// Will be null for error responses
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// HTTP Status Code (200, 201, 400, 404, 500, etc.)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Server timestamp when response was generated (UTC)
    /// Useful for auditing and logging
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// List of validation errors (if any)
    /// Used when validation fails (invalid email, missing required fields, etc.)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Creates a successful API response
    /// 
    /// Example:
    /// var response = ApiResponse<StudentDto>.SuccessResponse(studentData, "Student retrieved successfully");
    /// </summary>
    public static ApiResponse<T> SuccessResponse(
        T? data = default,
        string message = "Success",
        int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an error API response
    /// 
    /// Example:
    /// throw new NotFoundException("Student not found");
    /// // Caught by middleware and converted to:
    /// var response = ApiResponse<Student>.ErrorResponse("Student not found", 404);
    /// </summary>
    public static ApiResponse<T> ErrorResponse(
        string message,
        int statusCode = 400,
        T? data = default,
        List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Generic Paginated Response for returning large datasets
/// Used for: Students list, Courses list, Grades, Attendance records, etc.
/// 
/// Example Response:
/// {
///   "items": [ { student1 }, { student2 }, ... ],
///   "totalCount": 500,
///   "pageNumber": 1,
///   "pageSize": 10,
///   "totalPages": 50,
///   "hasPreviousPage": false,
///   "hasNextPage": true
/// }
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The actual list of items for this page
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// Total number of records in the database (not just this page)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based, starts at 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages available
    /// Calculated as: (TotalCount + PageSize - 1) / PageSize
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Can we go to previous page?
    /// Useful for pagination UI controls
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Can we go to next page?
    /// Useful for pagination UI controls
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
