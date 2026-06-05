using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Common.Interfaces;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Persistence.Context;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository — covers all relational entities.
/// MongoDB entities (Product, Post) use their own dedicated repositories.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TId">Primary key type.</typeparam>
public class GenericRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public virtual async Task<TEntity?> GetByIdAsNoTrackingAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        return entity;
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public virtual async Task<PagedList<TEntity>> GetPagedResultAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = await _dbSet.CountAsync(cancellationToken);
        var items = await GetPagedAsync(page, pageSize, cancellationToken);
        return new PagedList<TEntity>(items, page, pageSize, total);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(cancellationToken);

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(e => e.Id.Equals(id), cancellationToken);

    // ── Write ─────────────────────────────────────────────────────────────────

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    /// <inheritdoc/>
    /// <remarks>
    /// Performs a true SQL bulk insert bypassing EF Core change-tracking.
    /// <c>SaveChangesAsync</c> is NOT required after this call.
    /// Entities must already have their primary keys set (Guid.CreateVersion7()).
    /// Audit fields (CreatedAt, ConcurrencyStamp) must be populated before calling.
    /// </remarks>
    public virtual async Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var list = entities as List<TEntity> ?? entities.ToList();
        if (list.Count == 0) return;

        var config = new BulkConfig
        {
            BatchSize = 500,
            SetOutputIdentity = false,
            PreserveInsertOrder = true
        };

        await _context.BulkInsertAsync(list, config, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Performs a true SQL bulk update bypassing EF Core change-tracking.
    /// <c>SaveChangesAsync</c> is NOT required after this call.
    /// </remarks>
    public virtual async Task BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var list = entities as List<TEntity> ?? entities.ToList();
        if (list.Count == 0) return;

        var config = new BulkConfig { BatchSize = 500 };
        await _context.BulkUpdateAsync(list, config, cancellationToken: cancellationToken);
    }

    public virtual void Update(TEntity entity)
        => _dbSet.Update(entity);

    public virtual void Delete(TEntity entity)
    {
        // If entity supports soft-delete, use it; otherwise hard-delete
        if (entity is ISoftDelete softDelete)
        {
            softDelete.Delete();
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual void Attach(TEntity entity)
        => _dbSet.Attach(entity);

    public virtual void Detach(TEntity entity)
        => _context.Entry(entity).State = EntityState.Detached;
}
