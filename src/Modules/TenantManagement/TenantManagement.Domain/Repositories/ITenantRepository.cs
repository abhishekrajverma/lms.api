using SchoolErp.Modules.TenantManagement.Domain.Aggregates;

namespace SchoolErp.Modules.TenantManagement.Domain.Repositories;

public interface ITenantRepository
{
    Task AddAsync(Tenant entity, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
