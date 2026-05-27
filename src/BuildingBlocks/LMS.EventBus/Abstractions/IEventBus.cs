namespace LMS.EventBus.Abstractions;

/// <summary>
/// The core EventBus contract.
/// Two implementations:
///   - EventBusInMemory  → used in development and tests
///   - EventBusRabbitMQ  → used in production via MassTransit
///
/// Register in DI:
///   // Dev
///   services.AddSingleton&lt;IEventBus, EventBusInMemory&gt;();
///
///   // Prod
///   services.AddSingleton&lt;IEventBus, EventBusRabbitMQ&gt;();
/// </summary>
public interface IEventBus
{
    /// <summary>Publish an event to all registered handlers.</summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    /// <summary>Register a handler for a specific event type.</summary>
    void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    /// <summary>Unregister a handler for a specific event type.</summary>
    void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}
