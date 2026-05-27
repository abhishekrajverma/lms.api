using LMS.Academic.API.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Academic.API.Infrastructure.Persistence.Configurations;

public sealed class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("PayrollRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.BasicSalary).HasPrecision(18, 2);
        builder.Property(x => x.Allowances).HasPrecision(18, 2);
        builder.Property(x => x.Deductions).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(x => x.NetSalary);
    }
}
