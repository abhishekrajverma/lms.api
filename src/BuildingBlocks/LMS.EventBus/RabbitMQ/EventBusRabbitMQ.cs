using LMS.EventBus.Abstractions;
using LMS.EventBus.Subscriptions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LMS.EventBus.RabbitMQ;

/// <summary>
/// Production EventBus backed by RabbitMQ via MassTransit.
/// MassTransit handles connection pooling, retries, and dead-letter queues.
///
/// Register in Program.cs:
///   services.AddMassTransit(x =>
///   {
///       x.UsingRabbitMq((ctx, cfg) =>
///       {
///           cfg.Host(configuration["RabbitMQ:Host"]);
///           cfg.ConfigureEndpoints(ctx);
///       });
///   });
///   services.AddSingleton&lt;IEventBus, EventBusRabbitMQ&gt;();
/// </summary>
public sealed class EventBusRabbitMQ : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventBusSubscriptionsManager _subscriptionsManager;
    private readonly ILogger<EventBusRabbitMQ> _logger;

    public EventBusRabbitMQ(
        IPublishEndpoint publishEndpoint,
        EventBusSubscriptionsManager subscriptionsManager,
        ILogger<EventBusRabbitMQ> logger)
    {
        _publishEndpoint = publishEndpoint;
        _subscriptionsManager = subscriptionsManager;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        _logger.LogInformation(
            "[EventBus:RabbitMQ] Publishing event {EventName} ({EventId})",
            @event.EventType, @event.Id);

        try
        {
            await _publishEndpoint.Publish(@event, cancellationToken);

            _logger.LogInformation(
                "[EventBus:RabbitMQ] Published {EventName} successfully",
                @event.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[EventBus:RabbitMQ] Failed to publish {EventName} ({EventId})",
                @event.EventType, @event.Id);
            throw;
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _logger.LogInformation(
            "[EventBus:RabbitMQ] Subscribing {HandlerType} to {EventName}",
            typeof(THandler).Name, typeof(TEvent).Name);

        // MassTransit handles consumer registration at startup via AddConsumer<THandler>()
        // This method keeps the subscription manager in sync for introspection/tooling.
        _subscriptionsManager.AddSubscription<TEvent, THandler>();
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _logger.LogInformation(
            "[EventBus:RabbitMQ] Unsubscribing {HandlerType} from {EventName}",
            typeof(THandler).Name, typeof(TEvent).Name);

        _subscriptionsManager.RemoveSubscription<TEvent, THandler>();
    }
}
