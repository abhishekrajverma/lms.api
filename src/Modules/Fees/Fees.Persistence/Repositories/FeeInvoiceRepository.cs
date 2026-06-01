using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Fees.Domain.Aggregates;
using SchoolErp.Modules.Fees.Domain.Repositories;

namespace SchoolErp.Modules.Fees.Persistence.Repositories;

public sealed class FeeInvoiceRepository : IFeeInvoiceRepository
{
    private readonly FeesDbContext _db;

    public FeeInvoiceRepository(FeesDbContext db) => _db = db;

    public async Task AddAsync(FeeInvoice entity, CancellationToken cancellationToken = default)
    {
        await _db.FeeInvoices.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<FeeInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.FeeInvoices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
