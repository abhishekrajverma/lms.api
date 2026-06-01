using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Communication.Domain.Aggregates;

public sealed class Notification : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private Notification(Guid id, Guid tenantId, string Subject)
    {
        Id = id;
        TenantId = tenantId;
        Subject = Subject;
    }

        public static Notification Create(Guid tenantId, string Subject)
    {
        return new Notification(Guid.NewGuid(), tenantId, Subject);
    }
    
}
