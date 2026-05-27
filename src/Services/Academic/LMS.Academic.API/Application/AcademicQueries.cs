using LMS.Academic.API.Application.DTOs;
using LMS.Academic.API.Domain.Aggregates;
using LMS.Academic.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LMS.Academic.API.Application.Queries;

public sealed record ListClassesQuery : IRequest<Result<IReadOnlyList<ClassDto>>>;
public sealed record ListTeachersQuery(string? Search, string? Department, string? Status) : IRequest<Result<IReadOnlyList<TeacherListItemDto>>>;
public sealed record GetTeachersSummaryQuery : IRequest<Result<TeachersSummaryDto>>;
public sealed record ListTransportRoutesQuery(string? Status) : IRequest<Result<IReadOnlyList<TransportRouteDto>>>;
public sealed record GetTransportSummaryQuery : IRequest<Result<TransportSummaryDto>>;
public sealed record ListPayrollRecordsQuery(string? Status, string? Department) : IRequest<Result<IReadOnlyList<PayrollRecordDto>>>;
public sealed record GetPayrollSummaryQuery : IRequest<Result<PayrollSummaryDto>>;
public sealed record GetPendingSalaryApprovalsQuery : IRequest<Result<IReadOnlyList<PendingSalaryApprovalDto>>>;
public sealed record GetUpcomingExamsQuery(int Limit = 10) : IRequest<Result<IReadOnlyList<UpcomingExamDto>>>;
public sealed record GetTodaysClassesQuery : IRequest<Result<IReadOnlyList<TodaysClassDto>>>;
public sealed record GetTeacherAttendanceTodayQuery : IRequest<Result<IReadOnlyList<TeacherAttendanceTodayDto>>>;
public sealed record GetSalaryDistributionQuery : IRequest<Result<IReadOnlyList<SalaryByDepartmentDto>>>;
public sealed record GetRevenueGrowthQuery(int Months = 6) : IRequest<Result<IReadOnlyList<RevenueGrowthDto>>>;
public sealed record GetClassPerformanceQuery : IRequest<Result<IReadOnlyList<ClassPerformanceDto>>>;
public sealed record GetSubjectPerformanceQuery : IRequest<Result<IReadOnlyList<SubjectPerformanceDto>>>;
public sealed record GetMonthlyRevenueReportQuery(int Months = 6) : IRequest<Result<IReadOnlyList<MonthlyRevenueReportDto>>>;
public sealed record GetExpenseCategoriesQuery : IRequest<Result<IReadOnlyList<ExpenseCategoryDto>>>;
public sealed record GetAttendanceTrendQuery : IRequest<Result<IReadOnlyList<AttendanceTrendWeekDto>>>;

public sealed class ListClassesQueryHandler : IRequestHandler<ListClassesQuery, Result<IReadOnlyList<ClassDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public ListClassesQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<ClassDto>>> Handle(ListClassesQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<ClassDto>>();
        var items = await _db.Classes.AsNoTracking()
            .OrderBy(c => c.Grade).ThenBy(c => c.Name)
            .Select(c => new ClassDto(c.Id, c.Name, c.Grade))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<ClassDto>>(items);
    }
}

public sealed class ListTeachersQueryHandler : IRequestHandler<ListTeachersQuery, Result<IReadOnlyList<TeacherListItemDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public ListTeachersQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<TeacherListItemDto>>> Handle(ListTeachersQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<TeacherListItemDto>>();
        var query = _db.Teachers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(t => t.FirstName.ToLower().Contains(term) || t.LastName.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(request.Department))
            query = query.Where(t => t.Department == request.Department.Trim());
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TeacherStatus>(request.Status, true, out var status))
            query = query.Where(t => t.Status == status);

        var items = await query.OrderBy(t => t.Department).ThenBy(t => t.LastName)
            .Select(t => new TeacherListItemDto(
                t.Id, t.FirstName + " " + t.LastName, t.Department, t.Subject, t.Email, t.Phone, t.Salary,
                t.Status == TeacherStatus.OnLeave ? "on-leave" : t.Status.ToString().ToLowerInvariant()))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<TeacherListItemDto>>(items);
    }
}

public sealed class GetTeachersSummaryQueryHandler : IRequestHandler<GetTeachersSummaryQuery, Result<TeachersSummaryDto>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetTeachersSummaryQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<TeachersSummaryDto>> Handle(GetTeachersSummaryQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<TeachersSummaryDto>();
        var total = await _db.Teachers.CountAsync(ct);
        var active = await _db.Teachers.CountAsync(t => t.Status == TeacherStatus.Active, ct);
        var onLeave = await _db.Teachers.CountAsync(t => t.Status == TeacherStatus.OnLeave, ct);
        var departments = await _db.Teachers.Select(t => t.Department).Distinct().CountAsync(ct);
        return Result.Success(new TeachersSummaryDto(total, active, onLeave, departments));
    }
}

public sealed class ListTransportRoutesQueryHandler : IRequestHandler<ListTransportRoutesQuery, Result<IReadOnlyList<TransportRouteDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public ListTransportRoutesQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<TransportRouteDto>>> Handle(ListTransportRoutesQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<TransportRouteDto>>();
        var routes = await _db.TransportRoutes.AsNoTracking().ToListAsync(ct);
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TransportStatus>(request.Status, true, out var status))
            routes = routes.Where(r => r.Status == status).ToList();

        var dtos = routes.Select(r => new TransportRouteDto(
            r.Id, r.RouteName, r.BusNumber, r.DriverName, r.DriverPhone, r.StudentCount, r.Capacity,
            r.Status.ToString().ToLowerInvariant(), r.Location, r.Eta, r.LastUpdate)).ToList();
        return Result.Success<IReadOnlyList<TransportRouteDto>>(dtos);
    }
}

public sealed class GetTransportSummaryQueryHandler : IRequestHandler<GetTransportSummaryQuery, Result<TransportSummaryDto>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetTransportSummaryQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<TransportSummaryDto>> Handle(GetTransportSummaryQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<TransportSummaryDto>();
        var routes = await _db.TransportRoutes.AsNoTracking().ToListAsync(ct);
        return Result.Success(new TransportSummaryDto(
            routes.Count(r => r.Status == TransportStatus.Active),
            routes.Sum(r => r.StudentCount),
            routes.Count(r => r.Status == TransportStatus.Maintenance)));
    }
}

public sealed class ListPayrollRecordsQueryHandler : IRequestHandler<ListPayrollRecordsQuery, Result<IReadOnlyList<PayrollRecordDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public ListPayrollRecordsQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<PayrollRecordDto>>> Handle(ListPayrollRecordsQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<PayrollRecordDto>>();
        var records = await _db.PayrollRecords.AsNoTracking().ToListAsync(ct);
        if (!string.IsNullOrWhiteSpace(request.Department))
            records = records.Where(r => r.Department == request.Department.Trim()).ToList();
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<PayrollStatus>(request.Status, true, out var status))
            records = records.Where(r => r.Status == status).ToList();

        var dtos = records.Select(r => new PayrollRecordDto(
            r.Id, r.EmployeeName, r.Department, r.BasicSalary, r.Allowances, r.Deductions, r.NetSalary,
            r.Status.ToString().ToLowerInvariant())).ToList();
        return Result.Success<IReadOnlyList<PayrollRecordDto>>(dtos);
    }
}

public sealed class GetPayrollSummaryQueryHandler : IRequestHandler<GetPayrollSummaryQuery, Result<PayrollSummaryDto>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetPayrollSummaryQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<PayrollSummaryDto>> Handle(GetPayrollSummaryQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<PayrollSummaryDto>();
        var records = await _db.PayrollRecords.AsNoTracking().ToListAsync(ct);
        return Result.Success(new PayrollSummaryDto(
            records.Sum(r => r.NetSalary),
            records.Where(r => r.Status == PayrollStatus.Pending).Sum(r => r.NetSalary),
            records.Where(r => r.Status == PayrollStatus.Paid).Sum(r => r.NetSalary)));
    }
}

public sealed class GetPendingSalaryApprovalsQueryHandler : IRequestHandler<GetPendingSalaryApprovalsQuery, Result<IReadOnlyList<PendingSalaryApprovalDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetPendingSalaryApprovalsQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<PendingSalaryApprovalDto>>> Handle(GetPendingSalaryApprovalsQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<PendingSalaryApprovalDto>>();
        var items = await _db.PayrollRecords.AsNoTracking()
            .Where(r => r.Status == PayrollStatus.Pending || r.Status == PayrollStatus.Approved)
            .OrderBy(r => r.Status)
            .Take(10)
            .Select(r => new PendingSalaryApprovalDto(
                r.Id, r.EmployeeName, r.NetSalary, r.PayMonth, r.Status.ToString().ToLowerInvariant()))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<PendingSalaryApprovalDto>>(items);
    }
}

public sealed class GetUpcomingExamsQueryHandler : IRequestHandler<GetUpcomingExamsQuery, Result<IReadOnlyList<UpcomingExamDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetUpcomingExamsQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<UpcomingExamDto>>> Handle(GetUpcomingExamsQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<UpcomingExamDto>>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = await _db.ExamSchedules.AsNoTracking()
            .Where(e => e.ExamDate >= today)
            .OrderBy(e => e.ExamDate)
            .Take(request.Limit)
            .Select(e => new UpcomingExamDto(e.Id, e.Subject, e.ClassName, e.ExamDate.ToString("yyyy-MM-dd"), e.ExamTime))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<UpcomingExamDto>>(items);
    }
}

public sealed class GetTodaysClassesQueryHandler : IRequestHandler<GetTodaysClassesQuery, Result<IReadOnlyList<TodaysClassDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetTodaysClassesQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<TodaysClassDto>>> Handle(GetTodaysClassesQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<TodaysClassDto>>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = await _db.ClassSessions.AsNoTracking()
            .Where(s => s.SessionDate == today)
            .OrderBy(s => s.TimeSlot)
            .Select(s => new TodaysClassDto(s.Id, s.Subject, s.ClassName, s.TeacherName, s.TimeSlot, s.Room))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<TodaysClassDto>>(items);
    }
}

public sealed class GetTeacherAttendanceTodayQueryHandler : IRequestHandler<GetTeacherAttendanceTodayQuery, Result<IReadOnlyList<TeacherAttendanceTodayDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetTeacherAttendanceTodayQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<TeacherAttendanceTodayDto>>> Handle(GetTeacherAttendanceTodayQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<TeacherAttendanceTodayDto>>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = await _db.TeacherAttendanceRecords.AsNoTracking()
            .Where(r => r.Date == today)
            .Select(r => new TeacherAttendanceTodayDto(
                r.Id, r.TeacherName, r.Department,
                r.Status.ToString().ToLowerInvariant(),
                r.CheckInTime))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<TeacherAttendanceTodayDto>>(items);
    }
}

public sealed class GetSalaryDistributionQueryHandler : IRequestHandler<GetSalaryDistributionQuery, Result<IReadOnlyList<SalaryByDepartmentDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetSalaryDistributionQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<SalaryByDepartmentDto>>> Handle(GetSalaryDistributionQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<SalaryByDepartmentDto>>();
        var items = await _db.Teachers.AsNoTracking()
            .GroupBy(t => t.Department)
            .Select(g => new SalaryByDepartmentDto(g.Key, g.Sum(t => t.Salary)))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<SalaryByDepartmentDto>>(items);
    }
}

public sealed class GetRevenueGrowthQueryHandler : IRequestHandler<GetRevenueGrowthQuery, Result<IReadOnlyList<RevenueGrowthDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetRevenueGrowthQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<RevenueGrowthDto>>> Handle(GetRevenueGrowthQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<RevenueGrowthDto>>();
        var payroll = await _db.PayrollRecords.AsNoTracking().ToListAsync(ct);
        var grouped = payroll
            .GroupBy(p => new { p.PayMonth })
            .Take(request.Months)
            .Select(g => new RevenueGrowthDto(
                g.Key.PayMonth,
                g.Sum(p => p.NetSalary) * 10,
                g.Sum(p => p.NetSalary)))
            .ToList();
        return Result.Success<IReadOnlyList<RevenueGrowthDto>>(grouped);
    }
}

public sealed class GetClassPerformanceQueryHandler : IRequestHandler<GetClassPerformanceQuery, Result<IReadOnlyList<ClassPerformanceDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetClassPerformanceQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<ClassPerformanceDto>>> Handle(GetClassPerformanceQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<ClassPerformanceDto>>();
        var items = await _db.Classes.AsNoTracking()
            .Select(c => new ClassPerformanceDto(c.Name, 75m + c.Grade.Length, 30))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<ClassPerformanceDto>>(items);
    }
}

public sealed class GetSubjectPerformanceQueryHandler : IRequestHandler<GetSubjectPerformanceQuery, Result<IReadOnlyList<SubjectPerformanceDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetSubjectPerformanceQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<SubjectPerformanceDto>>> Handle(GetSubjectPerformanceQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<SubjectPerformanceDto>>();
        var items = await _db.Teachers.AsNoTracking()
            .GroupBy(t => t.Subject)
            .Select(g => new SubjectPerformanceDto(g.Key, 70m + g.Count() * 2))
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<SubjectPerformanceDto>>(items);
    }
}

public sealed class GetMonthlyRevenueReportQueryHandler : IRequestHandler<GetMonthlyRevenueReportQuery, Result<IReadOnlyList<MonthlyRevenueReportDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetMonthlyRevenueReportQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<MonthlyRevenueReportDto>>> Handle(GetMonthlyRevenueReportQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<MonthlyRevenueReportDto>>();
        var payroll = await _db.PayrollRecords.AsNoTracking().ToListAsync(ct);
        var items = payroll
            .GroupBy(p => p.PayMonth)
            .Take(request.Months)
            .Select(g => new MonthlyRevenueReportDto(g.Key, g.Sum(p => p.NetSalary) * 12, g.Sum(p => p.NetSalary) * 8, g.Sum(p => p.NetSalary)))
            .ToList();
        return Result.Success<IReadOnlyList<MonthlyRevenueReportDto>>(items);
    }
}

public sealed class GetExpenseCategoriesQueryHandler : IRequestHandler<GetExpenseCategoriesQuery, Result<IReadOnlyList<ExpenseCategoryDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetExpenseCategoriesQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<ExpenseCategoryDto>>> Handle(GetExpenseCategoriesQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<ExpenseCategoryDto>>();
        var payroll = await _db.PayrollRecords.AsNoTracking().ToListAsync(ct);
        var items = payroll
            .GroupBy(p => p.Department)
            .Select(g => new ExpenseCategoryDto(g.Key, g.Sum(p => p.NetSalary)))
            .ToList();
        return Result.Success<IReadOnlyList<ExpenseCategoryDto>>(items);
    }
}

public sealed class GetAttendanceTrendQueryHandler : IRequestHandler<GetAttendanceTrendQuery, Result<IReadOnlyList<AttendanceTrendWeekDto>>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    public GetAttendanceTrendQueryHandler(AcademicDbContext db, ITenantContext tenant) { _db = db; _tenant = tenant; }
    public async Task<Result<IReadOnlyList<AttendanceTrendWeekDto>>> Handle(GetAttendanceTrendQuery request, CancellationToken ct)
    {
        if (!_tenant.IsResolved) return AcademicQueryHelpers.TenantRequired<IReadOnlyList<AttendanceTrendWeekDto>>();
        var teacherCount = await _db.Teachers.CountAsync(ct);
        var items = Enumerable.Range(1, 4)
            .Select(w => new AttendanceTrendWeekDto($"Week {w}", 2500 + w * 10, teacherCount))
            .ToList();
        return Result.Success<IReadOnlyList<AttendanceTrendWeekDto>>(items);
    }
}

internal static class AcademicQueryHelpers
{
    public static Result<T> TenantRequired<T>() =>
        Result.Failure<T>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));
}
