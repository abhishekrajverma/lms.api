using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subject).HasMaxLength(100);
        builder.Property(x => x.ClassName).HasMaxLength(50);
        builder.Property(x => x.TeacherName).HasMaxLength(200);
        builder.Property(x => x.TimeSlot).HasMaxLength(50);
        builder.Property(x => x.Room).HasMaxLength(50);
    }
}
