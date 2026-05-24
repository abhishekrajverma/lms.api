# EMS Testing Guide

## Overview

This guide covers unit testing, integration testing, and end-to-end testing for the EMS API.

## Unit Testing

### Running Unit Tests

```bash
# Run all unit tests
dotnet test EMS.Tests/EMS.Tests.csproj

# Run specific test class
dotnet test EMS.Tests/EMS.Tests.csproj --filter "ClassName=AuthenticationServiceTests"

# Run tests with coverage
dotnet test EMS.Tests/EMS.Tests.csproj /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run tests with verbose output
dotnet test EMS.Tests/EMS.Tests.csproj --logger "console;verbosity=detailed"
```

### Unit Test Categories

#### Authentication Tests (`AuthenticationServiceTests.cs`)
- ? Login with valid credentials
- ? Login with invalid password
- ? Account lockout after failed attempts
- ? User registration
- ? Duplicate email prevention
- ? Password change
- ? Logout token revocation

#### Password Security Tests (`PasswordHasherTests.cs`)
- ? Password hashing
- ? Password verification
- ? Hash uniqueness (BCrypt salt)
- ? Invalid password detection
- ? Empty password handling

#### JWT Token Tests (`JwtTokenProviderTests.cs`)
- ? Access token generation
- ? Token claim validation
- ? Token expiration
- ? Refresh token generation
- ? Token validation
- ? Expired token rejection
- ? User ID extraction from token

#### Authorization Tests (`AuthorizationServiceTests.cs`)
- ? Role-based access check
- ? Multiple role validation
- ? Permission sets per role
- ? Action authorization

### Test Structure

```csharp
[Fact]
public async Task TestName_WhenCondition_ShouldResult()
{
    // Arrange - Setup test data
    var user = TestFixtures.CreateSampleUser();
    
    // Act - Execute action
    var result = await service.DoSomething(user);
    
    // Assert - Verify results
    Assert.NotNull(result);
}
```

## Integration Testing

### Running Integration Tests

```bash
# Run integration tests
dotnet test EMS.Tests/EMS.Tests.csproj --filter "Category=Integration"

# Run specific integration test
dotnet test EMS.Tests/EMS.Tests.csproj --filter "ClassName=AuthenticationIntegrationTests"
```

### Integration Test Scenarios

#### Authentication Flow
1. User registration
2. User login
3. Token generation
4. Profile access with token
5. Password change
6. Logout and token revocation
7. Access denied without token

#### Student Workflow
1. Admin creates student
2. Student views their profile
3. Student enrolls in course
4. Faculty marks attendance
5. Faculty submits grades
6. Student views grades

#### Grade Submission Flow
1. Create grade record
2. Verify GPA calculation
3. Submit for approval
4. Generate transcript
5. Dispute grade (if needed)

### Test Database Setup

Integration tests use:
- **SQL Server LocalDB** for data persistence
- **In-memory Redis** for caching tests
- **TestServer** from ASP.NET Core

```csharp
public class IntegrationTestBase : IClassFixture<EmsTestFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly EmsTestFactory Factory;

    public IntegrationTestBase(EmsTestFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }
}
```

## Test Coverage

### Target Coverage
- **Overall:** >80%
- **Controllers:** >75%
- **Services:** >85%
- **Repositories:** >80%
- **Security:** >90%

### Generate Coverage Report

```bash
# Install reportgenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Generate HTML report
reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"coverage-report" -reporttypes:"HtmlInline"
```

## Mocking Best Practices

### Using Moq

```csharp
var mockRepository = new Mock<IGenericRepository<User>>();

// Setup method
mockRepository
    .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);

// Verify method was called
mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
```

### Test Fixtures

Use `TestFixtures` for common test data:

```csharp
var user = TestFixtures.CreateSampleUser();
var course = TestFixtures.CreateSampleCourse();
var students = TestFixtures.CreateSampleStudents(5);
```

## Continuous Integration

### GitHub Actions

Tests run automatically on:
- Push to main/develop
- Pull requests
- Manual workflow dispatch

### Build & Test Pipeline

```yaml
# See .github/workflows/build-test.yml
- Run unit tests
- Collect code coverage
- Upload coverage to CodeCov
- Generate coverage badge
```

## Local Testing Checklist

- [ ] Run all unit tests: `dotnet test`
- [ ] Run integration tests: `dotnet test --filter "Category=Integration"`
- [ ] Check code coverage: >80%
- [ ] Run SonarCloud analysis
- [ ] Verify security tests pass
- [ ] Check API documentation

## Troubleshooting

### Tests Fail with Database Connection
```bash
# Ensure SQL Server is running
dotnet run --project EMS.Api

# Or use Docker Compose
docker-compose up sqlserver
```

### Redis Connection Fails
```bash
# Start Redis
docker-compose up redis

# Or use local Redis
redis-server
```

### Timeout Issues
```bash
# Increase test timeout in .runsettings
<TestRunConfiguration>
  <TestTimeouts>
    <Individual>30000</Individual>
  </TestTimeouts>
</TestRunConfiguration>
```

## Best Practices

1. **Isolate Tests** - Each test should be independent
2. **Use Fixtures** - Reuse common test data
3. **Clear Names** - Test names describe what they test
4. **Arrange-Act-Assert** - Follow AAA pattern
5. **Mock External Services** - Don't call real APIs
6. **Test Edge Cases** - Null, empty, invalid inputs
7. **Don't Skip Tests** - Remove Ignore attribute
8. **Fast Tests** - Unit tests should be <100ms
9. **Deterministic** - Same input always same output
10. **Maintainable** - Easy to update tests

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/Moq/moq4)
- [Microsoft Testing Guide](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Arrange-Act-Assert Pattern](https://xp123.com/articles/3a-arrange-act-assert/)
