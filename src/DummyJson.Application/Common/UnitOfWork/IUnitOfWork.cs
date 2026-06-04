using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Application.Common.UnitOfWork;

/// <summary>
/// Unit of Work — coordinates multiple repositories under a single transaction.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Returns a typed repository for a given entity.
    /// Repositories are cached per UoW instance.
    /// </summary>
    IRepository<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : Entity<TId>
        where TId : struct;

    /// <summary>
    /// Persists all tracked changes to the database.
    /// Returns the number of records affected.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Begins a new database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// Call this only after all work within the transaction has succeeded.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// Called automatically by <c>TransactionalCommandHandlerDecorator</c> on exception.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns true if a transaction is currently active.</summary>
    bool HasActiveTransaction { get; }
}
