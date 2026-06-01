using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Examinations.Domain.Aggregates;

public sealed class Exam : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private Exam(Guid id, Guid tenantId, string Title)
    {
        Id = id;
        TenantId = tenantId;
        Title = Title;
    }

        public static Exam Create(Guid tenantId, string Title)
    {
        return new Exam(Guid.NewGuid(), tenantId, Title);
    }
    
}
