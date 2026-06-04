using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Comments;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICommentRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="GenericRepository{TEntity,TId}"/>.
/// </summary>
public sealed class CommentRepository : GenericRepository<Comment, Guid>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<PagedList<Comment>> GetByPostIdAsync(
        Guid postId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(c => c.PostId == postId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<Comment>(items, page, pageSize, total);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Comment>> GetByUsernameAsync(
        string username, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(c => c.Username == username)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
}
