namespace LMS.EventBus.Abstractions;

/// <summary>
/// Implement this in each module to handle an integration event.
///
/// Example:
///   public class OnUserRegistered : IIntegrationEventHandler&lt;UserRegisteredIntegrationEvent&gt;
///   {
///       public Task Handle(UserRegisteredIntegrationEvent @event, CancellationToken ct)
///       {
///           // create student profile, send welcome email, etc.
///       }
///   }
/// </summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
