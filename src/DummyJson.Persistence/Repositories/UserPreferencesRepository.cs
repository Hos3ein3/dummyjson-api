using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;

using MongoDB.Driver;

namespace DummyJson.Persistence.Repositories;

public sealed class UserPreferencesRepository : MongoRepository<UserPreferences>, IUserPreferencesRepository
{
    public UserPreferencesRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<UserPreferences> GetOrSetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserPreferences>.Filter.Eq(x => x.UserId, userId);
        var existing = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var newPreferences = UserPreferences.Create(userId);
        await _collection.InsertOneAsync(newPreferences, null, cancellationToken);
        return newPreferences;
    }
}
