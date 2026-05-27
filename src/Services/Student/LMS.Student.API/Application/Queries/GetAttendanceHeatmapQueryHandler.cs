using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LMS.Student.API.Application.Queries;

public sealed class GetAttendanceHeatmapQueryHandler
    : IRequestHandler<GetAttendanceHeatmapQuery, Result<IReadOnlyList<AttendanceHeatmapCellDto>>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetAttendanceHeatmapQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<AttendanceHeatmapCellDto>>> Handle(
        GetAttendanceHeatmapQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<AttendanceHeatmapCellDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-request.Months));
        var snapshots = await _db.DailyAttendanceSnapshots
            .AsNoTracking()
            .Where(s => s.Date >= from)
            .OrderBy(s => s.Date)
            .ToListAsync(cancellationToken);

        var cells = snapshots.Select(s => new AttendanceHeatmapCellDto(
            s.Date.ToString("MMM", CultureInfo.InvariantCulture),
            s.Date.Day,
            s.AttendancePercentage)).ToList();

        return Result.Success<IReadOnlyList<AttendanceHeatmapCellDto>>(cells);
    }
}
