namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Aggregate root — owns the consistency boundary for a DDD aggregate.
/// Inherits domain-event support from <see cref="Entity{TId}"/>.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : struct
{
    protected AggregateRoot(TId id) : base(id) { }

    // Required by EF Core
    protected AggregateRoot() { }

    /// <summary>
    /// Concurrency token for optimistic locking (supported by EF Core via IsRowVersion / xmin).
    /// </summary>
    public uint Version { get; private set; }
}
