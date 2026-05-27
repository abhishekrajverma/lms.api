using LMS.Academic.API.Domain.Aggregates;
using LMS.Academic.API.Infrastructure.Persistence.Configurations;
using LMS.SharedKernal.Persistence;
using LMS.SharedKernal.Primitives;
using Microsoft.EntityFrameworkCore;

namespace LMS.Academic.API.Infrastructure.Persistence;

public sealed class AcademicDbContext : TenantAwareDbContext
{
    public AcademicDbContext(DbContextOptions<AcademicDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext) { }

    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<TeacherAttendanceRecord> TeacherAttendanceRecords => Set<TeacherAttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SchoolClassConfiguration());
        modelBuilder.ApplyConfiguration(new TeacherConfiguration());
        modelBuilder.ApplyConfiguration(new TransportRouteConfiguration());
        modelBuilder.ApplyConfiguration(new PayrollRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ExamScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new ClassSessionConfiguration());
        modelBuilder.ApplyConfiguration(new TeacherAttendanceRecordConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
