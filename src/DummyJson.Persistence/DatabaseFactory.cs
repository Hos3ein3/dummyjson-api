using DummyJson.Persistence.Context;

namespace DummyJson.Persistence;

/// <summary>
/// Concrete implementation of <see cref="IDatabaseFactory"/>.
/// Holds references to both the EF Core and MongoDB contexts,
/// resolved from the DI container at construction time.
/// </summary>
public sealed class DatabaseFactory : IDatabaseFactory
{
    private readonly AppDbContext _relationalContext;
    private readonly MongoDbContext _documentContext;

    public DatabaseFactory(AppDbContext relationalContext, MongoDbContext documentContext)
    {
        _relationalContext = relationalContext;
        _documentContext = documentContext;
    }

    /// <inheritdoc/>
    public AppDbContext GetRelationalContext() => _relationalContext;

    /// <inheritdoc/>
    public MongoDbContext GetDocumentContext() => _documentContext;
}
