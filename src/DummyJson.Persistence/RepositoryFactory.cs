using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Common.Repository;
using DummyJson.Persistence.Context;
using DummyJson.Persistence.Repositories;

namespace DummyJson.Persistence;

/// <summary>
/// Concrete implementation of <see cref="IRepositoryFactory"/>.
/// Each repository is lazily instantiated on first access and cached
/// for the lifetime of the DI scope (Scoped registration).
/// </summary>
public sealed class RepositoryFactory : IRepositoryFactory
{
    private readonly AppDbContext _context;

    // Lazy backing fields — only instantiated when the property is first accessed
    private IUserRepository? _users;
    private ICartRepository? _carts;
    private ITodoRepository? _todos;
    public RepositoryFactory(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IUserRepository Users
        => _users ??= new UserRepository(_context);

    /// <inheritdoc/>
    public ICartRepository Carts
        => _carts ??= new CartRepository(_context);

    /// <inheritdoc/>
    public ITodoRepository Todos
        => _todos ??= new TodoRepository(_context);

}
