using LMS.Student.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Student.API.Infrastructure.Persistence.Configurations;

public sealed class DailyAttendanceSnapshotConfiguration : IEntityTypeConfiguration<DailyAttendanceSnapshot>
{
    public void Configure(EntityTypeBuilder<DailyAttendanceSnapshot> builder)
    {
        builder.ToTable("DailyAttendanceSnapshots");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.Date }).IsUnique();
    }
}
