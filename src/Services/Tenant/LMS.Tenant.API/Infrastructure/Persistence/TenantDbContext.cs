using LMS.SharedKernal.Primitives;
using LMS.Tenant.API.Domain.Aggregates;
using LMS.Tenant.API.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tenant.API.Infrastructure.Persistence;

public sealed class TenantDbContext : DbContext, IUnitOfWork, LMS.SharedKernal.Outbox.IOutboxDbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<SchoolTenant> Tenants => Set<SchoolTenant>();
    public DbSet<LMS.SharedKernal.Outbox.OutboxMessage> OutboxMessages => Set<LMS.SharedKernal.Outbox.OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SchoolTenantConfiguration());
        modelBuilder.Entity<LMS.SharedKernal.Outbox.OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.HasIndex(x => x.ProcessedOnUtc);
        });
        base.OnModelCreating(modelBuilder);
    }
}
