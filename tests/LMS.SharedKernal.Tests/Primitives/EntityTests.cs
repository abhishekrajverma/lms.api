using Xunit;
using LMS.SharedKernal.Primitives;
using FluentAssertions;

namespace LMS.SharedKernal.Tests.Primitives;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }
        public void Raise(IDomainEvent e) => RaiseDomainEvent(e);
    }

    private class TestDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    [Fact]
    public void Constructor_WithValidId_SetsId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var act = () => new TestEntity(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Entity Id cannot be empty*");
    }

    [Fact]
    public void Equals_SameId_SameType_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var e1 = new TestEntity(id);
        var e2 = new TestEntity(id);
        e1.Should().Be(e2);
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var e1 = new TestEntity(Guid.NewGuid());
        var e2 = new TestEntity(Guid.NewGuid());
        e1.Should().NotBe(e2);
    }

    [Fact]
    public void RaiseDomainEvent_AddsEventToCollection()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Raise(new TestDomainEvent());
        entity.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Raise(new TestDomainEvent());
        entity.Raise(new TestDomainEvent());
        entity.ClearDomainEvents();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void HasDomainEvents_WithEvents_ReturnsTrue()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Raise(new TestDomainEvent());
        entity.HasDomainEvents().Should().BeTrue();
    }

    [Fact]
    public void RaiseDomainEvent_WithNull_ThrowsArgumentNullException()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var act = () => entity.Raise(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
