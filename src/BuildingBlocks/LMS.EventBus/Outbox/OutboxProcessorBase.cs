using System.Text.Json;
using LMS.EventBus.Abstractions;
using LMS.SharedKernal.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LMS.EventBus.Outbox;

public abstract class OutboxProcessorBase<TContext> : BackgroundService
    where TContext : DbContext, IOutboxDbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    protected OutboxProcessorBase(IServiceScopeFactory scopeFactory, ILogger logger)
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
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processing failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = ResolveType(message.Type);
                if (eventType is null)
                {
                    message.MarkFailed($"Unknown event type: {message.Type}");
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
                if (integrationEvent is IIntegrationEvent evt)
                    await eventBus.PublishAsync(evt, cancellationToken);

                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);
                _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            }
        }

        if (messages.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }

    private static Type? ResolveType(string typeName)
    {
        return Type.GetType(typeName)
               ?? AppDomain.CurrentDomain.GetAssemblies()
                   .Select(a => a.GetType(typeName))
                   .FirstOrDefault(t => t is not null);
    }
}
