namespace EMS.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user registration
/// POST /api/v1/auth/register
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Username for login
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Password (will be hashed)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match Password)
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public string Role { get; set; } = "Student";
}

/// <summary>
/// Request DTO for user login
/// POST /api/v1/auth/login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username or email
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Password (plain text, will be hashed and compared)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Is remember me checked?
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Response DTO for successful login
/// POST /api/v1/auth/login (response)
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token (for obtaining new access token)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time (in seconds)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// User profile information
    /// </summary>
    public UserProfileResponse User { get; set; } = new();
}

/// <summary>
/// Response DTO for user profile
/// GET /api/v1/auth/profile
/// POST /api/v1/auth/login (User field)
/// </summary>
public class UserProfileResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Profile photo URL
    /// </summary>
    public string? ProfilePhotoUrl { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Account status
    /// </summary>
    public string AccountStatus { get; set; } = string.Empty;

    /// <summary>
    /// Is email verified?
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Is MFA enabled?
    /// </summary>
    public bool IsMfaEnabled { get; set; }

    /// <summary>
    /// Department ID (if faculty/staff)
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Department name (if faculty/staff)
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Employee ID (if staff)
    /// </summary>
    public string? EmployeeId { get; set; }
}

/// <summary>
/// Request DTO for changing password
/// POST /api/v1/auth/change-password
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password (must match NewPassword)
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for password reset
/// POST /api/v1/auth/forgot-password
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for reset password confirmation
/// POST /api/v1/auth/reset-password
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Reset token received via email
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for refresh token
/// POST /api/v1/auth/refresh-token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for general auth responses
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Success indicator
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Data (optional)
    /// </summary>
    public object? Data { get; set; }
}
