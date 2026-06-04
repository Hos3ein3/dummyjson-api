using DummyJson.Domain.Common.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using DummyJson.Application.Common.Events;

namespace DummyJson.Infrastructure.Events;

/// <summary>
/// In-process domain event dispatcher.
/// Resolves all registered <see cref="IDomainEventHandler{T}"/> implementations from DI
/// and invokes them sequentially after a successful SaveChanges.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Dispatches all domain events.
    /// Call after <c>SaveChangesAsync</c> succeeds.
    /// </summary>
    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            _logger.LogDebug("Dispatching domain event {EventType} ({EventId})",
                domainEvent.GetType().Name, domainEvent.EventId);

            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                try
                {
                    var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
                    await (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling domain event {EventType}", domainEvent.GetType().Name);
                    // Domain event handlers should not fail silently in production — consider an outbox pattern
                    throw;
                }
            }
        }
    }
}
