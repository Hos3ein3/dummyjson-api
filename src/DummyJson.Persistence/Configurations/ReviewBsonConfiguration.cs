using DummyJson.Domain.Products;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace DummyJson.Persistence.Configurations;

/// <summary>
/// MongoDB BSON class-map configuration for <see cref="ProductReview"/>.
///
/// <para>
/// <see cref="ProductReview"/> is now a standalone MongoDB document in the
/// <c>product_reviews</c> collection. It references its parent product by
/// <see cref="ProductReview.ProductId"/> — a Guid that maps to the PostgreSQL
/// <c>Products.Id</c> column (no FK constraint across stores).
/// </para>
///
/// <para>
/// Register once at startup via <see cref="Register"/>.
/// </para>
/// </summary>
public static class ReviewBsonConfiguration
{
    private static bool _registered;
    private static readonly Lock _lock = new();

    /// <summary>
    /// Registers the BSON class map for <see cref="ProductReview"/>.
    /// Safe to call multiple times — subsequent calls are no-ops.
    /// </summary>
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
            ConventionRegistry.Register("DummyJson.ProductReview", pack, t => t == typeof(ProductReview));

            if (!BsonClassMap.IsClassMapRegistered(typeof(ProductReview)))
            {
                BsonClassMap.RegisterClassMap<ProductReview>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);

                    // Cross-store reference to PostgreSQL Products.Id
                    cm.MapMember(r => r.ProductId)
                        .SetElementName("productId")
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(r => r.ReviewerName).SetElementName("reviewerName");
                    cm.MapMember(r => r.ReviewerEmail).SetElementName("reviewerEmail");
                    cm.MapMember(r => r.Rating).SetElementName("rating");
                    cm.MapMember(r => r.Comment).SetElementName("comment");
                    cm.MapMember(r => r.Date).SetElementName("date")
                        .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
                });
            }

            _registered = true;
        }
    }
}
