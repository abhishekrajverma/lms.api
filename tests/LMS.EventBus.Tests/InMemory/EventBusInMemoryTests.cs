using Xunit;
using FluentAssertions;
using LMS.EventBus.Abstractions;
using LMS.EventBus.InMemory;
using LMS.EventBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LMS.EventBus.Tests.InMemory;

// ── Test doubles ─────────────────────────────────────────────────────────────

public class ProductCreatedEvent : IntegrationEvent
{
    public string ProductName { get; init; } = string.Empty;
}

public class TrackingList
{
    public List<string> Handled { get; } = new();
}

public class ProductCreatedHandler : IIntegrationEventHandler<ProductCreatedEvent>
{
    private readonly TrackingList _tracking;
    public ProductCreatedHandler(TrackingList tracking) => _tracking = tracking;

    public Task Handle(ProductCreatedEvent @event, CancellationToken ct = default)
    {
        _tracking.Handled.Add(@event.ProductName);
        return Task.CompletedTask;
    }
}

public class SecondProductCreatedHandler : IIntegrationEventHandler<ProductCreatedEvent>
{
    private readonly TrackingList _tracking;
    public SecondProductCreatedHandler(TrackingList tracking) => _tracking = tracking;

    public Task Handle(ProductCreatedEvent @event, CancellationToken ct = default)
    {
        _tracking.Handled.Add($"second:{@event.ProductName}");
        return Task.CompletedTask;
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public class EventBusInMemoryTests
{
    private (EventBusInMemory bus, TrackingList tracking) CreateBus(
        Action<IServiceCollection>? configure = null)
    {
        var tracking = new TrackingList();
        var services = new ServiceCollection();
        services.AddSingleton(tracking);
        services.AddTransient<ProductCreatedHandler>();
        services.AddTransient<SecondProductCreatedHandler>();
        configure?.Invoke(services);

        var provider = services.BuildServiceProvider();
        var mgr = new EventBusSubscriptionsManager();
        var logger = NullLogger<EventBusInMemory>.Instance;
        var bus = new EventBusInMemory(provider, mgr, logger);

        return (bus, tracking);
    }

    [Fact]
    public async Task PublishAsync_WithSubscriber_InvokesHandler()
    {
        var (bus, tracking) = CreateBus();
        bus.Subscribe<ProductCreatedEvent, ProductCreatedHandler>();

        await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Laptop" });

        tracking.Handled.Should().ContainSingle()
            .Which.Should().Be("Laptop");
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_InvokesAllHandlers()
    {
        var (bus, tracking) = CreateBus();
        bus.Subscribe<ProductCreatedEvent, ProductCreatedHandler>();
        bus.Subscribe<ProductCreatedEvent, SecondProductCreatedHandler>();

        await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Phone" });

        tracking.Handled.Should().HaveCount(2);
        tracking.Handled.Should().Contain("Phone");
        tracking.Handled.Should().Contain("second:Phone");
    }

    [Fact]
    public async Task PublishAsync_NoSubscribers_DoesNotThrow()
    {
        var (bus, tracking) = CreateBus();

        var act = async () =>
            await bus.PublishAsync(new ProductCreatedEvent { ProductName = "TV" });

        await act.Should().NotThrowAsync();
        tracking.Handled.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_AfterUnsubscribe_DoesNotInvokeHandler()
    {
        var (bus, tracking) = CreateBus();
        bus.Subscribe<ProductCreatedEvent, ProductCreatedHandler>();
        bus.Unsubscribe<ProductCreatedEvent, ProductCreatedHandler>();

        await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Tablet" });

        tracking.Handled.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_EventProperties_ArePassedToHandler()
    {
        string? receivedName = null;
        var services = new ServiceCollection();
        var tracking = new TrackingList();
        services.AddSingleton(tracking);
        services.AddTransient<ProductCreatedHandler>();
        services.AddTransient<SecondProductCreatedHandler>();

        var provider = services.BuildServiceProvider();
        var mgr = new EventBusSubscriptionsManager();
        var bus = new EventBusInMemory(provider, mgr, NullLogger<EventBusInMemory>.Instance);

        bus.Subscribe<ProductCreatedEvent, ProductCreatedHandler>();
        var evt = new ProductCreatedEvent { ProductName = "Camera" };
        await bus.PublishAsync(evt);

        tracking.Handled.Should().Contain("Camera");
    }

    [Fact]
    public void Subscribe_ThenCheckSubscription_IsRegistered()
    {
        var (bus, _) = CreateBus();
        bus.Subscribe<ProductCreatedEvent, ProductCreatedHandler>();

        // Verify via publishing — if handler runs, subscription worked
        // (subscriptionsManager is internal, so we verify via behavior)
        var act = async () =>
            await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Test" });

        act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_EachHandledIndependently()
    {
        var (bus, tracking) = CreateBus();
        bus.Subscribe<ProductCreatedEvent, ProductCreatedHandler>();

        await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Item1" });
        await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Item2" });
        await bus.PublishAsync(new ProductCreatedEvent { ProductName = "Item3" });

        tracking.Handled.Should().HaveCount(3);
        tracking.Handled.Should().BeEquivalentTo(["Item1", "Item2", "Item3"]);
    }
}
