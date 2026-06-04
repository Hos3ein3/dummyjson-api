using System;
using DummyJson.Application.Common.Events;

namespace DummyJson.Application.Users.Events;

public sealed record UserRegisteredIntegrationEvent(Guid UserId, string Email) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
