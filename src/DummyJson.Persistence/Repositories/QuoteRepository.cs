using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Quotes;
using DummyJson.Persistence.Context;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="IQuoteRepository"/>.
/// </summary>
public sealed class QuoteRepository : MongoRepository<Quote>, IQuoteRepository
{
    public QuoteRepository(MongoDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Quote>> GetByAuthorAsync(
        string author, CancellationToken ct = default)
    {
        var filter = Builders<Quote>.Filter.Regex(q => q.Author, new BsonRegularExpression(author, "i"));
        
        return await _collection.Find(filter)
            .SortBy(q => q.Author)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<Quote?> GetRandomAsync(CancellationToken ct = default)
    {
        // MongoDB random document selection using $sample aggregation
        var pipeline = new[]
        {
            new BsonDocument("$sample", new BsonDocument("size", 1))
        };
        
        var result = await _collection.Aggregate<Quote>(pipeline, cancellationToken: ct).FirstOrDefaultAsync(ct);
        return result;
    }
}
