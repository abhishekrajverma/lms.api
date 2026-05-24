namespace EMS.Tests.Security;

using EMS.Infrastructure.Security;
using EMS.Tests.Common;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

/// <summary>
/// Unit tests for JwtTokenProvider
/// Tests JWT token generation and validation
/// </summary>
public class JwtTokenProviderTests
{
    private readonly JwtTokenProvider _tokenProvider;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenProviderTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-that-is-long-enough-for-testing-purposes-only",
            ExpirationMinutes = 60,
            Issuer = "test-issuer",
            Audience = "test-audience"
        };

        _tokenProvider = new JwtTokenProvider(_jwtSettings, TestFixtures.CreateMockLogger<JwtTokenProvider>());
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser();

        // Act
        var token = _tokenProvider.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT format has dots
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser();

        // Act
        var token = _tokenProvider.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        Assert.NotNull(jwtToken);
        Assert.Contains(jwtToken.Claims, c => c.Type == "sub" && c.Value == user.Id.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == "email" && c.Value == user.Email);
        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == user.Role);
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser();

        // Act
        var token = _tokenProvider.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        Assert.NotNull(jwtToken);
        var expirationMinutes = (jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes;
        Assert.InRange(expirationMinutes, _jwtSettings.ExpirationMinutes - 1, _jwtSettings.ExpirationMinutes + 1);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRandomToken()
    {
        // Act
        var token1 = _tokenProvider.GenerateRefreshToken();
        var token2 = _tokenProvider.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotEmpty(token1);
        Assert.NotEqual(token1, token2); // Should be different each time
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser();
        var token = _tokenProvider.GenerateAccessToken(user);

        // Act
        var principal = _tokenProvider.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.NotNull(principal.Identity);
        Assert.True(principal.Identity.IsAuthenticated);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _tokenProvider.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange - Create token that expired immediately
        var expiredSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            ExpirationMinutes = -1, // Already expired
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var expiredTokenProvider = new JwtTokenProvider(expiredSettings, 
            TestFixtures.CreateMockLogger<JwtTokenProvider>());
        var user = TestFixtures.CreateSampleUser();
        var token = expiredTokenProvider.GenerateAccessToken(user);

        // Act
        var principal = _tokenProvider.ValidateToken(token);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(id: 42);
        var token = _tokenProvider.GenerateAccessToken(user);

        // Act
        var userId = _tokenProvider.GetUserIdFromToken(token);

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(42, userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _tokenProvider.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void IsTokenExpired_WithValidToken_ShouldReturnFalse()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser();
        var token = _tokenProvider.GenerateAccessToken(user);

        // Act
        var isExpired = _tokenProvider.IsTokenExpired(token);

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsTokenExpired_WithInvalidToken_ShouldReturnTrue()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isExpired = _tokenProvider.IsTokenExpired(invalidToken);

        // Assert
        Assert.True(isExpired);
    }
}
