using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Billing.Domain.Aggregates;
using SchoolErp.Modules.Billing.Domain.Repositories;

namespace SchoolErp.Modules.Billing.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _db;

    public SubscriptionRepository(BillingDbContext db) => _db = db;

    public async Task AddAsync(Subscription entity, CancellationToken cancellationToken = default)
    {
        await _db.Subscriptions.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Subscriptions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
