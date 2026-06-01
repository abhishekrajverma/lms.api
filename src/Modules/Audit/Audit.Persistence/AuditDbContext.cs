using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Audit.Domain.Aggregates;

namespace SchoolErp.Modules.Audit.Persistence;

public sealed class AuditDbContext : BaseDbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs", "Audit");
            e.HasKey(x => x.Id);
        });
    }
}
