using DummyJson.Domain.Posts;
using DummyJson.Domain.Products;
using DummyJson.Domain.Users;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DummyJson.Persistence.Context;

/// <summary>
/// MongoDB context — holds typed collection references.
/// Products and Posts are stored in MongoDB.
/// </summary>
public sealed class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string 'MongoDB' is not configured.");

        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        Database = client.GetDatabase(mongoUrl.DatabaseName ?? "DummyJsonDb");

        ConfigureCollections();
    }

    public IMongoCollection<Product> Products => Database.GetCollection<Product>("products");
    public IMongoCollection<Post> Posts => Database.GetCollection<Post>("posts");
    public IMongoCollection<UserPreferences> UserPreferences => Database.GetCollection<UserPreferences>("user_preferences");

    private void ConfigureCollections()
    {
        // Create indexes for Products
        var productIndexes = Products.Indexes;
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Category)));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Text(p => p.Title)));

        // Create indexes for Posts
        var postIndexes = Posts.Indexes;
        postIndexes.CreateOne(new CreateIndexModel<Post>(
            Builders<Post>.IndexKeys.Ascending(p => p.UserId)));
    }
}
