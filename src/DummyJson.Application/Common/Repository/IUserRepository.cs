using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Users;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="ApplicationUser"/> aggregate root.
/// Extends <see cref="IRepository{TEntity,TId}"/> with User-specific queries.
/// </summary>
public interface IUserRepository : IRepository<ApplicationUser, Guid>
{
    /// <summary>Finds a user by their unique email address (case-insensitive).</summary>
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Finds a user by their unique username (case-insensitive).</summary>
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Full-text style search on FirstName, LastName, Username, Email.</summary>
    Task<PagedList<ApplicationUser>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken ct = default);
}
