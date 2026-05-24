namespace EMS.Tests.Security;

using EMS.Infrastructure.Security;
using EMS.Tests.Common;
using Xunit;

/// <summary>
/// Unit tests for PasswordHasher
/// Tests BCrypt password hashing and verification
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher(TestFixtures.CreateMockLogger<PasswordHasher>());
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.StartsWith("$2a$", hash); // BCrypt hash format
    }

    [Fact]
    public void HashPassword_WithDifferentPasswords_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password1 = "SecurePassword123!";
        var password2 = "DifferentPassword456!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password1);
        var hash2 = _passwordHasher.HashPassword(password2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // BCrypt uses salt, so hashes differ
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash = _passwordHasher.HashPassword("password");

        // Act
        var result = _passwordHasher.VerifyPassword("", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "password";

        // Act
        var result = _passwordHasher.VerifyPassword(password, "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword(""));
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword(null));
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ShouldReturnFalse()
    {
        // Arrange
        var password = "password";
        var invalidHash = "not_a_valid_bcrypt_hash";

        // Act
        var result = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        Assert.False(result);
    }
}
