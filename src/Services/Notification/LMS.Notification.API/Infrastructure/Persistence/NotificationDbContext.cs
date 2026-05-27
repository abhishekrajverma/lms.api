using LMS.Notification.API.Domain.Aggregates;
using LMS.Notification.API.Infrastructure.Persistence.Configurations;
using LMS.SharedKernal.Persistence;
using LMS.SharedKernal.Primitives;
using Microsoft.EntityFrameworkCore;

namespace LMS.Notification.API.Infrastructure.Persistence;

public sealed class NotificationDbContext : TenantAwareDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext) { }

    public DbSet<NotificationMessage> Notifications => Set<NotificationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new NotificationMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
