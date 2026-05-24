# ?? EMS (Educational Management System) - Complete Development Guide

**Version:** 1.0.0  
**Last Updated:** January 2024  
**Status:** Production Ready ?

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture & Design](#architecture--design)
3. [Project Structure](#project-structure)
4. [Core Components](#core-components)
5. [Building the API](#building-the-api)
6. [Step-by-Step Implementation](#step-by-step-implementation)
7. [Security Implementation](#security-implementation)
8. [Testing Strategy](#testing-strategy)
9. [Docker & Deployment](#docker--deployment)
10. [Performance Optimization](#performance-optimization)
11. [Best Practices](#best-practices)
12. [Troubleshooting](#troubleshooting)

---

## Project Overview

### What is EMS?

The **Educational Management System (EMS)** is a comprehensive REST API built with **.NET 8** that manages student enrollment, course management, grade tracking, and attendance monitoring for educational institutions.

### Key Features

? **Complete CRUD Operations** for Students, Courses, Grades, Attendance  
? **JWT-based Authentication** with BCrypt password hashing  
? **Role-Based Authorization** (Admin, Faculty, Student, DepartmentHead)  
? **High-Performance Data Access** using Dapper ORM  
? **Redis Caching** for improved response times  
? **Comprehensive Error Handling** with custom exceptions  
? **Structured Logging** using Serilog  
? **API Documentation** with Swagger/OpenAPI  
? **Unit & Integration Testing** with xUnit  
? **Docker Containerization** for easy deployment  
? **CI/CD Pipelines** with GitHub Actions  
? **Automated Health Checks** and monitoring  

### Technology Stack

```
Runtime:        .NET 8
Language:       C#
Database:       SQL Server
ORM:            Dapper
Cache:          Redis
Authentication: JWT + BCrypt
Logging:        Serilog
Testing:        xUnit + Moq
Documentation:  Swagger/OpenAPI
Containers:     Docker
CI/CD:          GitHub Actions
Cloud:          Azure Container Instances
```

---

## Architecture & Design

### Layered Architecture Pattern

The EMS API follows a **Clean Architecture** approach with clear separation of concerns:

```
???????????????????????????????????????????????????????
?              PRESENTATION LAYER                      ?
?         (Controllers, Models, Views)                 ?
?  Controllers ? Middleware ? Extensions ? Program    ?
???????????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????????
?           APPLICATION LAYER                          ?
?    (Business Logic, Validation, Mapping)             ?
?  Services ? Validators ? DTOs ? Mappers ? Interfaces?
???????????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????????
?         INFRASTRUCTURE LAYER                         ?
?      (Data Access, Caching, Logging)                 ?
?  Repositories ? Cache ? Logging ? Database ? Security?
???????????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????????
?           DOMAIN LAYER                               ?
?         (Core Business Entities)                     ?
?   Entities ? Enums ? Interfaces ? Base Classes       ?
???????????????????????????????????????????????????????
```

### Design Principles Applied

| Principle | Implementation |
|-----------|-----------------|
| **Single Responsibility** | Each service handles one entity type |
| **Open/Closed Principle** | Extension through interfaces, not modification |
| **Liskov Substitution** | Repositories implement common IGenericRepository |
| **Interface Segregation** | Specific interfaces for different services |
| **Dependency Inversion** | Inject interfaces, not concrete implementations |
| **DRY (Don't Repeat Yourself)** | GenericRepository, TestFixtures, base classes |
| **SOLID Principles** | Followed throughout the codebase |

---

## Project Structure

### Directory Organization

```
EMS.Backend/
??? EMS.Domain/                          # Core business entities
?   ??? Entities/
?   ?   ??? BaseEntity.cs               # Common properties
?   ?   ??? User.cs                     # Authentication & profiles
?   ?   ??? Student.cs                  # Student records
?   ?   ??? Course.cs                   # Course catalog
?   ?   ??? Grade.cs                    # Grade records
?   ?   ??? Attendance.cs               # Attendance tracking
?   ?   ??? Department.cs               # Department management
?   ??? Enums/
?       ??? EntityEnums.cs              # User roles, statuses, etc.
?
??? EMS.Application/                    # Business logic layer
?   ??? DTOs/                           # Data Transfer Objects
?   ?   ??? Student/StudentDTOs.cs
?   ?   ??? Course/CourseDTOs.cs
?   ?   ??? Grade/GradeDTOs.cs
?   ?   ??? Attendance/AttendanceDTOs.cs
?   ?   ??? Auth/AuthDTOs.cs
?   ??? Validators/                     # FluentValidation
?   ?   ??? Student/StudentValidators.cs
?   ?   ??? Course/CourseValidators.cs
?   ?   ??? Grade/GradeValidators.cs
?   ?   ??? Attendance/AttendanceValidators.cs
?   ?   ??? Auth/AuthValidators.cs
?   ??? Services/                       # Business logic
?   ?   ??? StudentService.cs
?   ?   ??? CourseService.cs
?   ?   ??? GradeService.cs
?   ?   ??? AttendanceService.cs
?   ?   ??? AuthenticationService.cs
?   ?   ??? AuthorizationService.cs
?   ??? Interfaces/
?   ?   ??? Services/
?   ?   ?   ??? EducationServiceInterfaces.cs
?   ?   ?   ??? AuthenticationInterfaces.cs
?   ?   ??? Repositories/
?   ?       ??? IRepositories.cs
?   ??? Mappers/
?       ??? MappingProfile.cs
?
??? EMS.Infrastructure/                 # Data access & external services
?   ??? Repositories/
?   ?   ??? GenericRepository.cs        # Base CRUD operations
?   ?   ??? StoredProcedureRepository.cs
?   ?   ??? UnitOfWork.cs               # Transaction management
?   ?   ??? DbConnectionFactory.cs
?   ??? Database/
?   ?   ??? 001_CreateTables.sql
?   ?   ??? 002_CreateIndexes.sql
?   ?   ??? 003-006_StoredProcedures.sql
?   ?   ??? 007_RefreshTokensTable.sql
?   ?   ??? 008_TokenBlacklistTable.sql
?   ??? Caching/
?   ?   ??? RedisCacheService.cs
?   ??? Logging/
?   ?   ??? LoggingConfiguration.cs
?   ??? Security/
?   ?   ??? PasswordHasher.cs
?   ?   ??? JwtTokenProvider.cs
?   ?   ??? LoginAttemptTracker.cs
?   ?   ??? TokenBlacklistService.cs
?   ??? Extensions/
?       ??? ServiceExtensions.cs
?
??? EMS.Api/                            # Presentation layer
?   ??? Controllers/
?   ?   ??? StudentController.cs
?   ?   ??? CourseController.cs
?   ?   ??? GradeController.cs
?   ?   ??? AttendanceController.cs
?   ?   ??? AuthController.cs
?   ??? Middleware/
?   ?   ??? ExceptionHandlingMiddleware.cs
?   ?   ??? PerformanceMonitoringMiddleware.cs
?   ?   ??? SecurityHeadersMiddleware.cs
?   ??? Extensions/
?   ?   ??? ServiceCollectionExtensions.cs
?   ?   ??? AuthenticationExtensions.cs
?   ??? Program.cs                      # Startup configuration
?   ??? appsettings.json                # Configuration
?   ??? Dockerfile                      # Container image
?   ??? Readme.md                       # API documentation
?
??? EMS.Shared/                         # Shared utilities
?   ??? Common/
?   ?   ??? ApiResponse.cs              # Standard API response
?   ?   ??? PaginationModel.cs
?   ??? Exceptions/
?   ?   ??? CustomExceptions.cs
?   ??? Constants/
?       ??? ApplicationConstants.cs
?
??? EMS.Tests/                          # Testing layer
?   ??? Common/
?   ?   ??? TestFixtures.cs
?   ??? Services/
?   ?   ??? AuthenticationServiceTests.cs
?   ?   ??? AuthorizationServiceTests.cs
?   ??? Security/
?   ?   ??? PasswordHasherTests.cs
?   ?   ??? JwtTokenProviderTests.cs
?   ??? Integration/
?   ?   ??? IntegrationTestBase.cs
?   ?   ??? AuthenticationIntegrationTests.cs
?   ??? README-TESTING.md
?   ??? appsettings.Test.json
?
??? .github/workflows/                  # CI/CD pipelines
?   ??? build-test.yml
?   ??? docker-build.yml
?   ??? deploy-staging.yml
?   ??? deploy-prod.yml
?   ??? code-quality.yml
?
??? docker-compose.yml                  # Local development
??? .env.example                        # Environment template
??? tests.runsettings                   # Test configuration
??? README-DEPLOYMENT.md                # Deployment guide
??? STEPS_6_7_COMPLETION.md             # Implementation summary
```

---

## Core Components

### 1. Domain Layer (EMS.Domain)

**Purpose:** Define core business entities and enums

#### BaseEntity.cs
```csharp
// Common properties for all entities
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

#### Key Entities
- **User** - Authentication and user profiles
- **Student** - Student records with enrollment details
- **Course** - Course catalog and metadata
- **Grade** - Grade records and GPA calculation
- **Attendance** - Attendance tracking per student-course
- **Department** - Department organization

### 2. Application Layer (EMS.Application)

**Purpose:** Implement business logic and validation

#### Service Pattern
```csharp
public interface IStudentService
{
    Task<StudentResponse> GetStudentByIdAsync(int id);
    Task<PaginatedResponse<StudentResponse>> GetAllStudentsAsync(int page, int size);
    Task<int> CreateStudentAsync(CreateStudentRequest request);
    Task<bool> UpdateStudentAsync(int id, UpdateStudentRequest request);
    Task<bool> DeleteStudentAsync(int id);
}

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateStudentRequest> _validator;
    
    // Implementation with business logic
}
```

#### Validation Pattern
```csharp
public class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.EnrollmentNumber)
            .NotEmpty().WithMessage("Enrollment number is required")
            .Length(5, 10).WithMessage("Must be 5-10 characters");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100);
    }
}
```

#### Mapping Pattern
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentResponse>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        
        CreateMap<CreateStudentRequest, Student>()
            .ForMember(dest => dest.CreatedAt, 
                opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

### 3. Infrastructure Layer (EMS.Infrastructure)

**Purpose:** Data access, caching, and external services

#### GenericRepository Pattern
```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    // CRUD operations
    public async Task<T> GetByIdAsync(int id);
    public async Task<List<T>> GetAllAsync();
    public async Task<int> InsertAsync(T entity);
    public async Task<int> UpdateAsync(T entity);
    public async Task<int> DeleteAsync(int id);
}
```

#### UnitOfWork Pattern
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly DbConnectionFactory _connectionFactory;
    private IDbTransaction _transaction;
    
    public async Task<int> SaveChangesAsync()
    {
        _transaction?.Commit();
        return 1;
    }
    
    public async Task RollbackAsync()
    {
        _transaction?.Rollback();
    }
}
```

#### Caching Pattern
```csharp
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _redis.GetDatabase().StringSetAsync(key, json, expiry);
    }
    
    public async Task<T> GetAsync<T>(string key)
    {
        var value = await _redis.GetDatabase().StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString());
    }
}
```

### 4. API Layer (EMS.Api)

**Purpose:** HTTP endpoints and request handling

#### Controller Pattern
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // Require authentication
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetStudentById(int id)
    {
        try
        {
            var student = await _service.GetStudentByIdAsync(id);
            return Ok(ApiResponse<StudentResponse>.SuccessResponse(student));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
        }
    }
}
```

#### Middleware Pattern
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        context.Response.StatusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        
        return context.Response.WriteAsJsonAsync(
            ApiResponse<object>.ErrorResponse(exception.Message));
    }
}
```

---

## Building the API

### Step-by-Step Process

#### **Step 1: Project Setup**

```bash
# Create solution
dotnet new sln -n EMS.Backend

# Create projects
dotnet new classlib -n EMS.Domain
dotnet new classlib -n EMS.Application
dotnet new classlib -n EMS.Infrastructure
dotnet new classlib -n EMS.Shared
dotnet new webapi -n EMS.Api
dotnet new xunit -n EMS.Tests

# Add to solution
dotnet sln EMS.Backend.sln add **/*.csproj
```

#### **Step 2: Add NuGet Dependencies**

```bash
# Core packages
dotnet add EMS.Application package AutoMapper
dotnet add EMS.Application package FluentValidation

# Database
dotnet add EMS.Infrastructure package Dapper
dotnet add EMS.Infrastructure package System.Data.SqlClient

# Caching
dotnet add EMS.Infrastructure package StackExchange.Redis

# Logging
dotnet add EMS.Api package Serilog.AspNetCore

# Security
dotnet add EMS.Infrastructure package BCrypt.Net-Core
dotnet add EMS.Api package System.IdentityModel.Tokens.Jwt

# Testing
dotnet add EMS.Tests package xunit
dotnet add EMS.Tests package Moq
dotnet add EMS.Tests package FluentAssertions

# API Documentation
dotnet add EMS.Api package Swashbuckle.AspNetCore
```

#### **Step 3: Define Domain Models**

```csharp
// Domain/Entities/Student.cs
public class Student : BaseEntity
{
    public string EnrollmentNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int DepartmentId { get; set; }
    public decimal CurrentGPA { get; set; }
    public string Status { get; set; } // Active, Probation, Suspended
}
```

#### **Step 4: Create DTOs for API Communication**

```csharp
// Application/DTOs/Student/StudentDTOs.cs
public class CreateStudentRequest
{
    [Required]
    public string EnrollmentNumber { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }
}

public class StudentResponse
{
    public int Id { get; set; }
    public string EnrollmentNumber { get; set; }
    public string FullName { get; set; }
    public decimal CurrentGPA { get; set; }
}
```

#### **Step 5: Implement Validators**

```csharp
// Application/Validators/Student/CreateStudentValidator.cs
public class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.EnrollmentNumber)
            .NotEmpty()
            .Length(5, 10);
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

#### **Step 6: Create AutoMapper Profiles**

```csharp
// Application/Mappers/MappingProfile.cs
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentResponse>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        
        CreateMap<CreateStudentRequest, Student>()
            .ForMember(dest => dest.CreatedAt, 
                opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

#### **Step 7: Build Service Interfaces**

```csharp
// Application/Interfaces/Services/IStudentService.cs
public interface IStudentService
{
    Task<StudentResponse> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<StudentResponse>> GetAllStudentsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateStudentAsync(int id, UpdateStudentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteStudentAsync(int id, CancellationToken cancellationToken = default);
}
```

#### **Step 8: Implement Services**

```csharp
// Application/Services/StudentService.cs
public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateStudentRequest> _createValidator;
    private readonly ICacheService _cache;
    private readonly ILogger<StudentService> _logger;
    
    public async Task<StudentResponse> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Try cache first
        var cacheKey = $"student:{id}";
        var cached = await _cache.GetAsync<StudentResponse>(cacheKey);
        if (cached != null)
            return cached;
        
        // Get from database
        var student = await _unitOfWork.Repository<Student>()
            .GetByIdAsync(id, cancellationToken);
        
        if (student == null)
            throw new NotFoundException(nameof(Student), id.ToString());
        
        var response = _mapper.Map<StudentResponse>(student);
        
        // Cache result
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(1));
        
        return response;
    }
    
    public async Task<int> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        // Validate
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        // Map to entity
        var student = _mapper.Map<Student>(request);
        
        // Save to database
        var id = await _unitOfWork.Repository<Student>()
            .InsertAsync(student, cancellationToken);
        
        // Invalidate cache
        await _cache.RemoveAsync("students:all");
        
        return id;
    }
}
```

#### **Step 9: Create Controllers**

```csharp
// Api/Controllers/StudentsController.cs
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetStudentById(int id)
    {
        _logger.LogInformation("Getting student with ID: {StudentId}", id);
        
        try
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            return Ok(ApiResponse<StudentResponse>.SuccessResponse(student, "Student retrieved"));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Student not found: {StudentId}", id);
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<ActionResult<ApiResponse<int>>> CreateStudent([FromBody] CreateStudentRequest request)
    {
        _logger.LogInformation("Creating new student: {Email}", request.Email);
        
        try
        {
            var id = await _studentService.CreateStudentAsync(request);
            return CreatedAtAction(nameof(GetStudentById), new { id }, 
                ApiResponse<int>.SuccessResponse(id, "Student created", 201));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error creating student"));
        }
    }
}
```

#### **Step 10: Configure Dependency Injection**

```csharp
// Api/Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        
        // Validators
        services.AddValidatorsFromAssembly(typeof(CreateStudentValidator).Assembly);
        
        // Mapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        
        return services;
    }
    
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddScoped<DbConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddScoped<ICacheService, RedisCacheService>();
        
        // Logging
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        services.AddLogging(config => config.AddSerilog(logger));
        
        return services;
    }
}
```

#### **Step 11: Configure Program.cs**

```csharp
// Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<PerformanceMonitoringMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
```

#### **Step 12: Create Database Schema**

```sql
-- Infrastructure/Database/001_CreateTables.sql
CREATE TABLE [dbo].[Users] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL,
    [AccountStatus] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [IsEmailVerified] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [dbo].[Students] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EnrollmentNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [DateOfBirth] DATETIME2 NOT NULL,
    [DepartmentId] INT NOT NULL,
    [CurrentGPA] DECIMAL(3,2) NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id])
);
```

---

## Step-by-Step Implementation

### **Phase 1: Foundation (Steps 1-3)**

| Step | Focus | Output | Files |
|------|-------|--------|-------|
| 1 | Domain Layer | Entities, Enums | 7 |
| 2 | Application Layer | Services, DTOs, Validators | 15 |
| 3 | AutoMapper | Entity-DTO Mapping | 1 |

### **Phase 2: Infrastructure (Step 4)**

| Component | Pattern | Files |
|-----------|---------|-------|
| Repositories | Generic + Specific | 4 |
| Database | SQL Scripts | 6 |
| Caching | Redis | 1 |
| Logging | Serilog | 1 |

### **Phase 3: API & Security (Step 5)**

| Feature | Implementation | Files |
|---------|-----------------|-------|
| Controllers | 5 endpoints per resource | 5 |
| Middleware | Exception, Performance, Security | 3 |
| Authentication | JWT + BCrypt | 8 |
| Authorization | RBAC | 2 |

### **Phase 4: Testing & Deployment (Steps 6-7)**

| Category | Type | Files |
|----------|------|-------|
| Testing | Unit + Integration | 9 |
| Docker | Multi-stage build | 3 |
| CI/CD | GitHub Actions | 5 |

---

## Security Implementation

### Authentication Flow

```
1. User Registration
   POST /api/v1/auth/register
   ?? Validate input (FluentValidation)
   ?? Hash password (BCrypt)
   ?? Create user in database
   ?? Return user ID

2. User Login
   POST /api/v1/auth/login
   ?? Check account locked (Redis)
   ?? Find user by email/username
   ?? Verify password (BCrypt.Verify)
   ?? Check account status
   ?? Update last login
   ?? Generate JWT token
   ?? Generate refresh token
   ?? Return tokens + user info

3. Access Protected Endpoint
   GET /api/v1/students
   Authorization: Bearer {accessToken}
   ?? Validate JWT signature
   ?? Check token expiration
   ?? Extract user claims
   ?? Check authorization ([Authorize(Roles=...)])
   ?? Execute endpoint logic

4. Refresh Token
   POST /api/v1/auth/refresh-token
   ?? Validate refresh token
   ?? Generate new access token
   ?? Return new token

5. Logout
   POST /api/v1/auth/logout
   ?? Add token to blacklist (Redis)
   ?? Token immediately invalid
```

### Password Security

```csharp
// BCrypt Hashing
var plainPassword = "MyPassword123!";
var hash = BCrypt.HashPassword(plainPassword, workFactor: 12);
// Result: $2a$12$K7vF8Xz9L2mP0v3nXq1oZeR8d9m2k5j7x0c1b4a7d9e2f5g8h1k4

// Verification
bool isMatch = BCrypt.Verify(plainPassword, hash);  // true
bool isWrong = BCrypt.Verify("WrongPassword", hash);  // false
```

### JWT Token Claims

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim("FullName", user.GetFullName()),
    new Claim(ClaimTypes.Role, user.Role),
    new Claim("DepartmentId", user.DepartmentId?.ToString() ?? "0")
};
```

### Role-Based Access Control (RBAC)

```csharp
// Authorization Policies
[Authorize]                                      // Any authenticated user
[Authorize(Roles = "Admin")]                     // Admin only
[Authorize(Roles = "Faculty,Admin")]             // Faculty or Admin
[Authorize(Policy = "StudentOnly")]              // Custom policy

// Resource-Level Authorization
public async Task<IActionResult> GetStudentById(int id)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    var userRole = User.FindFirst(ClaimTypes.Role).Value;
    
    // Student can only view their own record
    if (userRole == "Student" && id != userId)
        return Forbid();  // 403 Forbidden
    
    return Ok(student);
}
```

### Security Headers

```csharp
// Added by SecurityHeadersMiddleware
response.Headers.Add("X-Content-Type-Options", "nosniff");
response.Headers.Add("X-Frame-Options", "DENY");
response.Headers.Add("Content-Security-Policy", "default-src 'self'");
response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
```

---

## Testing Strategy

### Unit Testing

```csharp
[Fact]
public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
{
    // Arrange
    var user = TestFixtures.CreateSampleUser();
    var request = new LoginRequest { UsernameOrEmail = user.Email, Password = "password" };
    
    var mockRepository = new Mock<IGenericRepository<User>>();
    mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<User> { user });
    
    var mockPasswordHasher = new Mock<IPasswordHasher>();
    mockPasswordHasher.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
        .Returns(true);
    
    // Act
    var result = await authService.LoginAsync(request);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.AccessToken);
}
```

### Integration Testing

```csharp
[Fact]
public async Task RegisterUser_WithValidData_ShouldSucceed()
{
    // Arrange
    var request = new RegisterRequest
    {
        Username = "testuser",
        Email = "test@example.com",
        Password = "SecurePassword123!"
    };
    
    // Act
    var response = await PostAsync("/api/v1/auth/register", request);
    
    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

### Test Coverage

- **Overall:** >80%
- **Services:** >85%
- **Controllers:** >75%
- **Security:** >90%

### Running Tests

```bash
# All tests
dotnet test EMS.Tests/EMS.Tests.csproj

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Specific category
dotnet test --filter "Category=Integration"
```

---

## Docker & Deployment

### Multi-Stage Dockerfile

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl -f http://localhost:5000/health || exit 1
EXPOSE 5000
ENTRYPOINT ["dotnet", "EMS.Api.dll"]
```

### Docker Compose

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: YourPassword123!
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  ems-api:
    build:
      context: .
      dockerfile: EMS.Api/Dockerfile
    depends_on:
      - sqlserver
      - redis
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=EMS_DB;User Id=sa;Password=YourPassword123!;"
      ConnectionStrings__Redis: "redis:6379"
    ports:
      - "5000:5000"
```

### CI/CD Pipeline

```
Push to GitHub
    ?
Build & Test (build-test.yml)
    ?? Run unit tests
    ?? Collect coverage
    ?? Upload artifacts
    ?
Docker Build (docker-build.yml)
    ?? Build image
    ?? Push to registry
    ?
Deploy Staging (deploy-staging.yml)
    ?? Deploy to Azure ACI
    ?? Run integration tests
    ?
Deploy Production (deploy-prod.yml)
    ?? Manual approval
    ?? Deploy to Azure ACI
    ?? Health checks
```

---

## Performance Optimization

### 1. Caching Strategy

```csharp
// Cache frequently accessed data
var cacheKey = $"student:{id}";
var cached = await _cache.GetAsync<StudentResponse>(cacheKey);
if (cached != null) return cached;

// Get from database
var student = await _repository.GetByIdAsync(id);
var response = _mapper.Map<StudentResponse>(student);

// Cache result for 1 hour
await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(1));
```

### 2. Database Optimization

```sql
-- Indexes for fast lookups
CREATE INDEX IX_User_Email ON [Users](Email);
CREATE INDEX IX_Student_EnrollmentNumber ON [Students](EnrollmentNumber);
CREATE INDEX IX_Grade_StudentId ON [Grades](StudentId);
CREATE INDEX IX_Attendance_StudentId_CourseId ON [Attendance](StudentId, CourseId);

-- Stored Procedure for complex operations
CREATE PROCEDURE sp_CalculateStudentGPA
    @StudentId INT
AS
BEGIN
    SELECT AVG(FinalPercentageScore) FROM Grades 
    WHERE StudentId = @StudentId AND IsApproved = 1
END
```

### 3. Async/Await Usage

```csharp
// All database calls are async
public async Task<Student> GetByIdAsync(int id)
{
    using var connection = _factory.CreateConnection();
    return await connection.QueryFirstOrDefaultAsync<Student>(
        "SELECT * FROM Students WHERE Id = @Id", 
        new { Id = id });
}

// Non-blocking HTTP requests
[HttpGet("{id}")]
public async Task<ActionResult> GetStudentById(int id)
{
    var student = await _studentService.GetStudentByIdAsync(id);
    return Ok(student);
}
```

### 4. Connection Pooling

```csharp
// Connection pooling built into SQL connection
var connectionString = "Server=localhost;Database=EMS_DB;Max Pool Size=100;Min Pool Size=5;";
using var connection = new SqlConnection(connectionString);
```

### 5. Query Optimization

```csharp
// Use Dapper for efficient queries (faster than EF Core)
var query = @"
    SELECT s.*, d.DepartmentName
    FROM Students s
    INNER JOIN Departments d ON s.DepartmentId = d.Id
    WHERE s.Status = @Status
    ORDER BY s.LastName
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

var students = await connection.QueryAsync<StudentResponse>(query, 
    new { Status = "Active", Offset = (page - 1) * pageSize, PageSize = pageSize });
```

---

## Best Practices

### 1. Code Organization

? **One service per entity** - Single responsibility  
? **Use interfaces** - Dependency injection  
? **Separation of concerns** - Clear layer boundaries  
? **DRY principle** - Reusable components  
? **Naming conventions** - Clear, descriptive names  

### 2. Error Handling

```csharp
// Custom exceptions for different scenarios
throw new NotFoundException("Student", studentId.ToString());
throw new UnauthorizedException("Invalid credentials");
throw new ConflictException("Email already exists");
throw new ValidationException(validationErrors);

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### 3. Logging

```csharp
// Structured logging with Serilog
_logger.LogInformation("User {UserId} logged in", user.Id);
_logger.LogWarning("Failed login attempt for {Email}", email);
_logger.LogError(ex, "Error processing student {StudentId}", studentId);
```

### 4. Input Validation

```csharp
// FluentValidation
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Password).MinimumLength(8).Matches("[A-Z]");
RuleFor(x => x.Age).GreaterThanOrEqualTo(18).LessThanOrEqualTo(120);
```

### 5. Testing

? **Unit tests** for business logic  
? **Integration tests** for API endpoints  
? **Mock external dependencies**  
? **Follow AAA pattern** (Arrange-Act-Assert)  
? **Aim for >80% coverage**  

### 6. Security

? **Hash passwords** with BCrypt  
? **Use JWT** for authentication  
? **Role-based authorization**  
? **Validate all inputs**  
? **Use HTTPS** in production  
? **Add security headers**  
? **Protect sensitive data**  

### 7. Documentation

? **Swagger/OpenAPI** for API docs  
? **XML comments** on public methods  
? **README files** for each component  
? **Architecture diagrams**  
? **Setup instructions**  

---

## Troubleshooting

### Common Issues

#### **Issue: Database Connection Fails**
```bash
# Check connection string
# Verify SQL Server is running
# Check credentials are correct
# Ensure database exists

# Test connection
sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT 1"
```

#### **Issue: Redis Connection Error**
```bash
# Verify Redis is running
redis-cli ping  # Should return PONG

# Check Redis connection string
ConnectionStrings__Redis=localhost:6379

# Using Docker
docker run -d -p 6379:6379 redis:latest
```

#### **Issue: Tests Fail**
```bash
# Run specific test with verbose output
dotnet test --filter "ClassName=AuthenticationServiceTests" --logger "console;verbosity=detailed"

# Check test configuration
# Verify test database exists
# Check mock setup in test
```

#### **Issue: Docker Image Won't Build**
```bash
# Check Dockerfile syntax
# Verify project files are included
# Check .dockerignore

# Build with verbose output
docker build -t ems-api:latest . --progress=plain
```

#### **Issue: API Returns 401 Unauthorized**
```
Possible causes:
1. Token expired - refresh token
2. Invalid token - re-login
3. Missing Authorization header
4. Wrong role - check [Authorize(Roles=...)]
5. Token in blacklist - logout was called
```

#### **Issue: Slow API Response**
```csharp
// Add caching
await _cache.SetAsync(key, value, TimeSpan.FromHours(1));

// Optimize queries
// Add database indexes
// Use stored procedures for complex operations
// Check middleware performance logs
```

---

## Deployment Checklist

### Pre-Deployment

- [ ] All tests passing (>80% coverage)
- [ ] Code review completed
- [ ] Security scan passed
- [ ] Database migrations ready
- [ ] Configuration updated
- [ ] Docker image built
- [ ] Health check endpoint working

### Deployment

- [ ] GitHub Actions workflow triggered
- [ ] Docker image pushed to registry
- [ ] Azure resources created
- [ ] Environment variables set
- [ ] Database migrated
- [ ] API deployed
- [ ] Health check passing

### Post-Deployment

- [ ] Monitor application logs
- [ ] Verify API endpoints
- [ ] Test authentication/authorization
- [ ] Check database connectivity
- [ ] Monitor performance metrics
- [ ] Setup alerts

---

## Summary

The EMS API is a **comprehensive, production-ready system** that demonstrates:

? **Clean Architecture** - Layered design with clear separation  
? **Best Practices** - SOLID principles, design patterns  
? **Security** - Authentication, authorization, encryption  
? **Performance** - Caching, async/await, optimized queries  
? **Testing** - Unit & integration tests with >80% coverage  
? **Deployment** - Docker, CI/CD, cloud-ready  
? **Documentation** - Comprehensive guides and comments  

This system can serve as a **template for building enterprise APIs** in .NET 8.

---

## Resources

- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Dapper ORM](https://github.com/DapperLib/Dapper)
- [AutoMapper](https://automapper.org/)
- [FluentValidation](https://fluentvalidation.net/)
- [Serilog](https://serilog.net/)
- [xUnit Testing](https://xunit.net/)
- [Docker Documentation](https://docs.docker.com/)
- [GitHub Actions](https://github.com/features/actions)

---

**Document Version:** 1.0.0  
**Last Updated:** January 2024  
**Status:** Complete ?  
**Total Implementation:** Steps 1-7 (All Complete)
