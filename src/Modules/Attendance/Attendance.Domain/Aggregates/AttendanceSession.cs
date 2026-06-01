using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Attendance.Domain.Aggregates;

public sealed class AttendanceSession : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public DateOnly SessionDate { get; private set; }

    private AttendanceSession(Guid id, Guid tenantId, DateOnly sessionDate)
    {
        Id = id;
        TenantId = tenantId;
        SessionDate = sessionDate;
    }

    public static AttendanceSession Create(Guid tenantId, DateOnly sessionDate) =>
        new(Guid.NewGuid(), tenantId, sessionDate);
}
