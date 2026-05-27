using Xunit;
using FluentAssertions;
using LMS.EventBus.Abstractions;
using LMS.EventBus.Subscriptions;

namespace LMS.EventBus.Tests.Subscriptions;

// ── Test doubles ────────────────────────────────────────────────────────────

public class OrderPlacedEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;
}

public class UserRegisteredEvent : IntegrationEvent
{
    public string Email { get; init; } = string.Empty;
}

public class OrderPlacedHandler : IIntegrationEventHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent @event, CancellationToken ct = default) => Task.CompletedTask;
}

public class AnotherOrderPlacedHandler : IIntegrationEventHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent @event, CancellationToken ct = default) => Task.CompletedTask;
}

public class UserRegisteredHandler : IIntegrationEventHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent @event, CancellationToken ct = default) => Task.CompletedTask;
}

// ── Tests ────────────────────────────────────────────────────────────────────

public class EventBusSubscriptionsManagerTests
{
    private EventBusSubscriptionsManager CreateManager() => new();

    [Fact]
    public void IsEmpty_WhenNoSubscriptions_ReturnsTrue()
    {
        var mgr = CreateManager();
        mgr.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_AfterSubscription_ReturnsFalse()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void AddSubscription_RegistersHandler()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.HasSubscriptionsForEvent<OrderPlacedEvent>().Should().BeTrue();
    }

    [Fact]
    public void AddSubscription_MultipleHandlers_RegistersBoth()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.AddSubscription<OrderPlacedEvent, AnotherOrderPlacedHandler>();

        var handlers = mgr.GetHandlersForEvent<OrderPlacedEvent>();
        handlers.Should().HaveCount(2);
        handlers.Should().Contain(typeof(OrderPlacedHandler));
        handlers.Should().Contain(typeof(AnotherOrderPlacedHandler));
    }

    [Fact]
    public void AddSubscription_DuplicateHandler_ThrowsInvalidOperationException()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();

        var act = () => mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public void RemoveSubscription_RemovesHandler()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.RemoveSubscription<OrderPlacedEvent, OrderPlacedHandler>();

        mgr.HasSubscriptionsForEvent<OrderPlacedEvent>().Should().BeFalse();
    }

    [Fact]
    public void RemoveSubscription_LastHandler_RaisesOnEventRemovedEvent()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();

        string? removedEvent = null;
        mgr.OnEventRemoved += (_, eventName) => removedEvent = eventName;

        mgr.RemoveSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        removedEvent.Should().Be("OrderPlacedEvent");
    }

    [Fact]
    public void RemoveSubscription_OneOfTwoHandlers_DoesNotRaiseOnEventRemoved()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.AddSubscription<OrderPlacedEvent, AnotherOrderPlacedHandler>();

        bool eventFired = false;
        mgr.OnEventRemoved += (_, _) => eventFired = true;

        mgr.RemoveSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        eventFired.Should().BeFalse();
        mgr.HasSubscriptionsForEvent<OrderPlacedEvent>().Should().BeTrue();
    }

    [Fact]
    public void RemoveSubscription_NonExistentHandler_DoesNotThrow()
    {
        var mgr = CreateManager();
        var act = () => mgr.RemoveSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetHandlersForEvent_NoHandlers_ReturnsEmptyList()
    {
        var mgr = CreateManager();
        var handlers = mgr.GetHandlersForEvent<OrderPlacedEvent>();
        handlers.Should().BeEmpty();
    }

    [Fact]
    public void HasSubscriptionsForEvent_ByString_Works()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.HasSubscriptionsForEvent("OrderPlacedEvent").Should().BeTrue();
        mgr.HasSubscriptionsForEvent("UnknownEvent").Should().BeFalse();
    }

    [Fact]
    public void GetEventTypeByName_ReturnsCorrectType()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        var type = mgr.GetEventTypeByName("OrderPlacedEvent");
        type.Should().Be(typeof(OrderPlacedEvent));
    }

    [Fact]
    public void GetEventTypeByName_UnknownEvent_ReturnsNull()
    {
        var mgr = CreateManager();
        var type = mgr.GetEventTypeByName("NonExistentEvent");
        type.Should().BeNull();
    }

    [Fact]
    public void GetEventKey_ReturnsTypeName()
    {
        var mgr = CreateManager();
        mgr.GetEventKey<OrderPlacedEvent>().Should().Be("OrderPlacedEvent");
    }

    [Fact]
    public void Clear_RemovesAllSubscriptions()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.AddSubscription<UserRegisteredEvent, UserRegisteredHandler>();
        mgr.Clear();
        mgr.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void MultipleEventTypes_ManagedIndependently()
    {
        var mgr = CreateManager();
        mgr.AddSubscription<OrderPlacedEvent, OrderPlacedHandler>();
        mgr.AddSubscription<UserRegisteredEvent, UserRegisteredHandler>();

        mgr.HasSubscriptionsForEvent<OrderPlacedEvent>().Should().BeTrue();
        mgr.HasSubscriptionsForEvent<UserRegisteredEvent>().Should().BeTrue();

        mgr.RemoveSubscription<OrderPlacedEvent, OrderPlacedHandler>();

        mgr.HasSubscriptionsForEvent<OrderPlacedEvent>().Should().BeFalse();
        mgr.HasSubscriptionsForEvent<UserRegisteredEvent>().Should().BeTrue();
    }
}
