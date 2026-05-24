namespace EMS.Shared.Interfaces.Security;

/// <summary>
/// Password hasher interface
/// Implements secure password hashing
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a plain text password
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify plain text password against hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Password hash to verify against</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// JWT token provider interface
/// Generates and validates JWT tokens
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    string GenerateAccessToken(EMS.Domain.Entities.User user);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <returns>Token claims if valid, null if invalid</returns>
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    int? GetUserIdFromToken(string token);

    /// <summary>
    /// Check if token is expired
    /// </summary>
    bool IsTokenExpired(string token);
}

/// <summary>
/// Token blacklist service interface
/// Manages revoked tokens (logout)
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Add token to blacklist
    /// </summary>
    Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if token is blacklisted
    /// </summary>
    Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear expired tokens from blacklist
    /// </summary>
    Task<bool> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Login attempt tracker interface
/// Prevents brute force attacks
/// </summary>
public interface ILoginAttemptTracker
{
    /// <summary>
    /// Record failed login attempt
    /// </summary>
    Task<bool> RecordFailedAttemptAsync(string usernameOrEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear failed attempts after successful login
    /// </summary>
    Task<bool> ClearAttemptsAsync(string usernameOrEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user account is locked
    /// </summary>
    Task<bool> IsAccountLockedAsync(string usernameOrEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining attempts before lockout
    /// </summary>
    Task<int> GetRemainingAttemptsAsync(string usernameOrEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get time until account unlock
    /// </summary>
    Task<TimeSpan?> GetLockoutTimeAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
}
