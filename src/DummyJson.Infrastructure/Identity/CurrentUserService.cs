using System.Security.Claims;
using DummyJson.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DummyJson.Infrastructure.Identity;

/// <summary>
/// Default implementation of ICurrentUserService using HttpContext.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var idString = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idString, out var id))
                return id;
            return null;
        }
    }

    public Guid GetUserId()
    {
        return UserId ?? throw new UnauthorizedAccessException("User is not authenticated or UserId claim is missing.");
    }

    public string? UserFullName => User?.FindFirstValue("FullName"); // Or whatever claim represents full name
    public IReadOnlyList<string> UserRoles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
    public string? UserEmail => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email);
    public string? Username => User?.FindFirstValue(ClaimTypes.Name) ?? User?.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
    public string? UserPhoneNumber => User?.FindFirstValue(ClaimTypes.MobilePhone);
}
