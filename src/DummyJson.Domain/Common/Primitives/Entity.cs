namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Base entity with a generic, value-type identifier.
/// By default use <see cref="Guid"/> produced by <c>Guid.CreateVersion7()</c>
/// which is time-sortable (UUIDv7) and DB-index friendly.
/// </summary>
/// <typeparam name="TId">Type of the identifier — must be a value type (struct).</typeparam>
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(TId id)
    {
        Id = id;
    }

    // Required by EF Core
    protected Entity()
    {
        if (typeof(TId) == typeof(Guid))
        {
            Id = (TId)(object)IdGenerator.NewId();
        }
    }

    public TId Id { get; protected init; }

    // ── Domain Events ─────────────────────────────────────────────────────────

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    // ── Equality ──────────────────────────────────────────────────────────────

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
        => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode()
        => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
