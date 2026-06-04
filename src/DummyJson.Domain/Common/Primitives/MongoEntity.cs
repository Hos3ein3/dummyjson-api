using System;

namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Base class for MongoDB documents.
/// </summary>
public abstract class MongoEntity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}
