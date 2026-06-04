using System;
using DummyJson.Application.Common.Events;

namespace DummyJson.Application.Products.Events;

/// <summary>
/// Integration event raised when a Product is created, meant to be published to a message bus (e.g. RabbitMQ/Kafka).
/// </summary>
public sealed record ProductCreatedIntegrationEvent(Guid ProductId, string Title) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
