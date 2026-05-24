namespace EMS.Tests.Integration;

using EMS.Application.DTOs.Auth;
using EMS.Shared.Common;
using System.Net;
using Xunit;

/// <summary>
/// Integration tests for authentication flow
/// Tests complete login, registration, and token management workflows
/// </summary>
public class AuthenticationIntegrationTests : IntegrationTestBase
{
    public AuthenticationIntegrationTests(EmsTestFactory factory) : base(factory) { }

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "testuser@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!",
            Role = "Student"
        };

        // Act
        var response = await PostAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "existing@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!",
            Role = "Student"
        };

        // Register first user
        await PostAsync("/api/v1/auth/register", request);

        // Try to register with same email
        // Act
        var response = await PostAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        // First register a user
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!",
            Role = "Student"
        };

        await PostAsync("/api/v1/auth/register", registerRequest);

        // Then login
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await PostAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.Contains("accessToken", content);
        Assert.Contains("refreshToken", content);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "nonexistent@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await PostAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_ShouldReturnUserData()
    {
        // Arrange
        // Register and login
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!",
            Role = "Student"
        };

        await PostAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = "SecurePassword123!"
        };

        var loginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        var loginContent = System.Text.Json.JsonDocument.Parse(
            await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginContent.RootElement
            .GetProperty("data")
            .GetProperty("accessToken")
            .GetString();

        // Act
        var response = await GetAsync("/api/v1/auth/profile", accessToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("test@example.com", content);
    }

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_ShouldSucceed()
    {
        // Arrange
        // Register and login
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "OldPassword123!",
            Role = "Student"
        };

        await PostAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = "OldPassword123!"
        };

        var loginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        var loginContent = System.Text.Json.JsonDocument.Parse(
            await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginContent.RootElement
            .GetProperty("data")
            .GetProperty("accessToken")
            .GetString();

        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!"
        };

        // Act
        var response = await PostAsync("/api/v1/auth/change-password", changePasswordRequest, accessToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        // Register and login
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePassword123!",
            Role = "Student"
        };

        await PostAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = "SecurePassword123!"
        };

        var loginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        var loginContent = System.Text.Json.JsonDocument.Parse(
            await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginContent.RootElement
            .GetProperty("data")
            .GetProperty("accessToken")
            .GetString();

        // Act
        var response = await PostAsync("/api/v1/auth/logout", new { }, accessToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify token is blacklisted
        var profileResponse = await GetAsync("/api/v1/auth/profile", accessToken);
        Assert.Equal(HttpStatusCode.Unauthorized, profileResponse.StatusCode);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_ShouldFail()
    {
        // Act
        var response = await GetAsync("/api/v1/students");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithInvalidToken_ShouldFail()
    {
        // Act
        var response = await GetAsync("/api/v1/students", "invalid_token_here");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
