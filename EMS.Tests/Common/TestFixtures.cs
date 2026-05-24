namespace EMS.Tests.Common;

using AutoMapper;
using EMS.Application.Mappers;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;
using EMS.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;

/// <summary>
/// Common test fixtures for all unit tests
/// Provides mock objects, sample data, and test helpers
/// </summary>
public static class TestFixtures
{
    /// <summary>
    /// Create mock logger
    /// </summary>
    public static ILogger<T> CreateMockLogger<T>() where T : class
    {
        return new Mock<ILogger<T>>().Object;
    }

    /// <summary>
    /// Create AutoMapper instance
    /// </summary>
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        return config.CreateMapper();
    }

    /// <summary>
    /// Create mock unit of work
    /// </summary>
    public static Mock<IUnitOfWork> CreateMockUnitOfWork()
    {
        return new Mock<IUnitOfWork>();
    }

    /// <summary>
    /// Create mock cache service
    /// </summary>
    public static Mock<ICacheService> CreateMockCacheService()
    {
        return new Mock<ICacheService>();
    }

    /// <summary>
    /// Create mock authentication service
    /// </summary>
    public static Mock<IAuthenticationService> CreateMockAuthenticationService()
    {
        return new Mock<IAuthenticationService>();
    }

    /// <summary>
    /// Create mock authorization service
    /// </summary>
    public static Mock<IAuthorizationService> CreateMockAuthorizationService()
    {
        return new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Create sample student
    /// </summary>
    public static Student CreateSampleStudent(int id = 1)
    {
        return new Student
        {
            Id = id,
            EnrollmentNumber = $"STU{id:D4}",
            RollNumber = $"ROLL{id:D4}",
            FirstName = "Ahmed",
            LastName = "Ali",
            DateOfBirth = new DateTime(2000, 1, 1),
            Gender = "Male",
            DepartmentId = 1,
            CurrentSemester = 1,
            CurrentAcademicYear = "2024",
            AdmissionYear = 2020,
            Status = "Active",
            CurrentGPA = 3.5m,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create sample course
    /// </summary>
    public static Course CreateSampleCourse(int id = 1)
    {
        return new Course
        {
            Id = id,
            CourseCode = $"CS{id:D3}",
            Name = "Introduction to Programming",
            Description = "Learn programming basics",
            CreditHours = 3,
            DepartmentId = 1,
            Capacity = 50,
            EnrolledStudentCount = 30,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(4),
            AcademicYear = "2024",
            SemesterType = "Fall",
            CourseLevel = 1,
            IsMandatory = true,
            DeliveryMode = "InPerson",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create sample grade
    /// </summary>
    public static Grade CreateSampleGrade(int id = 1, int studentId = 1, int courseId = 1)
    {
        return new Grade
        {
            Id = id,
            StudentId = studentId,
            CourseId = courseId,
            Semester = "Fall 2024",
            InternalAssessmentScore = 85,
            FinalExamScore = 88,
            FinalPercentageScore = 86.5m,
            LetterGrade = "A",
            GpaPoints = 4.0m,
            IsSubmitted = true,
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create sample attendance
    /// </summary>
    public static Attendance CreateSampleAttendance(int id = 1, int studentId = 1, int courseId = 1)
    {
        return new Attendance
        {
            Id = id,
            StudentId = studentId,
            CourseId = courseId,
            ClassDate = DateTime.UtcNow.AddDays(-1),
            Status = "Present",
            TimeMarked = TimeOnly.Parse("10:00"),
            IsApproved = true,
            MarkedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create sample user
    /// </summary>
    public static User CreateSampleUser(int id = 1, string role = "Student")
    {
        return new User
        {
            Id = id,
            Username = $"user{id}",
            Email = $"user{id}@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "$2a$12$hash", // Fake BCrypt hash
            Role = role,
            AccountStatus = "Active",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create multiple students
    /// </summary>
    public static List<Student> CreateSampleStudents(int count = 5)
    {
        var students = new List<Student>();
        for (int i = 1; i <= count; i++)
        {
            students.Add(CreateSampleStudent(i));
        }
        return students;
    }

    /// <summary>
    /// Create multiple courses
    /// </summary>
    public static List<Course> CreateSampleCourses(int count = 5)
    {
        var courses = new List<Course>();
        for (int i = 1; i <= count; i++)
        {
            courses.Add(CreateSampleCourse(i));
        }
        return courses;
    }

    /// <summary>
    /// Create paginated response helper
    /// </summary>
    public static PaginatedResponse<T> CreatePaginatedResponse<T>(List<T> items, int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = items.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
