using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Communication.Domain.Aggregates;

namespace SchoolErp.Modules.Communication.Persistence;

public sealed class CommunicationDbContext : BaseDbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications", "Communication");
            e.HasKey(x => x.Id);
        });
    }
}
