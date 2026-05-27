using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;
using LMS.Student.API.Domain.Events;

namespace LMS.Student.API.Domain.Aggregates;

public enum StudentStatus
{
    Active = 0,
    Inactive = 1
}

public enum StudentFeeStatus
{
    Paid = 0,
    Pending = 1,
    Overdue = 2
}

public sealed class StudentProfile : TenantAggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string AdmissionNumber { get; private set; } = string.Empty;
    public string ClassName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public StudentStatus Status { get; private set; } = StudentStatus.Active;
    public StudentFeeStatus FeeStatus { get; private set; } = StudentFeeStatus.Pending;
    public decimal AttendancePercentage { get; private set; }
    public int PresentDays { get; private set; }
    public int AbsentDays { get; private set; }
    public int LateDays { get; private set; }
    public DateTime EnrolledAtUtc { get; private set; }

    private StudentProfile(Guid id, Guid tenantId) : base(id, tenantId) { }

    private StudentProfile() { }

    public static StudentProfile Enroll(
        Guid tenantId,
        string firstName,
        string lastName,
        string admissionNumber,
        string? className = null,
        string? email = null,
        string? phone = null,
        StudentStatus status = StudentStatus.Active,
        StudentFeeStatus feeStatus = StudentFeeStatus.Pending,
        decimal attendancePercentage = 0,
        int presentDays = 0,
        int absentDays = 0,
        int lateDays = 0)
    {
        Guard.AgainstEmptyGuid(tenantId, nameof(tenantId));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstNullOrEmpty(admissionNumber, nameof(admissionNumber));

        var student = new StudentProfile(Guid.NewGuid(), tenantId)
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            AdmissionNumber = admissionNumber.Trim(),
            ClassName = className?.Trim() ?? string.Empty,
            Email = email?.Trim() ?? string.Empty,
            Phone = phone?.Trim() ?? string.Empty,
            Status = status,
            FeeStatus = feeStatus,
            AttendancePercentage = attendancePercentage,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            LateDays = lateDays,
            EnrolledAtUtc = DateTime.UtcNow
        };

        student.RaiseDomainEvent(new StudentEnrolledDomainEvent(student.Id, tenantId, student.AdmissionNumber));
        return student;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
