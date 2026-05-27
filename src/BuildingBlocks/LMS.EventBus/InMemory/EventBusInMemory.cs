using LMS.EventBus.Abstractions;
using LMS.EventBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.EventBus.InMemory;

/// <summary>
/// In-memory EventBus for development and testing.
/// Events are dispatched synchronously within the same process.
/// No broker (RabbitMQ) needed.
///
/// Register:
///   services.AddSingleton&lt;IEventBus, EventBusInMemory&gt;();
/// </summary>
public sealed class EventBusInMemory : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusSubscriptionsManager _subscriptionsManager;
    private readonly ILogger<EventBusInMemory> _logger;

    public EventBusInMemory(
        IServiceProvider serviceProvider,
        EventBusSubscriptionsManager subscriptionsManager,
        ILogger<EventBusInMemory> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriptionsManager = subscriptionsManager;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var eventName = @event.GetType().Name;

        _logger.LogInformation(
            "[EventBus:InMemory] Publishing event {EventName} ({EventId})",
            eventName, @event.Id);

        if (!_subscriptionsManager.HasSubscriptionsForEvent<TEvent>())
        {
            _logger.LogWarning(
                "[EventBus:InMemory] No subscribers for event {EventName}", eventName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handlerTypes = _subscriptionsManager.GetHandlersForEvent<TEvent>();

        foreach (var handlerType in handlerTypes)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is null)
            {
                _logger.LogWarning(
                    "[EventBus:InMemory] Handler {HandlerType} not registered in DI container",
                    handlerType.Name);
                continue;
            }

            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(@event.GetType());
            await (Task)concreteType
                .GetMethod(nameof(IIntegrationEventHandler<TEvent>.Handle))!
                .Invoke(handler, new object[] { @event, cancellationToken })!;

            _logger.LogInformation(
                "[EventBus:InMemory] Handled {EventName} with {HandlerType}",
                eventName, handlerType.Name);
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _logger.LogInformation(
            "[EventBus:InMemory] Subscribing {HandlerType} to {EventName}",
            typeof(THandler).Name, typeof(TEvent).Name);

        _subscriptionsManager.AddSubscription<TEvent, THandler>();
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _logger.LogInformation(
            "[EventBus:InMemory] Unsubscribing {HandlerType} from {EventName}",
            typeof(THandler).Name, typeof(TEvent).Name);

        _subscriptionsManager.RemoveSubscription<TEvent, THandler>();
    }
}
