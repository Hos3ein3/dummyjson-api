using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Quotes;
using SharedKernel.Results;
using DummyJson.Application.Common.Interfaces;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="Quote"/> aggregate root.
/// Extends <see cref="IMongoRepository{TEntity}"/> with Quote-specific queries.
/// </summary>
public interface IQuoteRepository : IMongoRepository<Quote>
{
    /// <summary>Returns all quotes attributed to the given author.</summary>
    Task<IReadOnlyList<Quote>> GetByAuthorAsync(string author, CancellationToken ct = default);

    /// <summary>Returns a random quote from the database.</summary>
    Task<Quote?> GetRandomAsync(CancellationToken ct = default);
}
