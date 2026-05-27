using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public sealed class ClassSession : TenantAggregateRoot
{
    public string Subject { get; private set; } = string.Empty;
    public string ClassName { get; private set; } = string.Empty;
    public string TeacherName { get; private set; } = string.Empty;
    public string TimeSlot { get; private set; } = string.Empty;
    public string Room { get; private set; } = string.Empty;
    public DateOnly SessionDate { get; private set; }

    private ClassSession(Guid id, Guid tenantId) : base(id, tenantId) { }
    private ClassSession() { }

    public static ClassSession Create(
        Guid tenantId,
        string subject,
        string className,
        string teacherName,
        string timeSlot,
        string room,
        DateOnly sessionDate)
    {
        Guard.AgainstEmptyGuid(tenantId);
        return new ClassSession(Guid.NewGuid(), tenantId)
        {
            Subject = subject.Trim(),
            ClassName = className.Trim(),
            TeacherName = teacherName.Trim(),
            TimeSlot = timeSlot.Trim(),
            Room = room.Trim(),
            SessionDate = sessionDate
        };
    }
}
