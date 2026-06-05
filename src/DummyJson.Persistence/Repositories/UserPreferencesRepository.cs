using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;

namespace DummyJson.Persistence.Repositories;

public sealed class UserPreferencesRepository : MongoRepository<UserPreferences>, IUserPreferencesRepository
{
    public UserPreferencesRepository(MongoDbContext context) : base(context)
    {
    }
}
