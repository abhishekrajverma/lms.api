using LMS.SharedKernal.Primitives;

namespace LMS.SharedKernal.Persistence;

/// <summary>
/// Base entity for tenant-scoped aggregates and entities.
/// TenantId is stamped automatically by TenantAwareDbContext on insert.
/// </summary>
public abstract class TenantEntity : Entity, IHasTenant
{
    public Guid TenantId { get; protected set; }

    protected TenantEntity(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    protected TenantEntity() { }

    internal void SetTenantId(Guid tenantId) => TenantId = tenantId;
}
