using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Common.Repository;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Services;

public class GenericService<TEntity, TId> : IGenericService<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : struct
{
    protected readonly IRepository<TEntity, TId> _repository;
    protected readonly IUnitOfWork _unitOfWork;

    public GenericService(IRepository<TEntity, TId> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<TEntity>(Error.NotFound($"{typeof(TEntity).Name}.NotFound", $"{typeof(TEntity).Name} with id {id} was not found."));
        }

        return Result.Success(entity);
    }

    public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(entities);
    }

    public virtual async Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity);
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity);
    }

    public virtual async Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound($"{typeof(TEntity).Name}.NotFound", $"{typeof(TEntity).Name} with id {id} was not found."));
        }

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
