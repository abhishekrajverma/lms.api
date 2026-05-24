namespace EMS.Infrastructure.Security;

using EMS.Shared.Interfaces.Security;
using Microsoft.Extensions.Logging;

/// <summary>
/// Password hasher implementation using BCrypt
/// Provides secure password hashing and verification
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;
    private const int WorkFactor = 12; // BCrypt work factor (higher = slower = more secure)

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Hash a plain text password using BCrypt
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        try
        {
            // BCrypt automatically generates salt and includes it in the hash
            var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
            _logger.LogDebug("Password hashed successfully");
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    /// <summary>
    /// Verify plain text password against BCrypt hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            
            if (isValid)
                _logger.LogDebug("Password verified successfully");
            else
                _logger.LogWarning("Password verification failed");

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }
}
