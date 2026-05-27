using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public sealed class ExamSchedule : TenantAggregateRoot
{
    public string Subject { get; private set; } = string.Empty;
    public string ClassName { get; private set; } = string.Empty;
    public DateOnly ExamDate { get; private set; }
    public string ExamTime { get; private set; } = string.Empty;

    private ExamSchedule(Guid id, Guid tenantId) : base(id, tenantId) { }
    private ExamSchedule() { }

    public static ExamSchedule Schedule(
        Guid tenantId,
        string subject,
        string className,
        DateOnly examDate,
        string examTime)
    {
        Guard.AgainstEmptyGuid(tenantId);
        return new ExamSchedule(Guid.NewGuid(), tenantId)
        {
            Subject = subject.Trim(),
            ClassName = className.Trim(),
            ExamDate = examDate,
            ExamTime = examTime.Trim()
        };
    }
}
