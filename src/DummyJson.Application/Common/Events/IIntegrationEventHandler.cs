using System.Threading;
using System.Threading.Tasks;

namespace DummyJson.Application.Common.Events;

/// <summary>
/// Defines a handler for a specific integration event.
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
{
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
