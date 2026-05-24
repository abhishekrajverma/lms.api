using Microsoft.AspNetCore.Http;

namespace EMS.Shared.Exceptions;

/// <summary>
/// Base custom exception class for the EMS application
/// All custom exceptions inherit from this
/// </summary>
public abstract class EmsException : Exception
{
    /// <summary>
    /// HTTP Status Code associated with this exception
    /// </summary>
    public int StatusCode { get; }

    protected EmsException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found
/// HTTP Status: 404 Not Found
/// 
/// Examples:
/// - Student with ID 999 doesn't exist
/// - Course code "CS101" not found
/// - Grade record for student X in course Y not found
/// - Class schedule not found
/// </summary>
public class NotFoundException : EmsException
{
    public NotFoundException(string message) 
        : base(message, StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string resourceName, string identifier) 
        : base($"{resourceName} with identifier '{identifier}' not found", StatusCodes.Status404NotFound)
    {
    }
}

/// <summary>
/// Exception thrown when request data is invalid or validation fails
/// HTTP Status: 400 Bad Request
/// 
/// Examples:
/// - Invalid email format for student registration
/// - Missing required fields (name, enrollment number, etc.)
/// - Invalid course code format
/// - GPA outside valid range
/// - Attendance percentage invalid
/// </summary>
public class BadRequestException : EmsException
{
    /// <summary>
    /// List of validation errors for the request
    /// </summary>
    public List<string>? Errors { get; }

    public BadRequestException(string message) 
        : base(message, StatusCodes.Status400BadRequest)
    {
    }

    public BadRequestException(string message, List<string>? errors) 
        : base(message, StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception thrown when user is not authenticated
/// HTTP Status: 401 Unauthorized
/// 
/// Examples:
/// - Student trying to access grades without logging in
/// - Faculty trying to submit grades without authentication
/// - Admin trying to access reports without valid session
/// - Token expired or invalid
/// </summary>
public class UnauthorizedException : EmsException
{
    public UnauthorizedException(string message) 
        : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}

/// <summary>
/// Exception thrown when user doesn't have permission to access a resource
/// HTTP Status: 403 Forbidden
/// 
/// Examples:
/// - Student trying to view other student's grades
/// - Faculty trying to modify grades they didn't submit
/// - Admin trying to access finance module without permission
/// - Student trying to enroll in a closed course
/// </summary>
public class ForbiddenException : EmsException
{
    public ForbiddenException(string message) 
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}

/// <summary>
/// Exception thrown when there's a conflict with existing data
/// HTTP Status: 409 Conflict
/// 
/// Examples:
/// - Student enrollment number already exists
/// - Course code already exists in the database
/// - Email address already registered
/// - Duplicate attendance record for same student and date
/// - Time slot conflict in class schedule
/// </summary>
public class ConflictException : EmsException
{
    public ConflictException(string message) 
        : base(message, StatusCodes.Status409Conflict)
    {
    }
}

/// <summary>
/// Exception thrown when an operation violates business rules
/// HTTP Status: 422 Unprocessable Entity
/// 
/// Examples:
/// - Student trying to enroll after semester closed
/// - Faculty trying to submit grades after deadline
/// - Trying to delete a course with enrolled students
/// - Trying to assign grade without prerequisites met
/// - Creating schedule with invalid time range
/// </summary>
public class UnprocessableEntityException : EmsException
{
    public UnprocessableEntityException(string message) 
        : base(message, StatusCodes.Status422UnprocessableEntity)
    {
    }
}

/// <summary>
/// Exception thrown when an internal server error occurs
/// HTTP Status: 500 Internal Server Error
/// 
/// Examples:
/// - Database connection failure
/// - Unexpected error in grade calculation
/// - File storage service unavailable
/// - AI service integration error
/// - Scheduler service failure
/// </summary>
public class InternalServerException : EmsException
{
    public InternalServerException(string message, Exception? innerException = null) 
        : base(message, StatusCodes.Status500InternalServerError)
    {
        // Log inner exception details for debugging
    }
}

/// <summary>
/// Exception thrown when external service is unavailable
/// HTTP Status: 503 Service Unavailable
/// 
/// Examples:
/// - AI/ML service for grade prediction not responding
/// - Email service unavailable
/// - Third-party integration (parent notification) failing
/// - Database server is temporarily down
/// - File storage service is down
/// </summary>
public class ServiceUnavailableException : EmsException
{
    public ServiceUnavailableException(string message) 
        : base(message, StatusCodes.Status503ServiceUnavailable)
    {
    }
}

/// <summary>
/// Exception thrown when operation times out
/// HTTP Status: 408 Request Timeout
/// 
/// Examples:
/// - Grade calculation takes too long
/// - Report generation exceeds timeout
/// - AI service response times out
/// - Database query exceeds timeout
/// </summary>
public class RequestTimeoutException : EmsException
{
    public RequestTimeoutException(string message) 
        : base(message, StatusCodes.Status408RequestTimeout)
    {
    }
}
