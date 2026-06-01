using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SchoolErp.Worker;

public sealed class OutboxDispatchWorker : BackgroundService
{
    private readonly ILogger<OutboxDispatchWorker> _logger;

    public OutboxDispatchWorker(ILogger<OutboxDispatchWorker> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Outbox dispatch heartbeat");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
