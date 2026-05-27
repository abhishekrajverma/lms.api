namespace LMS.Academic.API.Application.DTOs;

public sealed record ClassDto(Guid Id, string Name, string Grade);

public sealed record TeacherListItemDto(
    Guid Id,
    string Name,
    string Department,
    string Subject,
    string Email,
    string Phone,
    decimal Salary,
    string Status);

public sealed record TeachersSummaryDto(int Total, int Active, int OnLeave, int Departments);

public sealed record TransportRouteDto(
    Guid Id,
    string Route,
    string Bus,
    string Driver,
    string DriverPhone,
    int Students,
    int Capacity,
    string Status,
    string Location,
    string Eta,
    string LastUpdate);

public sealed record TransportSummaryDto(int ActiveRoutes, int TotalStudents, int UnderMaintenance);

public sealed record PayrollRecordDto(
    Guid Id,
    string Employee,
    string Department,
    decimal BasicSalary,
    decimal Allowances,
    decimal Deductions,
    decimal NetSalary,
    string Status);

public sealed record PayrollSummaryDto(decimal Total, decimal Pending, decimal Paid);

public sealed record PendingSalaryApprovalDto(
    Guid Id,
    string Name,
    decimal Amount,
    string Month,
    string Status);

public sealed record UpcomingExamDto(Guid Id, string Subject, string Class, string Date, string Time);

public sealed record TodaysClassDto(Guid Id, string Subject, string Class, string Teacher, string Time, string Room);

public sealed record TeacherAttendanceTodayDto(
    Guid Id,
    string Name,
    string Department,
    string Status,
    string Time);

public sealed record SalaryByDepartmentDto(string Department, decimal Amount);

public sealed record RevenueGrowthDto(string Month, decimal Revenue, decimal Expenses);

public sealed record ClassPerformanceDto(string Class, decimal AvgScore, int Students);

public sealed record SubjectPerformanceDto(string Subject, decimal AvgScore);

public sealed record MonthlyRevenueReportDto(string Month, decimal Revenue, decimal Fees, decimal Other);

public sealed record ExpenseCategoryDto(string Category, decimal Amount);

public sealed record AttendanceTrendWeekDto(string Week, int Students, int Teachers);
