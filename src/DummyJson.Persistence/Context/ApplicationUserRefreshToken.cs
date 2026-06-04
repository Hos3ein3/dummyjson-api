using System;

namespace DummyJson.Persistence.Context;

/// <summary>
/// Dedicated table for tracking Refresh Tokens per user/device.
/// Allows a user to be logged in on multiple devices simultaneously.
/// </summary>
public sealed class ApplicationUserRefreshToken
{
    public Guid Id { get; set; } = DummyJson.Domain.Common.Primitives.IdGenerator.NewId();
    
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty; // Matches the jti of the access token
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiryDate { get; set; }
}
