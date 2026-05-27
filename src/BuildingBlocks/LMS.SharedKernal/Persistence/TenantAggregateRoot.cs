using LMS.SharedKernal.Primitives;

namespace LMS.SharedKernal.Persistence;

/// <summary>
/// Tenant-scoped aggregate root. TenantId is stamped on insert by TenantAwareDbContext.
/// </summary>
public abstract class TenantAggregateRoot : AggregateRoot, IHasTenant
{
    public Guid TenantId { get; protected set; }

    protected TenantAggregateRoot(Guid id, Guid tenantId) : base(id) => TenantId = tenantId;

    protected TenantAggregateRoot() { }

    internal void SetTenantId(Guid tenantId) => TenantId = tenantId;
}
