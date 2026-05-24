namespace EMS.Tests.Services;

using EMS.Application.Interfaces.Services;
using EMS.Application.Services;
using EMS.Domain.Entities;
using EMS.Infrastructure.Repositories;
using EMS.Tests.Common;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for AuthorizationService
/// Tests role-based access control and permission checking
/// </summary>
public class AuthorizationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AuthorizationService _authService;

    public AuthorizationServiceTests()
    {
        _mockUnitOfWork = TestFixtures.CreateMockUnitOfWork();
        _authService = new AuthorizationService(
            _mockUnitOfWork.Object,
            TestFixtures.CreateMockLogger<AuthorizationService>()
        );
    }

    [Fact]
    public async Task UserHasRoleAsync_WithMatchingRole_ShouldReturnTrue()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Admin");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _authService.UserHasRoleAsync(user.Id, "Admin");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserHasRoleAsync_WithNonMatchingRole_ShouldReturnFalse()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Student");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _authService.UserHasRoleAsync(user.Id, "Admin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserHasAnyRoleAsync_WithMatchingRole_ShouldReturnTrue()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Faculty");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _authService.UserHasAnyRoleAsync(user.Id, "Admin", "Faculty");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserHasAnyRoleAsync_WithNoMatchingRoles_ShouldReturnFalse()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Student");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _authService.UserHasAnyRoleAsync(user.Id, "Admin", "Faculty");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnUserRoles()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Admin");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var roles = await _authService.GetUserRolesAsync(user.Id);

        // Assert
        Assert.NotNull(roles);
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ForStudentRole_ShouldReturnStudentPermissions()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Student");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var permissions = await _authService.GetUserPermissionsAsync(user.Id);

        // Assert
        Assert.NotNull(permissions);
        Assert.Contains("view_own_profile", permissions);
        Assert.Contains("view_own_grades", permissions);
        Assert.Contains("enroll_course", permissions);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ForAdminRole_ShouldReturnAdminPermissions()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Admin");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var permissions = await _authService.GetUserPermissionsAsync(user.Id);

        // Assert
        Assert.NotNull(permissions);
        Assert.Contains("manage_users", permissions);
        Assert.Contains("manage_courses", permissions);
        Assert.Contains("system_settings", permissions);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ForNonExistentUser_ShouldReturnEmptyList()
    {
        // Arrange
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var permissions = await _authService.GetUserPermissionsAsync(999);

        // Assert
        Assert.NotNull(permissions);
        Assert.Empty(permissions);
    }

    [Fact]
    public async Task CanPerformActionAsync_WithValidPermission_ShouldReturnTrue()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Faculty");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _authService.CanPerformActionAsync(user.Id, "Create", "Grade");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanPerformActionAsync_WithoutPermission_ShouldReturnFalse()
    {
        // Arrange
        var user = TestFixtures.CreateSampleUser(role: "Student");
        var mockRepository = new Mock<IGenericRepository<User>>();
        mockRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.Repository<User>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _authService.CanPerformActionAsync(user.Id, "Create", "Grade");

        // Assert
        Assert.False(result);
    }
}
