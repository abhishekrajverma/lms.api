using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("Classes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Grade).HasMaxLength(20).IsRequired();
    }
}
