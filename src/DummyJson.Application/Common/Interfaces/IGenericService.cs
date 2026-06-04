using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Interfaces;

public interface IGenericService<TEntity, TId> 
    where TEntity : Entity<TId>
    where TId : struct
{
    Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken = default);
}
