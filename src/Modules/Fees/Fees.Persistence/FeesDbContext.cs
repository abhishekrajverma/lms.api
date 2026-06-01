using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Fees.Domain.Aggregates;

namespace SchoolErp.Modules.Fees.Persistence;

public sealed class FeesDbContext : BaseDbContext
{
    public FeesDbContext(DbContextOptions<FeesDbContext> options) : base(options) { }

    public DbSet<FeeInvoice> FeeInvoices => Set<FeeInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<FeeInvoice>(e =>
        {
            e.ToTable("FeeInvoices", "Fees");
            e.HasKey(x => x.Id);
        });
    }
}
