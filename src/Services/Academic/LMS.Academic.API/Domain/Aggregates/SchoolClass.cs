using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public sealed class SchoolClass : TenantAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;

    private SchoolClass(Guid id, Guid tenantId) : base(id, tenantId) { }
    private SchoolClass() { }

    public static SchoolClass Create(Guid tenantId, string name, string grade)
    {
        Guard.AgainstEmptyGuid(tenantId);
        Guard.AgainstNullOrEmpty(name);
        Guard.AgainstNullOrEmpty(grade);
        return new SchoolClass(Guid.NewGuid(), tenantId) { Name = name.Trim(), Grade = grade.Trim() };
    }
}
