namespace LMS.Student.API.Application.DTOs;

public sealed record StudentDto(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string AdmissionNumber,
    string ClassName,
    string Email,
    string Phone,
    string Status,
    string FeeStatus,
    decimal AttendancePercentage,
    int PresentDays,
    int AbsentDays,
    int LateDays,
    DateTime EnrolledAtUtc);

public sealed record StudentListItemDto(
    Guid Id,
    string Name,
    string Class,
    string RollNo,
    string Email,
    string Phone,
    string Status,
    string FeeStatus,
    decimal Attendance);

public sealed record StudentsSummaryDto(
    int Total,
    int Active,
    int NewThisMonth,
    decimal AverageAttendance);

public sealed record PagedStudentsDto(IReadOnlyList<StudentListItemDto> Items, int Total);

public sealed record AttendanceSummaryDto(
    int TotalStudents,
    int PresentToday,
    int AbsentToday,
    decimal AttendanceRate);

public sealed record StudentAttendanceRowDto(
    Guid Id,
    string Name,
    string Class,
    int Present,
    int Absent,
    int Late,
    decimal Percentage);

public sealed record AttendanceHeatmapCellDto(string Month, int Day, int Attendance);

public sealed record WeeklyAttendanceDto(string Day, int Present, int Absent);

public sealed record RecentAdmissionDto(
    Guid Id,
    string Name,
    string Class,
    string Date,
    string Guardian);
