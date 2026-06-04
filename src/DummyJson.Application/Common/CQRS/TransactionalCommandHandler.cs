using DummyJson.Application.Common.UnitOfWork;

namespace DummyJson.Application.Common.CQRS;

/// <summary>
/// Base class for command handlers that require automatic transaction management.
/// It begins a transaction, calls the implemented logic, and commits/rolls back automatically.
/// </summary>
public abstract class TransactionalCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    protected readonly IUnitOfWork UnitOfWork;

    protected TransactionalCommandHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await UnitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await HandleTransactionalAsync(command, cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Implement your domain logic here. The transaction is already started.
    /// </summary>
    protected abstract Task<TResult> HandleTransactionalAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for void command handlers that require automatic transaction management.
/// </summary>
public abstract class TransactionalCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected readonly IUnitOfWork UnitOfWork;

    protected TransactionalCommandHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await UnitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await HandleTransactionalAsync(command, cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    protected abstract Task HandleTransactionalAsync(TCommand command, CancellationToken cancellationToken);
}
