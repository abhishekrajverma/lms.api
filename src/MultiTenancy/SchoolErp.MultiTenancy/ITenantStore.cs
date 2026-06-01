namespace SchoolErp.MultiTenancy;

public interface ITenantStore
{
    Task<TenantInfo?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantInfo?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task RegisterAsync(TenantInfo tenant, CancellationToken cancellationToken = default);
}

public sealed record TenantInfo(Guid Id, string Code, string Name);
