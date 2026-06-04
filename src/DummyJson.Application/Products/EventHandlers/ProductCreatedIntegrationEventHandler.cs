
using DummyJson.Application.Common.Events;
using DummyJson.Application.Products.Events;
using Microsoft.Extensions.Logging;

namespace DummyJson.Application.Products.EventHandlers;

/// <summary>
/// Sample handler for the ProductCreatedIntegrationEvent.
/// </summary>
public sealed class ProductCreatedIntegrationEventHandler : IIntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    private readonly ILogger<ProductCreatedIntegrationEventHandler> _logger;

    public ProductCreatedIntegrationEventHandler(ILogger<ProductCreatedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ProductCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        // Example: This would normally consume from RabbitMQ/MassTransit.
        // Or if publishing, this might be triggered to forward a Domain Event to an Integration Message Bus.
        _logger.LogInformation(
            "INTEGRATION EVENT HANDLED: Product created with ID: {ProductId}. Occurred at: {OccurredOn}", 
            integrationEvent.ProductId, 
            integrationEvent.OccurredOn);

        return Task.CompletedTask;
    }
}
