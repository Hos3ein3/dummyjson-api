using System.Security.Claims;


namespace DummyJson.Domain.SharedKernel.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to easily retrieve user information.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var idString = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(idString, out var id))
            return id;
        return null;
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    => principal.GetUserId() ?? throw new UnauthorizedAccessException("User is not authenticated or UserId claim is missing.");
    

    public static string? GetUserFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("FullName")?.Value;
    }

    public static IReadOnlyList<string> GetUserRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;
    }

    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst("unique_name")?.Value;
    }

    public static string? GetUserPhoneNumber(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.MobilePhone)?.Value;
    }

    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true;
    }

    public static void ForceAuthenticated(this ClaimsPrincipal principal)
    {
        if (!principal.IsAuthenticated())
            throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
