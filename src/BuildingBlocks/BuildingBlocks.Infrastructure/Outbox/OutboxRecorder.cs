using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Domain;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;

namespace SchoolErp.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxRecorder
{
    private readonly BaseDbContext _db;

    public OutboxRecorder(BaseDbContext db) => _db = db;

    public async Task RecordAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var evt in events)
        {
            _db.OutboxMessages.Add(new OutboxMessage
            {
                Type = evt.GetType().FullName ?? evt.GetType().Name,
                Payload = JsonSerializer.Serialize(evt),
                OccurredOnUtc = evt.OccurredOnUtc
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
