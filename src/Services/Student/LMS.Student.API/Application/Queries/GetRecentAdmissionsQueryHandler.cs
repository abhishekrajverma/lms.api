using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Application.Queries;

public sealed class GetRecentAdmissionsQueryHandler
    : IRequestHandler<GetRecentAdmissionsQuery, Result<IReadOnlyList<RecentAdmissionDto>>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetRecentAdmissionsQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<RecentAdmissionDto>>> Handle(
        GetRecentAdmissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<RecentAdmissionDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var limit = Math.Clamp(request.Limit, 1, 50);
        var items = await _db.Students
            .AsNoTracking()
            .OrderByDescending(s => s.EnrolledAtUtc)
            .Take(limit)
            .Select(s => new RecentAdmissionDto(
                s.Id,
                s.FirstName + " " + s.LastName,
                s.ClassName,
                s.EnrolledAtUtc.ToString("yyyy-MM-dd"),
                "Guardian"))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<RecentAdmissionDto>>(items);
    }
}
