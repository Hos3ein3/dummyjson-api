using Microsoft.AspNetCore.Identity;

namespace DummyJson.Persistence.Context;

/// <summary>
/// Custom ASP.NET Core Identity Role that includes Priority.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base()
    {
        Id = DummyJson.Domain.Common.Primitives.IdGenerator.NewId();
    }
    public ApplicationRole(string roleName) : base(roleName)
    {
        Id = DummyJson.Domain.Common.Primitives.IdGenerator.NewId();
    }

    /// <summary>
    /// Higher priority number indicates higher privileges (e.g. Developer > Admin > System)
    /// </summary>
    public int Priority { get; set; }
}
