using System.Reflection;
using LMS.SharedKernal.Outbox;
using LMS.SharedKernal.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LMS.SharedKernal.Persistence;

/// <summary>
/// Base DbContext that applies global tenant query filters and stamps TenantId on writes.
/// </summary>
public abstract class TenantAwareDbContext : DbContext, IUnitOfWork, Outbox.IOutboxDbContext
{
    private readonly ITenantContext _tenantContext;

    protected TenantAwareDbContext(DbContextOptions options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.HasIndex(x => x.ProcessedOnUtc);
        });

        ApplyTenantQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenantIdOnNewEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(TenantAwareDbContext)
                .GetMethod(nameof(ConfigureTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void ConfigureTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IHasTenant
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
    }

    private void StampTenantIdOnNewEntities()
    {
        if (!_tenantContext.IsResolved)
            return;

        foreach (var entry in ChangeTracker.Entries<TenantAggregateRoot>()
                     .Where(e => e.State == EntityState.Added))
        {
            entry.Entity.SetTenantId(_tenantContext.TenantId);
        }
    }
}
