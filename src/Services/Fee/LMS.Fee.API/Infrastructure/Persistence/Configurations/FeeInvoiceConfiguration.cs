using LMS.Fee.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Fee.API.Infrastructure.Persistence.Configurations;

public sealed class FeeInvoiceConfiguration : IEntityTypeConfiguration<FeeInvoice>
{
    public void Configure(EntityTypeBuilder<FeeInvoice> builder)
    {
        builder.ToTable("FeeInvoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ClassName).HasMaxLength(50);
        builder.Property(x => x.TotalFee).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Ignore(x => x.PendingAmount);
        builder.Ignore(x => x.Status);
    }
}
