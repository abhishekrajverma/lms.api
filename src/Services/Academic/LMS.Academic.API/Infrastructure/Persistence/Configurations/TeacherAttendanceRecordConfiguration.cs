using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class TeacherAttendanceRecordConfiguration : IEntityTypeConfiguration<TeacherAttendanceRecord>
{
    public void Configure(EntityTypeBuilder<TeacherAttendanceRecord> builder)
    {
        builder.ToTable("TeacherAttendanceRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TeacherName).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}
