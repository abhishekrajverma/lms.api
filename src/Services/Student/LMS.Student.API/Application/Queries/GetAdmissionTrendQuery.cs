using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LMS.Student.API.Application.Queries;

public sealed record AdmissionTrendPointDto(string Month, int Admissions);

public sealed record GetAdmissionTrendQuery(int Months = 6) : IRequest<Result<IReadOnlyList<AdmissionTrendPointDto>>>;

public sealed class GetAdmissionTrendQueryHandler : IRequestHandler<GetAdmissionTrendQuery, Result<IReadOnlyList<AdmissionTrendPointDto>>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetAdmissionTrendQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<AdmissionTrendPointDto>>> Handle(
        GetAdmissionTrendQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<AdmissionTrendPointDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var from = DateTime.UtcNow.AddMonths(-request.Months);
        var grouped = await _db.Students
            .AsNoTracking()
            .Where(s => s.EnrolledAtUtc >= from)
            .GroupBy(s => new { s.EnrolledAtUtc.Year, s.EnrolledAtUtc.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new AdmissionTrendPointDto(
                new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM", CultureInfo.InvariantCulture),
                g.Count()))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AdmissionTrendPointDto>>(grouped);
    }
}
