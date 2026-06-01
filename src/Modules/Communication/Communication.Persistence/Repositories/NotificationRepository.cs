using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Communication.Domain.Aggregates;
using SchoolErp.Modules.Communication.Domain.Repositories;

namespace SchoolErp.Modules.Communication.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly CommunicationDbContext _db;

    public NotificationRepository(CommunicationDbContext db) => _db = db;

    public async Task AddAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        await _db.Notifications.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
