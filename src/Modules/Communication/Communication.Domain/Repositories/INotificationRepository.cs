using SchoolErp.Modules.Communication.Domain.Aggregates;

namespace SchoolErp.Modules.Communication.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification entity, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
