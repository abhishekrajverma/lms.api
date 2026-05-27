using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.Tenant.API.Domain.Events;

namespace LMS.Tenant.API.Domain.Aggregates;

public sealed class SchoolTenant : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;
    public string SchemaName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private SchoolTenant(Guid id) : base(id) { }

    public static SchoolTenant Register(string name, string subdomain)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(subdomain, nameof(subdomain));

        var tenant = new SchoolTenant(Guid.NewGuid())
        {
            Name = name.Trim(),
            Subdomain = subdomain.Trim().ToLowerInvariant(),
            SchemaName = $"tenant_{subdomain.Trim().ToLowerInvariant().Replace('-', '_')}",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        tenant.RaiseDomainEvent(new TenantCreatedDomainEvent(tenant.Id, tenant.Name, tenant.Subdomain));
        tenant.IncrementVersion();
        return tenant;
    }

    public void Deactivate()
    {
        IsActive = false;
        IncrementVersion();
    }
}
