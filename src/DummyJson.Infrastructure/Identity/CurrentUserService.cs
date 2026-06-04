using System.Security.Claims;
using DummyJson.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DummyJson.Infrastructure.Identity;

/// <summary>
/// Reads the current authenticated user's identity from the JWT claims
/// in <see cref="IHttpContextAccessor.HttpContext"/>.
///
/// All properties return <c>null</c> (or empty collections) when the request
/// is unauthenticated — callers that require authentication should call
/// <see cref="GetUserId"/> which throws <see cref="UnauthorizedAccessException"/>.
///
/// Claim mapping (matches <c>JwtTokenService.GenerateAccessToken</c>):
/// <list type="table">
///   <listheader><term>Property</term><description>JWT Claim</description></listheader>
///   <item><term><see cref="UserId"/></term><description><c>sub</c></description></item>
///   <item><term><see cref="Username"/></term><description><c>unique_name</c></description></item>
///   <item><term><see cref="UserEmail"/></term><description><c>email</c></description></item>
///   <item><term><see cref="UserRoles"/></term><description><c>role</c> (ClaimTypes.Role)</description></item>
///   <item><term><see cref="UserFullName"/></term><description><c>FullName</c> (custom)</description></item>
///   <item><term><see cref="UserPhoneNumber"/></term><description><c>MobilePhone</c> (ClaimTypes)</description></item>
/// </list>
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    /// <summary>Safe access to the current ClaimsPrincipal — null when no HTTP context.</summary>
    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Returns the first non-null value found among the supplied claim types.
    /// </summary>
    private string? FirstClaim(params string[] claimTypes)
    {
        foreach (var type in claimTypes)
        {
            var value = Principal?.FindFirstValue(type);
            if (!string.IsNullOrEmpty(value)) return value;
        }
        return null;
    }

    // ── ICurrentUserService ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public Guid? UserId
    {
        get
        {
            // JwtTokenService writes the userId into the 'sub' claim
            var raw = FirstClaim(JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the request is not authenticated or the <c>sub</c> claim is absent.
    /// </exception>
    public Guid GetUserId()
        => UserId ?? throw new UnauthorizedAccessException(
            "The current request is not authenticated or the user-id claim is missing. " +
            "Ensure the endpoint is protected with [Authorize] / RequireAuthorization().");

    /// <inheritdoc/>
    /// <remarks>
    /// Reads the custom <c>FullName</c> claim written by <c>JwtTokenService</c>.
    /// Falls back to combining first + last name claims if available.
    /// </remarks>
    public string? UserFullName
        => FirstClaim("FullName", "full_name")
           ?? CombineFirstLast();

    /// <inheritdoc/>
    /// <remarks>Collects all <c>role</c> / <c>ClaimTypes.Role</c> claims.</remarks>
    public IReadOnlyList<string> UserRoles
        => Principal?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
           ?? [];

    /// <inheritdoc/>
    public string? UserEmail
        => FirstClaim(
            JwtRegisteredClaimNames.Email,
            ClaimTypes.Email);

    /// <inheritdoc/>
    public string? Username
        => FirstClaim(
            JwtRegisteredClaimNames.UniqueName,
            ClaimTypes.Name);

    /// <inheritdoc/>
    public string? UserPhoneNumber
        => FirstClaim(ClaimTypes.MobilePhone, "phone_number");

    /// <summary>True when a valid, authenticated principal is present.</summary>
    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated is true;

    /// <summary>
    /// Checks whether the current user has the given role (case-insensitive).
    /// Returns <c>false</c> for unauthenticated requests.
    /// </summary>
    public bool IsInRole(string role)
        => Principal?.IsInRole(role) is true;

    // ── Private helpers ───────────────────────────────────────────────────────

    private string? CombineFirstLast()
    {
        var first = FirstClaim(ClaimTypes.GivenName);
        var last = FirstClaim(ClaimTypes.Surname);
        if (first is null && last is null) return null;
        return $"{first} {last}".Trim();
    }
}
