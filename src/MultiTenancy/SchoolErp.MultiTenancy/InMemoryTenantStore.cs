using System.Collections.Concurrent;

namespace SchoolErp.MultiTenancy;

public sealed class InMemoryTenantStore : ITenantStore
{
    private readonly ConcurrentDictionary<Guid, TenantInfo> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _byCode = new(StringComparer.OrdinalIgnoreCase);

    public Task<TenantInfo?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.TryGetValue(tenantId, out var t) ? t : null);

    public Task<TenantInfo?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (_byCode.TryGetValue(code, out var id) && _byId.TryGetValue(id, out var t))
            return Task.FromResult<TenantInfo?>(t);
        return Task.FromResult<TenantInfo?>(null);
    }

    public Task RegisterAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
    {
        _byId[tenant.Id] = tenant;
        _byCode[tenant.Code] = tenant.Id;
        return Task.CompletedTask;
    }
}
