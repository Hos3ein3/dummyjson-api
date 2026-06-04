using System.Threading;
using System.Threading.Tasks;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Products.Events;
using Microsoft.Extensions.Logging;

namespace DummyJson.Application.Products.EventHandlers;

/// <summary>
/// Sample handler for the ProductCreatedDomainEvent.
/// </summary>
public sealed class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    public ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ProductCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Example: Send an email, clear cache, notify other bounded contexts, etc.
        _logger.LogInformation(
            "DOMAIN EVENT HANDLED: Product created with ID: {ProductId} and Title: {Title}. Occurred at: {OccurredOn}", 
            domainEvent.ProductId, 
            domainEvent.Title, 
            domainEvent.OccurredOn);

        return Task.CompletedTask;
    }
}
