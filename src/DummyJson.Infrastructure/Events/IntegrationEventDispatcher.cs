using DummyJson.Application.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DummyJson.Infrastructure.Events;

/// <summary>
/// Stub in-memory integration event dispatcher.
/// Replace the dispatch logic with your message bus (RabbitMQ, Azure Service Bus, etc.)
/// by swapping this implementation or adding a proper outbox pattern.
/// </summary>
public sealed class IntegrationEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IntegrationEventDispatcher> _logger;

    public IntegrationEventDispatcher(IServiceProvider serviceProvider, ILogger<IntegrationEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dispatching integration event {EventType} ({EventId})",
            integrationEvent.GetType().Name, integrationEvent.EventId);

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null) continue;
            try
            {
                var method = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!;
                await (Task)method.Invoke(handler, [integrationEvent, cancellationToken])!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling integration event {EventType}", integrationEvent.GetType().Name);
                // In production: push to dead-letter queue or retry mechanism
                throw;
            }
        }
    }
}
