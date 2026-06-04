using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Carts;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICartRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="GenericRepository{TEntity,TId}"/>.
/// </summary>
public sealed class CartRepository : GenericRepository<Cart, Guid>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _dbSet
            .Include("_items")   // shadow navigation — EF resolves via CartConfiguration
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Cart>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _dbSet
            .IgnoreQueryFilters()   // include soft-deleted records
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
}
