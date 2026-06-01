using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Identity.Domain.Aggregates;

public sealed class User : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private User(Guid id, Guid tenantId, string Email)
    {
        Id = id;
        TenantId = tenantId;
        Email = Email;
    }

        public static User Create(Guid tenantId, string Email)
    {
        return new User(Guid.NewGuid(), tenantId, Email);
    }
    
}
