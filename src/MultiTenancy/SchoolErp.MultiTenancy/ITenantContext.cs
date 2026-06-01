namespace SchoolErp.MultiTenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    bool HasTenant { get; }
    void SetTenant(Guid tenantId);
}

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public bool HasTenant => TenantId.HasValue;
    public void SetTenant(Guid tenantId) => TenantId = tenantId;
}
