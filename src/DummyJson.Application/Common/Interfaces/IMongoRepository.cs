using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for MongoDB collections.
/// </summary>
public interface IMongoRepository<T> where T : MongoEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
