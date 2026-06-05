namespace DummyJson.Domain.Common.Primitives;

public interface IEntity<TId> where TId : struct
{
    TId Id { get; }
}
