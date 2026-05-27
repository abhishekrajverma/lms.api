namespace LMS.EventBus.Abstractions;

/// <summary>
/// Base class for all integration events.
/// Inherit from this in each module:
///
///   public class UserRegisteredIntegrationEvent : IntegrationEvent
///   {
///       public string Email { get; }
///       public UserRegisteredIntegrationEvent(string email) => Email = email;
///   }
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
