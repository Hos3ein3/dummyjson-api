using DummyJson.Domain.Quotes;
using MongoDB.Bson.Serialization;

namespace DummyJson.Persistence.Configurations;

public static class QuoteBsonConfiguration
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Quote)))
        {
            BsonClassMap.RegisterClassMap<Quote>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
