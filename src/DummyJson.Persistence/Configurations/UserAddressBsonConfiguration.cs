using DummyJson.Domain.Users;
using MongoDB.Bson.Serialization;

namespace DummyJson.Persistence.Configurations;

public static class UserAddressBsonConfiguration
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(UserAddress)))
        {
            BsonClassMap.RegisterClassMap<UserAddress>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
