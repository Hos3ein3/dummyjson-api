namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Handler for a specific <see cref="IDomainEvent"/>.
/// Resolved from DI by <c>DomainEventDispatcher</c> after a successful <c>SaveChangesAsync</c>.
/// </summary>
/// <typeparam name="TEvent">The concrete domain event type to handle.</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event.
    /// </summary>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
