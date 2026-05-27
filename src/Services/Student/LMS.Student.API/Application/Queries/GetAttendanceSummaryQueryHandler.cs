using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Application.Queries;

public sealed class GetAttendanceSummaryQueryHandler
    : IRequestHandler<GetAttendanceSummaryQuery, Result<AttendanceSummaryDto>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetAttendanceSummaryQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<AttendanceSummaryDto>> Handle(
        GetAttendanceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<AttendanceSummaryDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var totalStudents = await _db.Students.CountAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var snapshot = await _db.DailyAttendanceSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Date == today, cancellationToken);

        var presentToday = snapshot?.PresentCount ?? await _db.Students
            .Where(s => s.Status == Domain.Aggregates.StudentStatus.Active && s.AttendancePercentage >= 80)
            .CountAsync(cancellationToken);
        var absentToday = snapshot?.AbsentCount ?? Math.Max(0, totalStudents - presentToday);
        var rate = totalStudents == 0
            ? 0
            : Math.Round(100m * presentToday / totalStudents, 1);

        return Result.Success(new AttendanceSummaryDto(totalStudents, presentToday, absentToday, rate));
    }
}
