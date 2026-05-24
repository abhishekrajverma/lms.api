# ?? EMS API - Quick Reference Guide

**Quick access to commands, patterns, and common tasks**

---

## ?? Quick Start

### Setup & Run

```bash
# Clone repository
git clone <repo-url>
cd EMS.Backend

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API
dotnet run --project EMS.Api

# API available at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### Docker

```bash
# Build image
docker build -t ems-api:latest -f EMS.Api/Dockerfile .

# Run with Docker Compose
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f ems-api
```

---

## ?? Project Structure Quick Reference

```
EMS.Domain/              ? Business entities (User, Student, Course, etc.)
EMS.Application/         ? Services, validators, DTOs
EMS.Infrastructure/      ? Database, repositories, caching, logging
EMS.Api/                 ? Controllers, middleware, Program.cs
EMS.Shared/              ? Custom exceptions, response models
EMS.Tests/               ? Unit & integration tests

Key Patterns:
??? Service Pattern       ? I{Entity}Service / {Entity}Service
??? Repository Pattern    ? IGenericRepository<T> / GenericRepository<T>
??? Validator Pattern     ? {Action}{Entity}Validator
??? DTO Pattern          ? {Action}{Entity}Request / {Entity}Response
??? Middleware Pattern   ? {Purpose}Middleware
```

---

## ?? Creating a New Resource

### 1. Create Entity

```csharp
// Domain/Entities/NewEntity.cs
public class NewEntity : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Status { get; set; }
}
```

### 2. Create DTOs

```csharp
// Application/DTOs/NewEntity/NewEntityDTOs.cs
public class CreateNewEntityRequest
{
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
}

public class NewEntityResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 3. Create Validators

```csharp
// Application/Validators/NewEntity/CreateNewEntityValidator.cs
public class CreateNewEntityValidator : AbstractValidator<CreateNewEntityRequest>
{
    public CreateNewEntityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100);
    }
}
```

### 4. Add AutoMapper

```csharp
// Application/Mappers/MappingProfile.cs
CreateMap<NewEntity, NewEntityResponse>();
CreateMap<CreateNewEntityRequest, NewEntity>()
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
```

### 5. Create Service Interface

```csharp
// Application/Interfaces/Services/INewEntityService.cs
public interface INewEntityService
{
    Task<NewEntityResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<NewEntityResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateNewEntityRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateNewEntityRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

### 6. Implement Service

```csharp
// Application/Services/NewEntityService.cs
public class NewEntityService : INewEntityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateNewEntityRequest> _validator;
    
    public async Task<int> CreateAsync(CreateNewEntityRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var entity = _mapper.Map<NewEntity>(request);
        return await _unitOfWork.Repository<NewEntity>().InsertAsync(entity, cancellationToken);
    }
}
```

### 7. Create Controller

```csharp
// Api/Controllers/NewEntityController.cs
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NewEntityController : ControllerBase
{
    private readonly INewEntityService _service;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<NewEntityResponse>>> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<NewEntityResponse>.SuccessResponse(result));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateNewEntityRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, 
            ApiResponse<int>.SuccessResponse(id, "Created successfully", 201));
    }
}
```

### 8. Register in DI Container

```csharp
// Api/Extensions/ServiceCollectionExtensions.cs
services.AddScoped<INewEntityService, NewEntityService>();
services.AddTransient<IValidator<CreateNewEntityRequest>, CreateNewEntityValidator>();
```

---

## ?? Authentication & Authorization Quick Reference

### Login Flow

```bash
# 1. Register
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "ahmed",
  "email": "ahmed@example.com",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "password": "SecurePassword123!",
  "role": "Student"
}

# Response
201 Created
{ "success": true, "data": 1 }

# 2. Login
POST /api/v1/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "ahmed@example.com",
  "password": "SecurePassword123!"
}

# Response
200 OK
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "a7f3k9m2l8...",
    "expiresIn": 3600,
    "user": { "id": 1, "username": "ahmed", "email": "ahmed@example.com" }
  }
}

# 3. Access Protected Endpoint
GET /api/v1/students
Authorization: Bearer eyJhbGci...

# 4. Refresh Token
POST /api/v1/auth/refresh-token
Content-Type: application/json

{ "refreshToken": "a7f3k9m2l8..." }

# 5. Logout
POST /api/v1/auth/logout
Authorization: Bearer eyJhbGci...
```

### Authorization Patterns

```csharp
// Any authenticated user
[Authorize]
public async Task<IActionResult> GetProfile() { }

// Specific role
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser(int id) { }

// Multiple roles
[Authorize(Roles = "Faculty,Admin")]
public async Task<IActionResult> SubmitGrades() { }

// Custom policy
[Authorize(Policy = "StudentOnly")]
public async Task<IActionResult> GetMyGrades() { }

// No authentication required
[AllowAnonymous]
public async Task<IActionResult> Login(LoginRequest request) { }
```

---

## ?? API Endpoint Patterns

### Standard Endpoints

```
GET     /api/v1/students              ? Get all (paginated)
GET     /api/v1/students/{id}         ? Get one
POST    /api/v1/students              ? Create
PUT     /api/v1/students/{id}         ? Update
DELETE  /api/v1/students/{id}         ? Delete

Query Parameters:
?pageNumber=1&pageSize=10              ? Pagination
?searchTerm=ahmed                      ? Search
?departmentId=1                        ? Filter
?sortBy=lastName&sortOrder=asc         ? Sorting
```

### Standard Response Format

```json
// Success Response
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "statusCode": 200
}

// Error Response
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "statusCode": 400
}

// Paginated Response
{
  "success": true,
  "message": "Data retrieved",
  "data": {
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
  },
  "statusCode": 200
}
```

---

## ?? Testing Quick Reference

### Unit Test Template

```csharp
[Fact]
public async Task MethodName_WhenCondition_ShouldResult()
{
    // Arrange
    var mockRepository = new Mock<IRepository>();
    mockRepository.Setup(m => m.GetAsync(1))
        .ReturnsAsync(testData);
    
    var service = new MyService(mockRepository.Object);
    
    // Act
    var result = await service.GetAsync(1);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result);
}
```

### Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter "ClassName=AuthenticationServiceTests"

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Verbose output
dotnet test --logger "console;verbosity=detailed"

# Integration tests only
dotnet test --filter "Category=Integration"
```

### Test Data

```csharp
// Use TestFixtures for sample data
var user = TestFixtures.CreateSampleUser();
var students = TestFixtures.CreateSampleStudents(5);
var course = TestFixtures.CreateSampleCourse();
```

---

## ?? Database Queries

### Common Queries

```sql
-- Get student with GPA
SELECT s.*, AVG(g.FinalPercentageScore) as GPA
FROM Students s
LEFT JOIN Grades g ON s.Id = g.StudentId
WHERE s.Id = @StudentId
GROUP BY s.Id, s.EnrollmentNumber, s.FirstName, s.LastName, s.DepartmentId, s.Status

-- Get enrolled students
SELECT DISTINCT s.*
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
WHERE e.CourseId = @CourseId
ORDER BY s.LastName

-- Student attendance summary
SELECT COUNT(*) as TotalClasses,
       SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) as Present,
       ROUND(100.0 * SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) / COUNT(*), 2) as Percentage
FROM Attendance
WHERE StudentId = @StudentId AND CourseId = @CourseId
```

### Using Dapper

```csharp
using var connection = _factory.CreateConnection();

// Single row
var student = await connection.QueryFirstOrDefaultAsync<Student>(
    "SELECT * FROM Students WHERE Id = @Id",
    new { Id = studentId });

// Multiple rows
var students = await connection.QueryAsync<Student>(
    "SELECT * FROM Students WHERE DepartmentId = @DeptId ORDER BY LastName",
    new { DeptId = deptId });

// Execute command
var affected = await connection.ExecuteAsync(
    "UPDATE Students SET CurrentGPA = @GPA WHERE Id = @Id",
    new { GPA = gpa, Id = studentId });

// Stored procedure
var gpa = await connection.ExecuteScalarAsync<decimal>(
    "sp_CalculateStudentGPA",
    new { StudentId = studentId },
    commandType: CommandType.StoredProcedure);
```

---

## ?? Caching Patterns

### Cache Usage

```csharp
// Get from cache or database
var cacheKey = $"student:{id}";
var cached = await _cache.GetAsync<StudentResponse>(cacheKey);
if (cached != null) return cached;

var student = await _repository.GetByIdAsync(id);
var response = _mapper.Map<StudentResponse>(student);

// Cache for 1 hour
await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(1));

return response;
```

### Cache Invalidation

```csharp
// Remove cache when data changes
await _cache.RemoveAsync($"student:{id}");
await _cache.RemoveAsync("students:all");

// Or pattern removal
await _cache.RemoveByPatternAsync("student:*");
```

---

## ?? Debugging Tips

### Enable Debug Logging

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug",
      "EMS": "Debug"
    }
  }
}
```

### Common Errors & Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| `401 Unauthorized` | Missing/invalid token | Include `Authorization: Bearer {token}` header |
| `403 Forbidden` | Insufficient permissions | Check user role matches endpoint requirement |
| `404 Not Found` | Resource doesn't exist | Verify ID exists in database |
| `500 Internal Server Error` | Unhandled exception | Check application logs |
| `Connection timeout` | Database unreachable | Verify connection string, check DB running |

### Viewing Logs

```bash
# Docker logs
docker logs -f ems-api

# Local files
tail -f logs/ems-api-*.txt

# Database logs (Serilog table)
SELECT TOP 100 * FROM [dbo].[Logs] ORDER BY [TimeStamp] DESC
```

---

## ?? Deployment Quick Commands

### Build for Production

```bash
# Build release
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish

# Run published
dotnet ./publish/EMS.Api.dll
```

### Docker Deployment

```bash
# Build image
docker build -t ems-api:v1.0.0 .

# Tag for registry
docker tag ems-api:v1.0.0 myregistry.azurecr.io/ems-api:v1.0.0

# Push to registry
docker push myregistry.azurecr.io/ems-api:v1.0.0

# Run in production
docker run -d \
  --name ems-api \
  -p 80:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="..." \
  -e JwtSettings__SecretKey="..." \
  myregistry.azurecr.io/ems-api:v1.0.0
```

### GitHub Actions

```yaml
# Trigger workflow
git push origin main

# Monitor in Actions tab
# View logs for each step
# Check deployment status
```

---

## ?? Documentation Links

| Topic | Location |
|-------|----------|
| Complete Development Guide | `COMPLETE_DEVELOPMENT_GUIDE.md` |
| Testing Guide | `EMS.Tests/README-TESTING.md` |
| Deployment Guide | `README-DEPLOYMENT.md` |
| Step 5 Summary | `EMS.Api/STEP5_AUTHENTICATION_SUMMARY.md` |
| Steps 6-7 Summary | `STEPS_6_7_COMPLETION.md` |
| Quick Start | `EMS.Api/QUICKSTART.md` |
| Project Overview | `EMS.Api/PROJECT_COMPLETION_SUMMARY.md` |

---

## ?? Useful Resources

```
API Endpoints (Swagger):      http://localhost:5000/swagger
Health Check:                 http://localhost:5000/health
Logs Directory:               ./logs/
Test Results:                 ./TestResults/
Docker Compose:               docker-compose.yml
Environment Config:           .env.example
```

---

## ?? Tips & Tricks

### Performance

- Always use **pagination** for list endpoints
- **Cache** frequently accessed data
- Use **stored procedures** for complex calculations
- Add **indexes** on frequently filtered columns
- Use **async/await** everywhere

### Security

- Never commit `.env` files
- Use **environment variables** for secrets
- Validate **all inputs** with FluentValidation
- Implement **rate limiting** on public endpoints
- Use **HTTPS** in production
- Keep **JWT secret key** long and random

### Code Quality

- Write **unit tests** for business logic
- Use **dependency injection** for testability
- Follow **naming conventions**
- Keep methods **small and focused**
- Use **constants** for magic strings
- Document **public APIs** with XML comments

---

## ?? Support

For issues or questions:

1. Check `COMPLETE_DEVELOPMENT_GUIDE.md` troubleshooting section
2. Review relevant test files for usage examples
3. Check Docker/deployment guides
4. Review code comments and XML documentation
5. Check GitHub Issues if using version control

---

**Last Updated:** January 2024  
**Version:** 1.0.0
