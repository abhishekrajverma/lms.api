using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Billing.Domain.Aggregates;

namespace SchoolErp.Modules.Billing.Persistence;

public sealed class BillingDbContext : BaseDbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Subscription>(e =>
        {
            e.ToTable("Subscriptions", "Billing");
            e.HasKey(x => x.Id);
        });
    }
}
