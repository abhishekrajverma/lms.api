using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Transport.Domain.Aggregates;

public sealed class Route : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private Route(Guid id, Guid tenantId, string Name)
    {
        Id = id;
        TenantId = tenantId;
        Name = Name;
    }

        public static Route Create(Guid tenantId, string Name)
    {
        return new Route(Guid.NewGuid(), tenantId, Name);
    }
    
}
