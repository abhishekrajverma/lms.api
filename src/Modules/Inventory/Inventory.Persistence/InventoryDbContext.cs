using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Inventory.Domain.Aggregates;

namespace SchoolErp.Modules.Inventory.Persistence;

public sealed class InventoryDbContext : BaseDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("InventoryItems", "Inventory");
            e.HasKey(x => x.Id);
        });
    }
}
