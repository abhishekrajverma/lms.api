using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Application.Queries;

public sealed class GetWeeklyAttendanceQueryHandler
    : IRequestHandler<GetWeeklyAttendanceQuery, Result<IReadOnlyList<WeeklyAttendanceDto>>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    private static readonly string[] Weekdays = ["Mon", "Tue", "Wed", "Thu", "Fri"];

    public GetWeeklyAttendanceQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<WeeklyAttendanceDto>>> Handle(
        GetWeeklyAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<WeeklyAttendanceDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday)
            weekStart = weekStart.AddDays(-7);

        var snapshots = await _db.DailyAttendanceSnapshots
            .AsNoTracking()
            .Where(s => s.Date >= weekStart && s.Date <= weekStart.AddDays(4))
            .ToListAsync(cancellationToken);

        var result = new List<WeeklyAttendanceDto>();
        for (var i = 0; i < 5; i++)
        {
            var date = weekStart.AddDays(i);
            var snap = snapshots.FirstOrDefault(s => s.Date == date);
            result.Add(new WeeklyAttendanceDto(
                Weekdays[i],
                snap?.PresentCount ?? 0,
                snap?.AbsentCount ?? 0));
        }

        return Result.Success<IReadOnlyList<WeeklyAttendanceDto>>(result);
    }
}
