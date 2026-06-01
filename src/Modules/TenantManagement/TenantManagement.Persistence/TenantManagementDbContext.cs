using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.TenantManagement.Domain.Aggregates;

namespace SchoolErp.Modules.TenantManagement.Persistence;

public sealed class TenantManagementDbContext : BaseDbContext
{
    public TenantManagementDbContext(DbContextOptions<TenantManagementDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants", "TenantManagement");
            e.Property(x => x.Id).HasColumnName("TenantId");
            e.HasKey(x => x.Id);
        });
    }
}
