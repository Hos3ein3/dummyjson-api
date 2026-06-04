namespace DummyJson.Application.Common.CQRS;

/// <summary>
/// Marker interface for commands that produce no typed result (fire-and-forget style).
/// </summary>
public interface ICommand { }

/// <summary>
/// Marker interface for commands that produce a typed result.
/// </summary>
/// <typeparam name="TResult">The type of the result returned after execution.</typeparam>
public interface ICommand<out TResult> { }
