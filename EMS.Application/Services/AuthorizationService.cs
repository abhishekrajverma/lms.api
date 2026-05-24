namespace EMS.Application.Services;

using EMS.Domain.Entities;
using EMS.Shared.Interfaces.Repositories;
using EMS.Shared.Interfaces.Security;
using EMS.Application.Interfaces.Services;
using EMS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Authorization service implementation
/// Handles role-based access control and resource-level authorization
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(IUnitOfWork unitOfWork, ILogger<AuthorizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> UserHasRoleAsync(int userId, string role, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if user {UserId} has role {Role}", userId, role);

        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            return user.Role.ToString() == role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user role");
            return false;
        }
    }

    public async Task<bool> UserHasAnyRoleAsync(int userId, params string[] roles)
    {
        _logger.LogDebug("Checking if user {UserId} has any of roles: {Roles}", 
            userId, string.Join(",", roles));

        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
                return false;

            return roles.Contains(user.Role.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user roles");
            return false;
        }
    }

    public async Task<bool> CanAccessResourceAsync(
        int userId, string resourceType, int resourceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if user {UserId} can access {ResourceType} {ResourceId}", 
            userId, resourceType, resourceId);

        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            // Admin can access everything
            if (user.Role.ToString() == "Admin")
                return true;

            // Role-specific access rules
            return resourceType switch
            {
                "Student" => CanAccessStudent(user, resourceId),
                "Grade" => CanAccessGrade(user, resourceId),
                "Attendance" => CanAccessAttendance(user, resourceId),
                "Course" => CanAccessCourse(user, resourceId),
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking resource access");
            return false;
        }
    }

    public async Task<bool> CanPerformActionAsync(
        int userId, string action, string resourceType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if user {UserId} can {Action} {ResourceType}", 
            userId, action, resourceType);

        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            // Admin can perform any action
            if (user.Role.ToString() == "Admin")
                return true;

            // Role-based action permissions
            return (user.Role.ToString(), action, resourceType) switch
            {
                // Students can view their own data
                ("Student", "View", "Student") => true,
                ("Student", "View", "Grade") => true,
                ("Student", "View", "Attendance") => true,
                ("Student", "View", "Course") => true,
                ("Student", "Enroll", "Course") => true,

                // Faculty can create and grade
                ("Faculty", "View", _) => true,
                ("Faculty", "Create", "Grade") => true,
                ("Faculty", "Update", "Grade") => true,
                ("Faculty", "Submit", "Grade") => true,
                ("Faculty", "Mark", "Attendance") => true,
                //("Faculty", "View", "Attendance") => true,

                // DepartmentHead
                ("DepartmentHead", "View", _) => true,
                ("DepartmentHead", "Create", "Course") => true,
                ("DepartmentHead", "Update", "Course") => true,

                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking action permission");
            return false;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting permissions for user {UserId}", userId);

        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return new List<string>();

            var role = user.Role.ToString();
            
            var permissions = role switch
            {
                "Admin" => GetAdminPermissions(),
                "Faculty" => GetFacultyPermissions(),
                "Student" => GetStudentPermissions(),
                "DepartmentHead" => GetDepartmentHeadPermissions(),
                _ => new List<string>()
            };

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting roles for user {UserId}", userId);

        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return new List<string>();

            return new List<string> { user.Role.ToString() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles");
            return new List<string>();
        }
    }

    // ============================================================================
    // Resource-level authorization checks
    // ============================================================================

    private bool CanAccessStudent(User user, int resourceId)
    {
        // Students can only view their own record
        if (user.Role.ToString() == "Student")
        {
            // TODO: Implement student access check
            return true; // Placeholder
        }

        return false;
    }

    private bool CanAccessGrade(User user, int resourceId)
    {
        // Faculty can view grades for their courses
        if (user.Role.ToString() == "Faculty")
        {
            // TODO: Check if grade belongs to faculty's course
            return true; // Placeholder
        }

        return false;
    }

    private bool CanAccessAttendance(User user, int resourceId)
    {
        // Faculty can view attendance for their courses
        if (user.Role.ToString() == "Faculty")
        {
            // TODO: Check if attendance is for faculty's course
            return true; // Placeholder
        }

        return false;
    }

    private bool CanAccessCourse(User user, int resourceId)
    {
        // Faculty can access their own courses
        if (user.Role.ToString() == "Faculty")
        {
            // TODO: Check if course is taught by faculty
            return true; // Placeholder
        }

        return false;
    }

    // ============================================================================
    // Permission sets by role
    // ============================================================================

    private List<string> GetAdminPermissions()
    {
        return new List<string>
        {
            "manage_users",
            "manage_courses",
            "manage_students",
            "manage_grades",
            "manage_attendance",
            "manage_departments",
            "view_reports",
            "system_settings",
            "user_management",
            "audit_logs"
        };
    }

    private List<string> GetFacultyPermissions()
    {
        return new List<string>
        {
            "view_students",
            "create_grades",
            "submit_grades",
            "view_grades",
            "mark_attendance",
            "view_attendance",
            "view_courses",
            "view_course_roster"
        };
    }

    private List<string> GetStudentPermissions()
    {
        return new List<string>
        {
            "view_own_profile",
            "view_own_grades",
            "view_own_attendance",
            "view_courses",
            "enroll_course",
            "withdraw_course",
            "change_password"
        };
    }

    private List<string> GetDepartmentHeadPermissions()
    {
        return new List<string>
        {
            "view_department_students",
            "manage_department_courses",
            "view_department_grades",
            "view_department_reports",
            "approve_grades"
        };
    }
}
