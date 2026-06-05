using DummyJson.Domain.Comments;
using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;
using DummyJson.Application.Common.Interfaces;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="Comment"/> aggregate root.
/// Extends <see cref="IMongoRepository{TEntity}"/> with Comment-specific queries.
/// </summary>
public interface ICommentRepository : IMongoRepository<Comment>
{
    /// <summary>Returns all comments associated with a given post, paged.</summary>
    Task<PagedList<Comment>> GetByPostIdAsync(Guid postId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns all comments written by a given user.</summary>
    Task<IReadOnlyList<Comment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
