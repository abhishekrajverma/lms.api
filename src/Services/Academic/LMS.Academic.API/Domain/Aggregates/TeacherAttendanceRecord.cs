using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public enum TeacherAttendanceStatus
{
    Present = 0,
    Late = 1,
    Absent = 2
}

public sealed class TeacherAttendanceRecord : TenantAggregateRoot
{
    public Guid TeacherId { get; private set; }
    public string TeacherName { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public TeacherAttendanceStatus Status { get; private set; }
    public string CheckInTime { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }

    private TeacherAttendanceRecord(Guid id, Guid tenantId) : base(id, tenantId) { }
    private TeacherAttendanceRecord() { }

    public static TeacherAttendanceRecord Create(
        Guid tenantId,
        Guid teacherId,
        string teacherName,
        string department,
        TeacherAttendanceStatus status,
        string checkInTime,
        DateOnly date)
    {
        Guard.AgainstEmptyGuid(tenantId);
        return new TeacherAttendanceRecord(Guid.NewGuid(), tenantId)
        {
            TeacherId = teacherId,
            TeacherName = teacherName.Trim(),
            Department = department.Trim(),
            Status = status,
            CheckInTime = checkInTime.Trim(),
            Date = date
        };
    }
}
