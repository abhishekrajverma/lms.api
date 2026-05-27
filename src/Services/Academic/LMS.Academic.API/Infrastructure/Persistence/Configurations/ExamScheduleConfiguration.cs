using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class ExamScheduleConfiguration : IEntityTypeConfiguration<ExamSchedule>
{
    public void Configure(EntityTypeBuilder<ExamSchedule> builder)
    {
        builder.ToTable("ExamSchedules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subject).HasMaxLength(100);
        builder.Property(x => x.ClassName).HasMaxLength(50);
    }
}
