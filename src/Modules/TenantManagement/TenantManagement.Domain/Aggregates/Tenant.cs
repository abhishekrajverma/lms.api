using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.TenantManagement.Domain.Aggregates;

public sealed class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";

    private Tenant(Guid id, string name, string code)
    {
        Id = id;
        Name = name;
        Code = code;
    }

    public static Tenant Create(string name, string code) =>
        new(Guid.NewGuid(), name, code);

    public void Provision()
    {
        if (Status == "Provisioned")
            throw new DomainException("Tenant is already provisioned.");
        Status = "Provisioned";
    }
}
