using System;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Products.Events;

/// <summary>
/// Domain event raised when a new Product is created.
/// </summary>
public sealed record ProductCreatedDomainEvent(Guid ProductId, string Title) : IDomainEvent
{
    public Guid EventId { get; init; } = IdGenerator.NewId();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
