using DummyJson.Domain.Comments;
using MongoDB.Bson.Serialization;

namespace DummyJson.Persistence.Configurations;

public static class CommentBsonConfiguration
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Comment)))
        {
            BsonClassMap.RegisterClassMap<Comment>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
