using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class TransportRouteConfiguration : IEntityTypeConfiguration<TransportRoute>
{
    public void Configure(EntityTypeBuilder<TransportRoute> builder)
    {
        builder.ToTable("TransportRoutes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RouteName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BusNumber).HasMaxLength(50);
        builder.Property(x => x.DriverName).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}
