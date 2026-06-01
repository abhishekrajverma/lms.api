using SchoolErp.Modules.Fees.Domain.Aggregates;

namespace SchoolErp.Modules.Fees.Domain.Repositories;

public interface IFeeInvoiceRepository
{
    Task AddAsync(FeeInvoice entity, CancellationToken cancellationToken = default);
    Task<FeeInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
