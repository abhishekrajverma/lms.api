using NetArchTest.Rules;
using Xunit;

namespace LMS.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string StudentDomain = "LMS.Student.API.Domain";
    private const string StudentApplication = "LMS.Student.API.Application";
    private const string StudentInfrastructure = "LMS.Student.API.Infrastructure";
    private const string StudentControllers = "LMS.Student.API.Controllers";

    [Fact]
    public void Domain_ShouldNotReference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(LMS.Student.API.Domain.Aggregates.StudentProfile).Assembly)
            .That().ResideInNamespace(StudentDomain)
            .ShouldNot().HaveDependencyOn(StudentInfrastructure)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Domain_ShouldNotReference_Application()
    {
        var result = Types.InAssembly(typeof(LMS.Student.API.Domain.Aggregates.StudentProfile).Assembly)
            .That().ResideInNamespace(StudentDomain)
            .ShouldNot().HaveDependencyOn(StudentApplication)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_ShouldNotReference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(LMS.Student.API.Application.Commands.CreateStudentCommand).Assembly)
            .That().ResideInNamespace(StudentApplication)
            .ShouldNot().HaveDependencyOn(StudentInfrastructure)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Controllers_ShouldNotReference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(LMS.Student.API.Controllers.StudentsController).Assembly)
            .That().ResideInNamespace(StudentControllers)
            .ShouldNot().HaveDependencyOn(StudentInfrastructure)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void TenantDomain_ShouldNotReference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(LMS.Tenant.API.Domain.Aggregates.SchoolTenant).Assembly)
            .That().ResideInNamespace("LMS.Tenant.API.Domain")
            .ShouldNot().HaveDependencyOn("LMS.Tenant.API.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
