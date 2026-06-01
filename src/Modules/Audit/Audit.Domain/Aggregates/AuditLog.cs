using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Audit.Domain.Aggregates;

public sealed class AuditLog : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private AuditLog(Guid id, Guid tenantId, string Action)
    {
        Id = id;
        TenantId = tenantId;
        Action = Action;
    }

        public static AuditLog Create(Guid tenantId, string Action)
    {
        return new AuditLog(Guid.NewGuid(), tenantId, Action);
    }
    
}
