using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Posts;

/// <summary>
/// Post aggregate root — corresponds to DummyJSON /posts resource.
/// Stored in <b>PostgreSQL</b> via EF Core.
///
/// <para>
/// <b>Tags:</b> Stored as a <c>jsonb</c> array column for fast denormalized reads.
/// The authoritative tag registry lives in the <c>Tags</c> / <c>PostTags</c> tables,
/// which are seeded in parallel and now have a proper FK to this table.
/// </para>
/// </summary>
public sealed class Post : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private Post() { }   // Required by EF Core

    private Post(Guid id, Guid userId, string title, string body, IEnumerable<string> tags) : base(id)
    {
        UserId = userId;
        Title = title;
        Body = body;
        Tags = tags.ToList();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// Denormalised tag strings stored as a <c>jsonb</c> array for fast lookup.
    /// Canonical tag entities live in the <c>PostTags</c> join table.
    /// </summary>
    public List<string> Tags { get; private set; } = [];

    public int Views { get; private set; }
    public int Likes { get; private set; }
    public int Dislikes { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────────
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ── ISoftDelete ───────────────────────────────────────────────────────────
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Post> Create(Guid userId, string title, string body, IEnumerable<string> tags)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Post>(Error.Validation(nameof(title), "Title cannot be empty."));

        return Result.Success(new Post(Guid.CreateVersion7(), userId, title, body, tags));
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result Update(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.Validation(nameof(title), "Title cannot be empty."));

        Title = title;
        Body = body;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void IncrementViews() => Views++;
    public void AddLike() => Likes++;
    public void AddDislike() => Dislikes++;

    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
