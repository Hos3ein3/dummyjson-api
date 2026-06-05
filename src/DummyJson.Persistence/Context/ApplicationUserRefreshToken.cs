using System;
using DummyJson.Domain.Users;

namespace DummyJson.Persistence.Context;

/// <summary>
/// Stores refresh tokens associated with a User.
/// Included in the EF Core model to allow querying and revoking.
/// </summary>
public sealed class ApplicationUserRefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty; // Matches the jti of the access token
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiryDate { get; set; }
}
