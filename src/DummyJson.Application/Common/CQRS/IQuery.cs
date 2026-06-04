namespace DummyJson.Application.Common.CQRS;

/// <summary>
/// Marker interface for queries. A query reads data and never changes state.
/// </summary>
/// <typeparam name="TResult">The type returned by the query handler.</typeparam>
public interface IQuery<out TResult> { }
