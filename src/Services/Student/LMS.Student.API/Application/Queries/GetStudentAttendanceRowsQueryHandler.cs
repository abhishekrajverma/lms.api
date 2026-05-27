using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Application.Queries;

public sealed class GetStudentAttendanceRowsQueryHandler
    : IRequestHandler<GetStudentAttendanceRowsQuery, Result<IReadOnlyList<StudentAttendanceRowDto>>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetStudentAttendanceRowsQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<StudentAttendanceRowDto>>> Handle(
        GetStudentAttendanceRowsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<StudentAttendanceRowDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var query = _db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Class))
            query = query.Where(s => s.ClassName == request.Class.Trim());

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term));
        }

        var rows = await query
            .OrderBy(s => s.ClassName)
            .ThenBy(s => s.AdmissionNumber)
            .Select(s => new StudentAttendanceRowDto(
                s.Id,
                s.FirstName + " " + s.LastName,
                s.ClassName,
                s.PresentDays,
                s.AbsentDays,
                s.LateDays,
                s.AttendancePercentage))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StudentAttendanceRowDto>>(rows);
    }
}
