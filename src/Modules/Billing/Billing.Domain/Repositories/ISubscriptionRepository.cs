using SchoolErp.Modules.Billing.Domain.Aggregates;

namespace SchoolErp.Modules.Billing.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription entity, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
