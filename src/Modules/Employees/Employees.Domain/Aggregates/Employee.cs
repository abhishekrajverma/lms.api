using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Employees.Domain.Aggregates;

public sealed class Employee : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    private Employee(Guid id, Guid tenantId, string firstName, string lastName)
    {
        Id = id;
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
    }

    public static Employee Create(Guid tenantId, string firstName, string lastName) =>
        new(Guid.NewGuid(), tenantId, firstName, lastName);
}
