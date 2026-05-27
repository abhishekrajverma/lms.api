using FluentAssertions;
using Xunit;
using LMS.Student.API.Domain.Aggregates;
using LMS.Student.API.Domain.Events;

namespace LMS.Student.UnitTests.Domain;

public sealed class StudentProfileTests
{
    [Fact]
    public void Enroll_ShouldCreateStudentWithDomainEvent()
    {
        var tenantId = Guid.NewGuid();

        var student = StudentProfile.Enroll(tenantId, "John", "Doe", "ADM-001");

        student.TenantId.Should().Be(tenantId);
        student.FirstName.Should().Be("John");
        student.LastName.Should().Be("Doe");
        student.AdmissionNumber.Should().Be("ADM-001");
        student.DomainEvents.Should().ContainSingle(e => e is StudentEnrolledDomainEvent);
    }
}
