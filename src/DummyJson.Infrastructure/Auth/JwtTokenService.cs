using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DummyJson.Application.Auth.Services;
using SharedKernel.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DummyJson.Infrastructure.Auth;

/// <summary>
/// JWT token generation and validation service.
/// Configuration is read from <c>JwtSettings</c> section in appsettings.json.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IConfiguration configuration)
    {
        _settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration section is missing.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
    }

    public string GenerateAccessToken(Guid userId, string username, string email, string role, string? fullName, string? phoneNumber)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(fullName))
            claims.Add(new Claim("FullName", fullName));
            
        if (!string.IsNullOrEmpty(phoneNumber))
            claims.Add(new Claim(ClaimTypes.MobilePhone, phoneNumber));

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: GetAccessTokenExpiry().UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public Result<Guid> ValidateAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = handler.ValidateToken(token, validationParams, out _);
            var userIdStr = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
                return Result.Failure<Guid>(CommonErrors.Unauthorized());

            return Result.Success(userId);
        }
        catch (SecurityTokenException)
        {
            return Result.Failure<Guid>(CommonErrors.Unauthorized());
        }
    }

    public DateTimeOffset GetAccessTokenExpiry()
        => DateTimeOffset.UtcNow.AddMinutes(_settings.ExpirationMinutes);
}

/// <summary>JWT configuration model — binds to <c>JwtSettings</c> config section.</summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
