using LMS.SharedKernal.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.Student.API.Domain.Aggregates;
using LMS.Student.API.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Infrastructure.Persistence;

public sealed class StudentDbContext : TenantAwareDbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext) { }

    public DbSet<StudentProfile> Students => Set<StudentProfile>();
    public DbSet<DailyAttendanceSnapshot> DailyAttendanceSnapshots => Set<DailyAttendanceSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StudentProfileConfiguration());
        modelBuilder.ApplyConfiguration(new DailyAttendanceSnapshotConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
