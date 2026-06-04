namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Handler for a specific <see cref="IIntegrationEvent"/>.
/// Resolved from DI by <c>IntegrationEventDispatcher</c>.
/// In a real system, this would be triggered by a message bus consumer.
/// </summary>
/// <typeparam name="TEvent">The concrete integration event type to handle.</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Handles the integration event.
    /// </summary>
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
