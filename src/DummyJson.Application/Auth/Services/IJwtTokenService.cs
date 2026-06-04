using SharedKernel.Results;

namespace DummyJson.Application.Auth.Services;

/// <summary>
/// Abstraction for JWT token generation and validation.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Generates a JWT access token for the given user.</summary>
    string GenerateAccessToken(Guid userId, string username, string email, string role, string? fullName, string? phoneNumber);

    /// <summary>Generates a random refresh token string.</summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an access token and returns the user ID if valid.
    /// Returns null if the token is invalid or expired.
    /// </summary>
    Result<Guid> ValidateAccessToken(string token);

    /// <summary>Returns the configured access token expiry.</summary>
    DateTimeOffset GetAccessTokenExpiry();
}
