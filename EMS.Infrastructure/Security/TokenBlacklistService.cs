namespace EMS.Infrastructure.Security;

using EMS.Shared.Interfaces.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

/// <summary>
/// Token blacklist service implementation
/// Manages revoked tokens (used for logout)
/// Tokens are cached in Redis with expiration
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<TokenBlacklistService> _logger;
    private const string BlacklistKeyPrefix = "token_blacklist_";

    public TokenBlacklistService(IDistributedCache cache, ILogger<TokenBlacklistService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Add token to blacklist
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revoking token");

        try
        {
            var tokenHash = GenerateTokenHash(token);
            var expirationTime = GetTokenExpiration(token);

            if (expirationTime == null)
            {
                _logger.LogWarning("Could not determine token expiration");
                return false;
            }

            // Store in cache with expiration matching token expiration
            await _cache.SetStringAsync(
                BlacklistKeyPrefix + tokenHash,
                DateTime.UtcNow.ToString("O"),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expirationTime
                },
                cancellationToken);

            _logger.LogInformation("Token revoked successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    /// <summary>
    /// Check if token is in blacklist
    /// </summary>
    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHash = GenerateTokenHash(token);
            var value = await _cache.GetStringAsync(BlacklistKeyPrefix + tokenHash, cancellationToken);

            var isRevoked = !string.IsNullOrEmpty(value);
            
            if (isRevoked)
                _logger.LogWarning("Token is revoked");

            return isRevoked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if token is revoked");
            return false;
        }
    }

    /// <summary>
    /// Clean up expired tokens from blacklist
    /// Note: Redis automatically removes expired keys, so this is mainly for logging
    /// </summary>
    public async Task<bool> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cleaning up expired tokens from blacklist");
        // Redis handles expiration automatically
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Generate hash of token for storage (saves space)
    /// </summary>
    private static string GenerateTokenHash(string token)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// Extract expiration time from JWT token
    /// </summary>
    private static DateTime? GetTokenExpiration(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            return jwtToken?.ValidTo;
        }
        catch
        {
            return null;
        }
    }
}
