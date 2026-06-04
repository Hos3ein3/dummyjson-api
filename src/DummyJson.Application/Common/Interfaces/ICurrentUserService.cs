namespace DummyJson.Application.Common.Interfaces;

/// <summary>
/// Service to access the currently authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    
    /// <summary>
    /// Gets the current User ID. Throws an exception if the user is not authenticated.
    /// </summary>
    Guid GetUserId();
    
    string? UserFullName { get; }
    IReadOnlyList<string> UserRoles { get; }
    string? UserEmail { get; }
    string? Username { get; }
    string? UserPhoneNumber { get; }

    /// <summary>True when the current request carries a valid, authenticated identity.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Throws an UnauthorizedAccessException if the user is not authenticated.
    /// </summary>
    void ForceAuthenticated();

    /// <summary>
    /// Returns <c>true</c> if the current user has the specified role.
    /// Case-insensitive. Returns <c>false</c> for unauthenticated requests.
    /// </summary>
    bool IsInRole(string role);
}
