namespace DummyJson.Domain.Common.Interfaces;

/// <summary>
/// Marks an entity as soft-deletable.
/// Deleted records remain in the database but are hidden from normal queries
/// via a global query filter in <c>AppDbContext</c>.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
    string? DeletedBy { get; }

    /// <summary>
    /// Soft-deletes this entity, setting <see cref="IsDeleted"/> to <c>true</c>.
    /// </summary>
    void Delete(string? deletedBy = null);

    /// <summary>
    /// Restores a previously soft-deleted entity.
    /// </summary>
    void Restore();
}
