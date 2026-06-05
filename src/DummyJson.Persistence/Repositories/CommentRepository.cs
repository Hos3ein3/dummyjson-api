using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Comments;
using DummyJson.Persistence.Context;
using MongoDB.Driver;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="ICommentRepository"/>.
/// </summary>
public sealed class CommentRepository : MongoRepository<Comment>, ICommentRepository
{
    public CommentRepository(MongoDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<PagedList<Comment>> GetByPostIdAsync(
        Guid postId, int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
        
        var total = await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
        
        var items = await _collection.Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return new PagedList<Comment>(items, page, pageSize, (int)total);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Comment>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.UserId, userId);
        
        return await _collection.Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
