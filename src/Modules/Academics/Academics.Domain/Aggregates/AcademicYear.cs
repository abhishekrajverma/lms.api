using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Academics.Domain.Aggregates;

public sealed class AcademicYear : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    private AcademicYear(Guid id, Guid tenantId, string name, DateOnly startDate, DateOnly endDate)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public static AcademicYear Create(Guid tenantId, string name, DateOnly startDate, DateOnly endDate) =>
        new(Guid.NewGuid(), tenantId, name, startDate, endDate);
}
