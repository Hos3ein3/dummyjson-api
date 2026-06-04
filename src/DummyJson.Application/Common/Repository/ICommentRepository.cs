using DummyJson.Domain.Comments;
using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="Comment"/> aggregate root.
/// Extends <see cref="IRepository{TEntity,TId}"/> with Comment-specific queries.
/// </summary>
public interface ICommentRepository : IRepository<Comment, Guid>
{
    /// <summary>Returns all comments associated with a given post, paged.</summary>
    Task<PagedList<Comment>> GetByPostIdAsync(Guid postId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns all comments written by a given username.</summary>
    Task<IReadOnlyList<Comment>> GetByUsernameAsync(string username, CancellationToken ct = default);
}
