using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="GenericRepository{TEntity,TId}"/>.
/// </summary>
public sealed class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email), ct);

    /// <inheritdoc/>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username), ct);

    /// <inheritdoc/>
    public async Task<PagedList<User>> SearchAsync(
        string searchTerm, int page, int pageSize, CancellationToken ct = default)
    {
        var term = $"%{searchTerm}%";
        var query = _dbSet
            .AsNoTracking()
            .Where(u =>
                EF.Functions.ILike(u.FirstName, term) ||
                EF.Functions.ILike(u.LastName, term)  ||
                EF.Functions.ILike(u.Username, term)  ||
                EF.Functions.ILike(u.Email, term));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<User>(items, page, pageSize, total);
    }
}
