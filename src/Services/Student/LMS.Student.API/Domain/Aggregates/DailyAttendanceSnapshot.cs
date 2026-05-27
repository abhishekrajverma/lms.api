using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Student.API.Domain.Aggregates;

public sealed class DailyAttendanceSnapshot : TenantAggregateRoot
{
    public DateOnly Date { get; private set; }
    public int PresentCount { get; private set; }
    public int AbsentCount { get; private set; }

    private DailyAttendanceSnapshot(Guid id, Guid tenantId) : base(id, tenantId) { }

    private DailyAttendanceSnapshot() { }

    public static DailyAttendanceSnapshot Create(Guid tenantId, DateOnly date, int presentCount, int absentCount)
    {
        Guard.AgainstEmptyGuid(tenantId);
        return new DailyAttendanceSnapshot(Guid.NewGuid(), tenantId)
        {
            Date = date,
            PresentCount = presentCount,
            AbsentCount = absentCount
        };
    }

    public int AttendancePercentage =>
        PresentCount + AbsentCount == 0
            ? 0
            : (int)Math.Round(100.0 * PresentCount / (PresentCount + AbsentCount));
}
