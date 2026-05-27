using LMS.Fee.API.Domain.Aggregates;
using LMS.Fee.API.Infrastructure.Persistence.Configurations;
using LMS.SharedKernal.Persistence;
using LMS.SharedKernal.Primitives;
using Microsoft.EntityFrameworkCore;

namespace LMS.Fee.API.Infrastructure.Persistence;

public sealed class FeeDbContext : TenantAwareDbContext
{
    public FeeDbContext(DbContextOptions<FeeDbContext> options, ITenantContext tenantContext) : base(options, tenantContext) { }
    public DbSet<FeeInvoice> FeeInvoices => Set<FeeInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FeeInvoiceConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
