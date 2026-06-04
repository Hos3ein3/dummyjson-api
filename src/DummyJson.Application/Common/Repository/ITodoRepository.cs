using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Todos;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="Todo"/> aggregate root.
/// Extends <see cref="IRepository{TEntity,TId}"/> with Todo-specific queries.
/// </summary>
public interface ITodoRepository : IRepository<Todo, Guid>
{
    /// <summary>Returns all todos belonging to the given user, paged.</summary>
    Task<PagedList<Todo>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns all incomplete todos belonging to the given user.</summary>
    Task<IReadOnlyList<Todo>> GetPendingByUserIdAsync(Guid userId, CancellationToken ct = default);
}
