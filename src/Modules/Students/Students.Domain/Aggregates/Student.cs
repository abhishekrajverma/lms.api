using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Students.Domain.Aggregates;

public sealed class Student : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    private Student(Guid id, Guid tenantId, string firstName, string lastName)
    {
        Id = id;
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
    }

    public static Student Create(Guid tenantId, string firstName, string lastName) =>
        new(Guid.NewGuid(), tenantId, firstName, lastName);
}
