using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Todos;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITodoRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="GenericRepository{TEntity,TId}"/>.
/// </summary>
public sealed class TodoRepository : GenericRepository<Todo, Guid>, ITodoRepository
{
    public TodoRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<PagedList<Todo>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().Where(t => t.UserId == userId);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<Todo>(items, page, pageSize, total);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Todo>> GetPendingByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(t => t.UserId == userId && !t.Completed)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}
