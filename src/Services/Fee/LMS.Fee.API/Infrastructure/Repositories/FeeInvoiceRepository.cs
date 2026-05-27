using LMS.Fee.API.Domain.Aggregates;
using LMS.Fee.API.Domain.Repositories;
using LMS.Fee.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMS.Fee.API.Infrastructure.Repositories;

public sealed class FeeInvoiceRepository : IFeeInvoiceRepository
{
    private readonly FeeDbContext _db;
    public FeeInvoiceRepository(FeeDbContext db) => _db = db;
    public Task<FeeInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.FeeInvoices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task AddAsync(FeeInvoice entity, CancellationToken cancellationToken = default) =>
        await _db.FeeInvoices.AddAsync(entity, cancellationToken);
    public void Update(FeeInvoice entity) => _db.FeeInvoices.Update(entity);
    public void Remove(FeeInvoice entity) => _db.FeeInvoices.Remove(entity);
}
