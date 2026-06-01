using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Billing.Domain.Aggregates;

public sealed class Subscription : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private Subscription(Guid id, Guid tenantId, string PlanCode)
    {
        Id = id;
        TenantId = tenantId;
        PlanCode = PlanCode;
    }

        public static Subscription Create(Guid tenantId, string PlanCode)
    {
        return new Subscription(Guid.NewGuid(), tenantId, PlanCode);
    }
    
}
