using Xunit;
using FluentAssertions;
using LMS.EventBus.Abstractions;

namespace LMS.EventBus.Tests;

public class TestEvent : IntegrationEvent
{
    public string Payload { get; init; } = string.Empty;
}

public class IntegrationEventTests
{
    [Fact]
    public void NewEvent_HasNonEmptyId()
    {
        var evt = new TestEvent();
        evt.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void NewEvent_HasOccurredOnSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new TestEvent();
        var after = DateTime.UtcNow.AddSeconds(1);

        evt.OccurredOn.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void NewEvent_EventType_IsClassName()
    {
        var evt = new TestEvent();
        evt.EventType.Should().Be("TestEvent");
    }

    [Fact]
    public void TwoEvents_HaveDifferentIds()
    {
        var e1 = new TestEvent();
        var e2 = new TestEvent();
        e1.Id.Should().NotBe(e2.Id);
    }
}
