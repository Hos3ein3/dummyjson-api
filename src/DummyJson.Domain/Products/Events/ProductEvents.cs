using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Products.Events;

public sealed record ProductCreatedEvent(Guid ProductId, string Title) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public sealed record ProductUpdatedEvent(Guid ProductId, string Title) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public sealed record ProductDeletedEvent(Guid ProductId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
