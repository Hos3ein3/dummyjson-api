using DummyJson.Application.Common.CQRS;

namespace DummyJson.Application.Common.Dispatcher;

/// <summary>
/// Dispatcher resolves handlers from DI and invokes them.
/// Acts as the single dispatch entry point — the equivalent of MediatR's ISender without the package.
/// </summary>
public interface IDispatcher
{
    /// <summary>Send a command that returns no typed result.</summary>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>Send a command that returns a typed result.</summary>
    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>Send a query and return its result.</summary>
    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
