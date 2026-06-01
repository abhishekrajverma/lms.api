using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolErp.BuildingBlocks.Infrastructure.EventBus;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;

namespace SchoolErp.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxProcessorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitPublisher>();

                var pending = await db.OutboxMessages
                    .Where(x => x.ProcessedOnUtc == null)
                    .OrderBy(x => x.OccurredOnUtc)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var message in pending)
                {
                    await publisher.PublishAsync(message.Type, message.Payload, stoppingToken);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                }

                if (pending.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processor error");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
