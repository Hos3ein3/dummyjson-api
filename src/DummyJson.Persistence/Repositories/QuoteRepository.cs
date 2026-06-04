using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Quotes;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IQuoteRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="GenericRepository{TEntity,TId}"/>.
/// </summary>
public sealed class QuoteRepository : GenericRepository<Quote, Guid>, IQuoteRepository
{
    public QuoteRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Quote>> GetByAuthorAsync(
        string author, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(q => EF.Functions.ILike(q.Author, $"%{author}%"))
            .OrderBy(q => q.Author)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<Quote?> GetRandomAsync(CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .OrderBy(_ => EF.Functions.Random())
            .FirstOrDefaultAsync(ct);
}
