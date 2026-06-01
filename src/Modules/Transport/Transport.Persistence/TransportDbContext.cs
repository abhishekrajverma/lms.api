using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Transport.Domain.Aggregates;

namespace SchoolErp.Modules.Transport.Persistence;

public sealed class TransportDbContext : BaseDbContext
{
    public TransportDbContext(DbContextOptions<TransportDbContext> options) : base(options) { }

    public DbSet<Route> Routes => Set<Route>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Route>(e =>
        {
            e.ToTable("Routes", "Transport");
            e.HasKey(x => x.Id);
        });
    }
}
