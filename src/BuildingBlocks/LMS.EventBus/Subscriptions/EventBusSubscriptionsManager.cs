using LMS.EventBus.Abstractions;

namespace LMS.EventBus.Subscriptions;

/// <summary>
/// Tracks which handlers are registered for which events.
/// Both EventBusInMemory and EventBusRabbitMQ use this internally.
/// </summary>
public class EventBusSubscriptionsManager
{
    private readonly Dictionary<string, List<Type>> _handlers = new();
    private readonly List<Type> _eventTypes = new();

    public bool IsEmpty => _handlers.Count == 0;

    public event EventHandler<string>? OnEventRemoved;

    public void AddSubscription<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = GetEventKey<TEvent>();

        if (!HasSubscriptionsForEvent(eventName))
            _handlers[eventName] = new List<Type>();

        if (_handlers[eventName].Contains(typeof(THandler)))
            throw new InvalidOperationException(
                $"Handler '{typeof(THandler).Name}' is already registered for '{eventName}'.");

        _handlers[eventName].Add(typeof(THandler));

        if (!_eventTypes.Contains(typeof(TEvent)))
            _eventTypes.Add(typeof(TEvent));
    }

    public void RemoveSubscription<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = GetEventKey<TEvent>();

        if (!HasSubscriptionsForEvent(eventName)) return;

        _handlers[eventName].Remove(typeof(THandler));

        if (_handlers[eventName].Count == 0)
        {
            _handlers.Remove(eventName);
            _eventTypes.RemoveAll(t => t == typeof(TEvent));
            OnEventRemoved?.Invoke(this, eventName);
        }
    }

    public bool HasSubscriptionsForEvent<TEvent>() where TEvent : IIntegrationEvent
        => HasSubscriptionsForEvent(GetEventKey<TEvent>());

    public bool HasSubscriptionsForEvent(string eventName)
        => _handlers.ContainsKey(eventName);

    public IReadOnlyList<Type> GetHandlersForEvent<TEvent>() where TEvent : IIntegrationEvent
        => GetHandlersForEvent(GetEventKey<TEvent>());

    public IReadOnlyList<Type> GetHandlersForEvent(string eventName)
        => _handlers.TryGetValue(eventName, out var handlers)
            ? handlers.AsReadOnly()
            : new List<Type>().AsReadOnly();

    public Type? GetEventTypeByName(string eventName)
        => _eventTypes.SingleOrDefault(t => t.Name == eventName);

    public string GetEventKey<TEvent>() where TEvent : IIntegrationEvent
        => typeof(TEvent).Name;

    public void Clear()
    {
        _handlers.Clear();
        _eventTypes.Clear();
    }
}
