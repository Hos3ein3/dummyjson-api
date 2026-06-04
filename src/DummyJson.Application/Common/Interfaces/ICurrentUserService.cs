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
}
