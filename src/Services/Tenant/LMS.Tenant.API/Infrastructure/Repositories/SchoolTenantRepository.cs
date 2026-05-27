using LMS.Tenant.API.Domain.Aggregates;
using LMS.Tenant.API.Domain.Repositories;
using LMS.Tenant.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tenant.API.Infrastructure.Repositories;

public sealed class SchoolTenantRepository : ISchoolTenantRepository
{
    private readonly TenantDbContext _db;

    public SchoolTenantRepository(TenantDbContext db) => _db = db;

    public async Task<SchoolTenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<SchoolTenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default) =>
        await _db.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain.ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(SchoolTenant entity, CancellationToken cancellationToken = default) =>
        await _db.Tenants.AddAsync(entity, cancellationToken);

    public void Update(SchoolTenant entity) => _db.Tenants.Update(entity);

    public void Remove(SchoolTenant entity) => _db.Tenants.Remove(entity);
}
