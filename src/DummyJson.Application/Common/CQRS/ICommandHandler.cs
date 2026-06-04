namespace DummyJson.Application.Common.CQRS;

/// <summary>
/// Handler for a command that produces no typed result.
/// Implementations should coordinate domain logic and call <c>IUnitOfWork.SaveChangesAsync</c>.
/// Rollback is handled by the <c>TransactionalCommandHandlerDecorator</c>.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for a command that produces a typed result.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
