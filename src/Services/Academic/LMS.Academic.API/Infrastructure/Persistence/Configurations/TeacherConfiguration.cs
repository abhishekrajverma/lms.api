using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.Subject).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Salary).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}
