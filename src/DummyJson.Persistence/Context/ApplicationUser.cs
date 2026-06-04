using Microsoft.AspNetCore.Identity;

namespace DummyJson.Persistence.Context;

/// <summary>
/// ASP.NET Core Identity user — bridges the Identity framework with our domain User.
/// The domain <c>User</c> aggregate holds business logic; this class is the identity store.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid DomainUserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Image { get; set; }
    
    // Explicitly requested verification flags
    public bool EmailVerified { get; set; }
    public bool PhoneNumberVerified { get; set; }

    public ICollection<ApplicationUserRefreshToken> RefreshTokens { get; set; } = new List<ApplicationUserRefreshToken>();

    public ApplicationUser()
    {
        Id = DummyJson.Domain.Common.Primitives.IdGenerator.NewId();
    }
}
