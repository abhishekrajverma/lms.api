using LMS.SharedKernal.Results;
using LMS.Tenant.API.Application.DTOs;
using LMS.Tenant.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tenant.API.Application.Queries;

public sealed class ListTenantsQueryHandler : IRequestHandler<ListTenantsQuery, Result<IReadOnlyList<SchoolDto>>>
{
    private readonly TenantDbContext _db;

    public ListTenantsQueryHandler(TenantDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<SchoolDto>>> Handle(
        ListTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var tenants = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var schools = tenants.Select(t => new SchoolDto(
            t.Id,
            t.Name,
            t.Subdomain.ToUpperInvariant(),
            "Enterprise",
            string.Empty)).ToList();

        return Result.Success<IReadOnlyList<SchoolDto>>(schools);
    }
}
