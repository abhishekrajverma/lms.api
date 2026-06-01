using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.TenantManagement.Domain.Aggregates;
using SchoolErp.Modules.TenantManagement.Domain.Repositories;

namespace SchoolErp.Modules.TenantManagement.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly TenantManagementDbContext _db;

    public TenantRepository(TenantManagementDbContext db) => _db = db;

    public async Task AddAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        await _db.Tenants.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Tenants.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
