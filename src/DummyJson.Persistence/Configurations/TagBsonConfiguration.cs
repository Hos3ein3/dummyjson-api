using DummyJson.Domain.Tags;
using MongoDB.Bson.Serialization;

namespace DummyJson.Persistence.Configurations;

public static class TagBsonConfiguration
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Tag)))
        {
            BsonClassMap.RegisterClassMap<Tag>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(ProductTag)))
        {
            BsonClassMap.RegisterClassMap<ProductTag>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(PostTag)))
        {
            BsonClassMap.RegisterClassMap<PostTag>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
