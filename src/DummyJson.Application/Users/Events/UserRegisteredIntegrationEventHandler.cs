using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Events;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Users;
using Microsoft.Extensions.Logging;

namespace DummyJson.Application.Users.Events;

public sealed class UserRegisteredIntegrationEventHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IMongoRepository<UserPreferences> _userPreferencesRepository;
    private readonly ILogger<UserRegisteredIntegrationEventHandler> _logger;

    public UserRegisteredIntegrationEventHandler(
        IMongoRepository<UserPreferences> userPreferencesRepository,
        ILogger<UserRegisteredIntegrationEventHandler> logger)
    {
        _userPreferencesRepository = userPreferencesRepository;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating default UserPreferences for new user {UserId}", integrationEvent.UserId);

        var preferences = UserPreferences.Create(integrationEvent.UserId, "Light", "en-US");
        
        await _userPreferencesRepository.InsertAsync(preferences, cancellationToken);
        
        _logger.LogInformation("Successfully created UserPreferences for {UserId}", integrationEvent.UserId);
    }
}
