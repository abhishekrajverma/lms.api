namespace EMS.Tests.Services;

using AutoMapper;
using EMS.Application.DTOs.Auth;
using EMS.Application.Interfaces.Services;
using EMS.Application.Services;
using EMS.Domain.Entities;
using EMS.Infrastructure.Repositories;
using EMS.Shared.Exceptions;
using EMS.Tests.Common;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for AuthenticationService
/// Tests login, registration, password management, and token operations
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenProvider> _mockJwtTokenProvider;
    private readonly Mock<ILoginAttemptTracker> _mockLoginAttemptTracker;
    private readonly Mock<ITokenBlacklistService> _mockTokenBlacklist;
    private readonly IMapper _mapper;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _mockUnitOfWork = TestFixtures.CreateMockUnitOfWork();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenProvider = new Mock<IJwtTokenProvider>();
        _mockLoginAttemptTracker = new Mock<ILoginAttemptTracker>();
        _mockTokenBlacklist = new Mock<ITokenBlacklistService>();
        _mapper = TestFixtures.CreateMapper();

        _authService = new AuthenticationService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenProvider.Object,
            _mockLoginAttemptTracker.Object,
            _mockTokenBlacklist.Object,
            _mapper,
            TestFixtures.CreateMockLogger<AuthenticationService>()
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(email: "test@example.com");
        var password = "password123";
        var request = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = password
        };

        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        _mockLoginAttemptTracker
            .Setup(t => t.IsAccountLockedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher
            .Setup(p => p.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenProvider
            .Setup(j => j.GenerateAccessToken(user))
            .Returns("access_token");

        _mockJwtTokenProvider
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
        Assert.Equal("refresh_token", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.NotNull(result.User);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(email: "test@example.com");
        var request = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = "wrongpassword"
        };

        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        _mockLoginAttemptTracker
            .Setup(t => t.IsAccountLockedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithLockedAccount_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "test@example.com",
            Password = "password123"
        };

        _mockLoginAttemptTracker
            .Setup(t => t.IsAccountLockedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockLoginAttemptTracker
            .Setup(t => t.GetLockoutTimeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TimeSpan.FromMinutes(30));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnUserId()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!",
            Role = "Student"
        };

        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>()); // No existing users

        mockRepository
            .Setup(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        _mockPasswordHasher
            .Setup(p => p.HashPassword(request.Password))
            .Returns("hashed_password");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.Equal(1, result);
        mockRepository.Verify(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var existingUser = TestFixtures.CreateSampleUser(email: "test@example.com");
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!"
        };

        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { existingUser });

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidCurrentPassword_ShouldSucceed()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword123!"
        };

        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        _mockPasswordHasher
            .Setup(p => p.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            .Returns(true);

        _mockPasswordHasher
            .Setup(p => p.HashPassword(request.NewPassword))
            .Returns("new_hash");

        // Act
        var result = await _authService.ChangePasswordAsync(user.Id, request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var token = "valid_token";

        _mockTokenBlacklist
            .Setup(t => t.RevokeTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.LogoutAsync(token);

        // Assert
        Assert.True(result);
        _mockTokenBlacklist.Verify(t => t.RevokeTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }
}
