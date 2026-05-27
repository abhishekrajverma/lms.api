using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Domain.Aggregates;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Application.Queries;

public sealed class GetStudentsSummaryQueryHandler : IRequestHandler<GetStudentsSummaryQuery, Result<StudentsSummaryDto>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetStudentsSummaryQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<StudentsSummaryDto>> Handle(
        GetStudentsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<StudentsSummaryDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var total = await _db.Students.CountAsync(cancellationToken);
        var active = await _db.Students.CountAsync(s => s.Status == StudentStatus.Active, cancellationToken);
        var newThisMonth = await _db.Students.CountAsync(s => s.EnrolledAtUtc >= startOfMonth, cancellationToken);
        var avgAttendance = total == 0
            ? 0
            : await _db.Students.AverageAsync(s => s.AttendancePercentage, cancellationToken);

        return Result.Success(new StudentsSummaryDto(total, active, newThisMonth, Math.Round(avgAttendance, 1)));
    }
}
