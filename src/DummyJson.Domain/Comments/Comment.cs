using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Comments;

/// <summary>
/// Comment aggregate root — corresponds to DummyJSON /comments resource.
/// Belongs to a Post identified by <see cref="PostId"/>.
/// Stored in PostgreSQL via EF Core.
/// </summary>
public sealed class Comment : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private Comment() { }

    private Comment(Guid id, string body, Guid? postId, string username, string fullName)
        : base(id)
    {
        Body = body;
        PostId = postId;
        Username = username;
        FullName = fullName;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Text body of the comment.</summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// ID of the parent post. Stored as a nullable Guid because the seeded data
    /// references DummyJSON integer post IDs which cannot be correlated during seeding.
    /// </summary>
    public Guid? PostId { get; private set; }

    /// <summary>Username of the commenter (denormalised from the user sub-object).</summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>Full name of the commenter (denormalised from the user sub-object).</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Like count on this comment.</summary>
    public int Likes { get; private set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Comment> Create(
        string body,
        Guid? postId,
        string username,
        string fullName,
        int likes = 0)
    {
        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure<Comment>(Error.Validation(nameof(body), "Comment body cannot be empty."));

        var comment = new Comment(Guid.CreateVersion7(), body, postId, username, fullName);
        comment.Likes = likes;
        return Result.Success(comment);
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result UpdateBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure(Error.Validation(nameof(body), "Comment body cannot be empty."));

        Body = body;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void AddLike() { Likes++; UpdatedAt = DateTimeOffset.UtcNow; }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
