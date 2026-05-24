namespace EMS.Application.Validators.Auth;

using EMS.Application.DTOs.Auth;
using FluentValidation;

/// <summary>
/// Validator for RegisterRequest
/// </summary>
public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        // Username
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        // First Name
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        // Last Name
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        // Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*)");

        // Confirm Password
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        // Phone Number (optional)
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{10,}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be valid (at least 10 digits)");

        // Role
        RuleFor(x => x.Role)
            .Must(r => new[] { "Student", "Faculty", "Parent" }.Contains(r))
            .When(x => !string.IsNullOrEmpty(x.Role))
            .WithMessage("Role must be Student, Faculty, or Parent");
    }
}

/// <summary>
/// Validator for LoginRequest
/// </summary>
public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        // Username or Email
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required");

        // Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

/// <summary>
/// Validator for ChangePasswordRequest
/// </summary>
public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        // Current Password
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        // New Password
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one number")
            .Matches(@"[!@#$%^&*]").WithMessage("New password must contain at least one special character");

        // Confirm New Password
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");

        // New password must be different from current
        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword)
            .WithMessage("New password must be different from current password");
    }
}

/// <summary>
/// Validator for ForgotPasswordRequest
/// </summary>
public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");
    }
}

/// <summary>
/// Validator for ResetPasswordRequest
/// </summary>
public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        // Token
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");

        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        // New Password
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one number")
            .Matches(@"[!@#$%^&*]").WithMessage("New password must contain at least one special character");

        // Confirm New Password
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

/// <summary>
/// Validator for RefreshTokenRequest
/// </summary>
public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        // Refresh Token
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
