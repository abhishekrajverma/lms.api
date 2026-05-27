using LMS.Tenant.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Tenant.API.Infrastructure.Persistence.Configurations;

public sealed class SchoolTenantConfiguration : IEntityTypeConfiguration<SchoolTenant>
{
    public void Configure(EntityTypeBuilder<SchoolTenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Subdomain).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Subdomain).IsUnique();
        builder.Property(x => x.SchemaName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
