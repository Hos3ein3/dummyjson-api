using DummyJson.Domain.Products;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace DummyJson.Persistence.Configurations;

public static class ProductImageBsonConfiguration
{
    private static bool _registered;
    private static readonly Lock _lock = new();

    public static void Register()
    {
        lock (_lock)
        {
            if (_registered) return;

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreIfNullConvention(true),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("DummyJson.ProductImage", pack, t => t == typeof(ProductImage));

            if (!BsonClassMap.IsClassMapRegistered(typeof(ProductImage)))
            {
                BsonClassMap.RegisterClassMap<ProductImage>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);

                    // Cross-store reference to PostgreSQL Products.Id
                    cm.MapMember(i => i.ProductId)
                        .SetElementName("productId")
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(i => i.Url).SetElementName("url");
                });
            }

            _registered = true;
        }
    }
}
