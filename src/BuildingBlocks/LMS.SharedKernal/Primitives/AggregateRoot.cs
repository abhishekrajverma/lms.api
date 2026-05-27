namespace LMS.SharedKernal.Primitives;

/// <summary>
/// Marks the root of a DDD Aggregate.
/// All state changes to child entities must go through the aggregate root.
/// Only AggregateRoot types should have a corresponding Repository.
/// Domain events are raised via <see cref="Entity.RaiseDomainEvent"/> inherited from <see cref="Entity"/>.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private int _version = 0;

    public int Version => _version;

    protected AggregateRoot(Guid id) : base(id) { }

    protected AggregateRoot() { }

    /// <summary>
    /// Increments the version for optimistic concurrency tracking.
    /// Call this inside your domain methods when state changes.
    /// </summary>
    protected void IncrementVersion() => _version++;
}
