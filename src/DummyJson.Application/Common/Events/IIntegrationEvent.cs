using System;

namespace DummyJson.Application.Common.Events;

/// <summary>
/// Marker interface for integration events.
/// Integration events are used to communicate between different microservices or bounded contexts asynchronously.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
