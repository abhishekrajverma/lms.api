namespace LMS.EventBus.Abstractions;

/// <summary>
/// Marker interface for events that cross module boundaries.
/// Unlike DomainEvents (which stay inside a module),
/// IntegrationEvents are published to the EventBus and consumed by other modules.
///
/// Example flow:
///   Identity module raises UserRegisteredDomainEvent
///   → DomainEventHandler publishes UserRegisteredIntegrationEvent to EventBus
///   → Students module subscribes and creates a student profile
/// </summary>
public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
