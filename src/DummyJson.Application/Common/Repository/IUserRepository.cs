using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Users;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="User"/> aggregate root.
/// Extends <see cref="IRepository{TEntity,TId}"/> with User-specific queries.
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>Finds a user by their unique email address (case-insensitive).</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Finds a user by their unique username (case-insensitive).</summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Full-text style search on FirstName, LastName, Username, Email.</summary>
    Task<PagedList<User>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken ct = default);
}
