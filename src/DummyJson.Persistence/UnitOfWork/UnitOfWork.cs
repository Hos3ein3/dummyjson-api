using DummyJson.Application.Common.Events;
using DummyJson.Application.Common.Repository;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Persistence.Context;
using DummyJson.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DummyJson.Persistence.UnitOfWork;

/// <summary>
/// Concrete Unit of Work wrapping <see cref="AppDbContext"/>.
/// Manages a single <see cref="IDbContextTransaction"/> shared across repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly Dictionary<string, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(AppDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    // ── Repository Factory ────────────────────────────────────────────────────

    public IRepository<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : Entity<TId>
        where TId : struct
    {
        var key = $"{typeof(TEntity).Name}-{typeof(TId).Name}";

        if (!_repositories.TryGetValue(key, out var repo))
        {
            repo = new GenericRepository<TEntity, TId>(_context);
            _repositories[key] = repo;
        }

        return (IRepository<TEntity, TId>)repo;
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _context.SaveChangesAsync(cancellationToken);
        
        var domainEvents = _context.CollectDomainEvents();
        if (domainEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    // ── Transaction ───────────────────────────────────────────────────────────

    public bool HasActiveTransaction => _transaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already in progress.");
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _transaction?.Dispose();
        _context.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        if (_transaction is not null)
            await _transaction.DisposeAsync();
        await _context.DisposeAsync();
        _disposed = true;
    }
}
