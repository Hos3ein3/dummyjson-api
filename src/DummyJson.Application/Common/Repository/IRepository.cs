using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Generic repository interface for basic CRUD operations.
/// Concrete implementation lives in the Persistence layer.
/// </summary>
/// <typeparam name="TEntity">Entity type that inherits from <see cref="Entity{TId}"/>.</typeparam>
/// <typeparam name="TId">Type of the primary key — must be a value type.</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>Returns entity by id, or null if not found.</summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Returns entity by id without tracking it in EF Core, or null if not found.</summary>
    Task<TEntity?> GetByIdAsNoTrackingAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities (not deleted if soft-delete is applied).</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a paged list of entities.</summary>
    Task<IReadOnlyList<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Returns a structured PagedList of entities.</summary>
    Task<PagedList<TEntity>> GetPagedResultAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<PagedList<TEntity>> GetPagedResultByOffsetAsync(
        int skip,
        int limit,
        CancellationToken cancellationToken = default);
    
    /// <summary>Returns total count of entities.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns true if an entity with the given id exists.</summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity. Call SaveChangesAsync on the UoW to persist.</summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Adds a collection of entities.</summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Efficiently inserts a large batch of entities in a single database round-trip.
    /// Bypasses EF Core change-tracking — no SaveChangesAsync needed.
    /// </summary>
    Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Efficiently updates a large batch of entities in a single database round-trip.
    /// Bypasses EF Core change-tracking — no SaveChangesAsync needed.
    /// </summary>
    Task BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Marks an entity as modified. EF Core tracks changes automatically.</summary>
    void Update(TEntity entity);

    /// <summary>Removes an entity from the store (hard delete).</summary>
    void Delete(TEntity entity);

    /// <summary>Attaches an entity to the context.</summary>
    void Attach(TEntity entity);

    /// <summary>Detaches an entity from the context.</summary>
    void Detach(TEntity entity);
}
