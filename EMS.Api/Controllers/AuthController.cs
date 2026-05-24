namespace EMS.Api.Controllers;

using EMS.Application.DTOs.Auth;
using EMS.Application.Interfaces.Services;
using EMS.Shared.Common;
using EMS.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Authentication API Controller
/// Handles user login, registration, password management, and token operations
/// Base route: /api/v1/auth
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    //private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        //IAuthorizationService authorizationService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        //_authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Login with credentials
    /// POST /api/v1/auth/login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for {UsernameOrEmail}", request.UsernameOrEmail);

        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message, 401));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return BadRequest(ApiResponse<object>.ErrorResponse("Login failed"));
        }
    }

    /// <summary>
    /// Register new user
    /// POST /api/v1/auth/register
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<int>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registration attempt for {Email}", request.Email);

        try
        {
            var userId = await _authService.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Login), new { },
                ApiResponse<int>.SuccessResponse(userId, "Registration successful. Please verify your email.", 201));
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Registration conflict: {Message}", ex.Message);
            return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, 409));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return BadRequest(ApiResponse<object>.ErrorResponse("Registration failed"));
        }
    }

    /// <summary>
    /// Get current user profile
    /// GET /api/v1/auth/profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting profile for current user");

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", 401));

            // TODO: Retrieve user profile from database
            return Ok(ApiResponse<UserProfileResponse>.SuccessResponse(
                new UserProfileResponse { Id = userId },
                "Profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error retrieving profile"));
        }
    }

    /// <summary>
    /// Change password
    /// POST /api/v1/auth/change-password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Change password request");

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", 401));

            var success = await _authService.ChangePasswordAsync(userId, request, cancellationToken);
            
            if (success)
                return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));

            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to change password"));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Change password failed: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message, 401));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error changing password"));
        }
    }

    /// <summary>
    /// Refresh access token
    /// POST /api/v1/auth/refresh-token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Token refresh requested");

        try
        {
            var response = await _authService.RefreshTokenAsync(request, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Token refreshed successfully"));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message, 401));
        }
        catch (NotImplementedException)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Token refresh not configured"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return BadRequest(ApiResponse<object>.ErrorResponse("Token refresh failed"));
        }
    }

    /// <summary>
    /// Logout (revoke token)
    /// POST /api/v1/auth/logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logout requested");

        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var success = await _authService.LogoutAsync(token, cancellationToken);
            
            if (success)
                return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully"));

            return BadRequest(ApiResponse<object>.ErrorResponse("Logout failed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return BadRequest(ApiResponse<object>.ErrorResponse("Logout failed"));
        }
    }

    /// <summary>
    /// Request password reset
    /// POST /api/v1/auth/forgot-password
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password reset requested for {Email}", request.Email);

        try
        {
            var success = await _authService.ForgotPasswordAsync(request, cancellationToken);
            
            if (success)
                return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset link sent to email"));

            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to send reset link"));
        }
        catch (NotImplementedException)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Password reset not configured"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset");
            return BadRequest(ApiResponse<object>.ErrorResponse("Password reset request failed"));
        }
    }

    /// <summary>
    /// Reset password with token
    /// POST /api/v1/auth/reset-password
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password reset for {Email}", request.Email);

        try
        {
            var success = await _authService.ResetPasswordAsync(request, cancellationToken);
            
            if (success)
                return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully"));

            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to reset password"));
        }
        catch (NotImplementedException)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Password reset not configured"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return BadRequest(ApiResponse<object>.ErrorResponse("Password reset failed"));
        }
    }

    /// <summary>
    /// Verify email with token
    /// POST /api/v1/auth/verify-email
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(
        [FromQuery] string email,
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email verification for {Email}", email);

        try
        {
            var success = await _authService.VerifyEmailAsync(email, token, cancellationToken);
            
            if (success)
                return Ok(ApiResponse<object>.SuccessResponse(null, "Email verified successfully"));

            return BadRequest(ApiResponse<object>.ErrorResponse("Email verification failed"));
        }
        catch (NotImplementedException)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email verification not configured"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return BadRequest(ApiResponse<object>.ErrorResponse("Email verification failed"));
        }
    }
}
