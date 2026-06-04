namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Marker interface for integration events — notifications meant for external systems or
/// other bounded contexts. Integration events are dispatched outside the current transaction,
/// typically via a message bus (RabbitMQ, Azure Service Bus, etc.).
/// The default implementation uses an in-memory dispatcher that can be replaced.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Unique identifier for this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp of when the event occurred.</summary>
    DateTimeOffset OccurredOn { get; }

    /// <summary>
    /// Fully-qualified event type name used for routing / deserialization on the consumer side.
    /// </summary>
    string EventType { get; }
}
