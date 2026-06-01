using SchoolErp.Modules.Attendance.Domain.Aggregates;

namespace SchoolErp.Modules.Attendance.Domain.Repositories;

public interface IAttendanceSessionRepository
{
    Task AddAsync(AttendanceSession entity, CancellationToken cancellationToken = default);
    Task<AttendanceSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
