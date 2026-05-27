using LMS.SharedKernal.Primitives;
using LMS.Tenant.API.Domain.Aggregates;

namespace LMS.Tenant.API.Domain.Repositories;

public interface ISchoolTenantRepository : IRepository<SchoolTenant>
{
    Task<SchoolTenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
}
