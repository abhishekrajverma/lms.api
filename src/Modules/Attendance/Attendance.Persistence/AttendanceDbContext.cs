using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Attendance.Domain.Aggregates;

namespace SchoolErp.Modules.Attendance.Persistence;

public sealed class AttendanceDbContext : BaseDbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : base(options) { }

    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AttendanceSession>(e =>
        {
            e.ToTable("AttendanceSessions", "Attendance");
            e.HasKey(x => x.Id);
        });
    }
}
