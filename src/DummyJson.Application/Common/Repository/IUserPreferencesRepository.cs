using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Users;

namespace DummyJson.Application.Common.Repository;

public interface IUserPreferencesRepository : IMongoRepository<UserPreferences>
{
    Task<UserPreferences> GetOrSetAsync(Guid userId, CancellationToken cancellationToken = default);
}
