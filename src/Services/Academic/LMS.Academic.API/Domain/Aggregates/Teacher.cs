using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public enum TeacherStatus
{
    Active = 0,
    OnLeave = 1,
    Inactive = 2
}

public sealed class Teacher : TenantAggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public decimal Salary { get; private set; }
    public TeacherStatus Status { get; private set; } = TeacherStatus.Active;

    private Teacher(Guid id, Guid tenantId) : base(id, tenantId) { }
    private Teacher() { }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public static Teacher Hire(
        Guid tenantId,
        string firstName,
        string lastName,
        string department,
        string subject,
        string email,
        string phone,
        decimal salary,
        TeacherStatus status = TeacherStatus.Active)
    {
        Guard.AgainstEmptyGuid(tenantId);
        Guard.AgainstNullOrEmpty(firstName);
        Guard.AgainstNullOrEmpty(lastName);
        return new Teacher(Guid.NewGuid(), tenantId)
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Department = department.Trim(),
            Subject = subject.Trim(),
            Email = email.Trim(),
            Phone = phone.Trim(),
            Salary = salary,
            Status = status
        };
    }
}
