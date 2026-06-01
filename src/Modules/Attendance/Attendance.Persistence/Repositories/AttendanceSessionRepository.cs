using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Attendance.Domain.Aggregates;
using SchoolErp.Modules.Attendance.Domain.Repositories;

namespace SchoolErp.Modules.Attendance.Persistence.Repositories;

public sealed class AttendanceSessionRepository : IAttendanceSessionRepository
{
    private readonly AttendanceDbContext _db;

    public AttendanceSessionRepository(AttendanceDbContext db) => _db = db;

    public async Task AddAsync(AttendanceSession entity, CancellationToken cancellationToken = default)
    {
        await _db.AttendanceSessions.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<AttendanceSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.AttendanceSessions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
