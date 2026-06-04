using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Persistence.Context;
using MongoDB.Driver;

namespace DummyJson.Persistence.Repositories;

public class MongoRepository<T> : IMongoRepository<T> where T : MongoEntity
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(MongoDbContext dbContext)
    {
        // Get collection name by type name, e.g. "UserPreferences" -> "user_preferences"
        var collectionName = typeof(T).Name.ToLowerInvariant();
        
        // Expose a public method in MongoDbContext or use reflection to get the database, 
        // or just pass IMongoDatabase to the repository.
        // Let's rely on MongoDbContext exposing a GetCollection<T> or the Database itself.
        _collection = dbContext.Database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, new InsertOneOptions(), cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions(), cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }
}
