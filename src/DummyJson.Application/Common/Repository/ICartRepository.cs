using DummyJson.Domain.Carts;
using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="Cart"/> aggregate root.
/// Extends <see cref="IRepository{TEntity,TId}"/> with Cart-specific queries.
/// </summary>
public interface ICartRepository : IRepository<Cart, Guid>
{
    /// <summary>
    /// Returns the active (non-deleted) cart belonging to a user,
    /// including its items, or <c>null</c> if none exists.
    /// </summary>
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns all carts belonging to a user (including soft-deleted).</summary>
    Task<IReadOnlyList<Cart>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);
}
