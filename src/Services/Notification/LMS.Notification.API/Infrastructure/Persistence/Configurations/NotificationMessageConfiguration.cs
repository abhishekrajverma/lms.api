using LMS.Notification.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Notification.API.Infrastructure.Persistence.Configurations;

public sealed class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
    }
}
