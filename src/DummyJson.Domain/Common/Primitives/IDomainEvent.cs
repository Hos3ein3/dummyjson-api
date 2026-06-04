namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Marker interface for domain events — things that happened within the domain boundary.
/// Domain events are dispatched in-process, typically after <c>SaveChangesAsync</c>.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp of when the event occurred.</summary>
    DateTimeOffset OccurredOn { get; }
}
