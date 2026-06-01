using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Fees.Domain.Aggregates;

public sealed class FeeInvoice : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    

    private FeeInvoice(Guid id, Guid tenantId, string InvoiceNumber)
    {
        Id = id;
        TenantId = tenantId;
        InvoiceNumber = InvoiceNumber;
    }

        public static FeeInvoice Create(Guid tenantId, string InvoiceNumber)
    {
        return new FeeInvoice(Guid.NewGuid(), tenantId, InvoiceNumber);
    }
    
}
