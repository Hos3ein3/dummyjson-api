using DummyJson.Persistence.Context;

namespace DummyJson.Persistence;

/// <summary>
/// Factory that provides access to the underlying database contexts.
///
/// This interface lives in the <c>DummyJson.Persistence</c> namespace (not Application)
/// because <see cref="AppDbContext"/> and <see cref="MongoDbContext"/> are infrastructure
/// types that the Application layer must not reference directly.
///
/// Infrastructure services and handlers that already have a Persistence dependency
/// (e.g. data-pipeline workers, seeding services) can consume this factory
/// without needing to know which context handles which entity.
/// </summary>
public interface IDatabaseFactory
{
    /// <summary>
    /// Returns the EF Core relational context (<see cref="AppDbContext"/>).
    /// Scoped to the current DI scope.
    /// </summary>
    AppDbContext GetRelationalContext();

    /// <summary>
    /// Returns the MongoDB context (<see cref="MongoDbContext"/>).
    /// Singleton shared across the application lifetime.
    /// </summary>
    MongoDbContext GetDocumentContext();
}
