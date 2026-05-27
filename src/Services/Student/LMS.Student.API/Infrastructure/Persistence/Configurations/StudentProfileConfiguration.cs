using LMS.Student.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Student.API.Infrastructure.Persistence.Configurations;

public sealed class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AdmissionNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ClassName).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.FeeStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.AttendancePercentage).HasPrecision(5, 2);
        builder.HasIndex(x => new { x.TenantId, x.AdmissionNumber }).IsUnique();
    }
}
