using SharedKernel.Results;

namespace DummyJson.Application.Auth.Services;

/// <summary>
/// Abstraction for refresh token storage (backed by Redis in Infrastructure).
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Stores a refresh token associated with a user ID.</summary>
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Validates a refresh token and returns the associated user ID if valid.</summary>
    Task<Result<Guid>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes a refresh token (on logout or rotation).</summary>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes all refresh tokens for a given user.</summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
