namespace DummyJson.Domain.Common.Interfaces;

/// <summary>
/// Marks an entity as auditable — tracks creation and modification metadata.
/// Implemented automatically via <c>AppDbContext.SaveChangesAsync</c>.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    string? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    string? UpdatedBy { get; }
}
