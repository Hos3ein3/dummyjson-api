using DummyJson.Domain.Posts;
using DummyJson.Domain.Products;
using DummyJson.Domain.Recipes;
using DummyJson.Domain.Tags;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Configurations;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace DummyJson.Persistence.Context;

/// <summary>
/// MongoDB context — holds typed collection references.
///
/// <para>
/// <b>After storage migration:</b>
/// <list type="bullet">
///   <item><b>Products</b> moved to PostgreSQL — <c>Products</c> collection removed.</item>
///   <item><b>Posts</b> moved to PostgreSQL — <c>Posts</c> collection removed.</item>
///   <item><b>Recipes</b> moved from PostgreSQL → MongoDB — <c>Recipes</c> collection added.</item>
///   <item><b>ProductReviews</b> promoted from embedded product array → standalone MongoDB collection.</item>
/// </list>
/// </para>
/// </summary>
public sealed class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        // Register base class map for MongoEntity so Id is mapped correctly
        if (!BsonClassMap.IsClassMapRegistered(typeof(DummyJson.Domain.Common.Primitives.MongoEntity)))
        {
            BsonClassMap.RegisterClassMap<DummyJson.Domain.Common.Primitives.MongoEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(e => e.Id)
                  .SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
            });
        }

        // Register all BSON class maps before any MongoDB I/O occurs
        RecipeBsonConfiguration.Register();
        ReviewBsonConfiguration.Register();
        ProductImageBsonConfiguration.Register();
        UserAddressBsonConfiguration.Register();
        TagBsonConfiguration.Register();

        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string 'MongoDB' is not configured.");

        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        Database = client.GetDatabase(mongoUrl.DatabaseName ?? "DummyJsonDb");

        ConfigureCollections();
    }

    // ── Collections ───────────────────────────────────────────────────────────

    /// <summary>Recipes — moved from PostgreSQL to MongoDB.</summary>
    public IMongoCollection<Recipe> Recipes => Database.GetCollection<Recipe>("recipes");

    /// <summary>
    /// Product reviews — promoted from embedded Product array (MongoDB) to
    /// standalone documents now that Product itself lives in PostgreSQL.
    /// </summary>
    public IMongoCollection<ProductReview> ProductReviews => Database.GetCollection<ProductReview>("product_reviews");

    /// <summary>
    /// Product images — standalone MongoDB collection for PostgreSQL products.
    /// </summary>
    public IMongoCollection<ProductImage> ProductImages => Database.GetCollection<ProductImage>("product_images");

    /// <summary>User preferences remain in MongoDB (schema-less per-user settings).</summary>
    public IMongoCollection<UserPreferences> UserPreferences => Database.GetCollection<UserPreferences>("user_preferences");

    /// <summary>User addresses — MongoDB document.</summary>
    public IMongoCollection<UserAddress> UserAddresses => Database.GetCollection<UserAddress>("user_addresses");

    /// <summary>Tags.</summary>
    public IMongoCollection<DummyJson.Domain.Tags.Tag> Tags => Database.GetCollection<DummyJson.Domain.Tags.Tag>("tags");
    
    /// <summary>Product Tags.</summary>
    public IMongoCollection<ProductTag> ProductTags => Database.GetCollection<ProductTag>("product_tags");
    
    /// <summary>Post Tags.</summary>
    public IMongoCollection<PostTag> PostTags => Database.GetCollection<PostTag>("post_tags");

    // ── Index creation ────────────────────────────────────────────────────────

    private void ConfigureCollections()
    {
        try
        {
            // Recipes — index by cuisine and difficulty for common filter queries
            var recipeIndexes = Recipes.Indexes;
            recipeIndexes.CreateOne(new CreateIndexModel<Recipe>(
                Builders<Recipe>.IndexKeys.Ascending(r => r.Cuisine)));
            recipeIndexes.CreateOne(new CreateIndexModel<Recipe>(
                Builders<Recipe>.IndexKeys.Ascending(r => r.Difficulty)));
            recipeIndexes.CreateOne(new CreateIndexModel<Recipe>(
                Builders<Recipe>.IndexKeys.Text(r => r.Name)));

            // Soft-delete filter index — speeds up the common "not deleted" filter
            recipeIndexes.CreateOne(new CreateIndexModel<Recipe>(
                Builders<Recipe>.IndexKeys.Ascending(r => r.IsDeleted)));

            // Product reviews — always queried by product
            var reviewIndexes = ProductReviews.Indexes;
            reviewIndexes.CreateOne(new CreateIndexModel<ProductReview>(
                Builders<ProductReview>.IndexKeys.Ascending(r => r.ProductId)));

            // Product images — always queried by product
            var imageIndexes = ProductImages.Indexes;
            imageIndexes.CreateOne(new CreateIndexModel<ProductImage>(
                Builders<ProductImage>.IndexKeys.Ascending(i => i.ProductId)));

            // User addresses — queried by UserId
            var userAddressIndexes = UserAddresses.Indexes;
            userAddressIndexes.CreateOne(new CreateIndexModel<UserAddress>(
                Builders<UserAddress>.IndexKeys.Ascending(ua => ua.UserId)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to configure MongoDB collections: {ex.Message}");
        }
    }
}
