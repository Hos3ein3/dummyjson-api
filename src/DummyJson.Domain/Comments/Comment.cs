using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Comments;

/// <summary>
/// Comment aggregate root — corresponds to DummyJSON /comments resource.
/// Belongs to a Post identified by <see cref="PostId"/>.
/// Stored in MongoDB via MongoDbContext.
/// </summary>
public sealed class Comment : MongoEntity, IAuditable, ISoftDelete
{
    private Comment() { }

    private Comment(string body, Guid? postId, Guid userId)
    {
        Body = body;
        PostId = postId;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Text body of the comment.</summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// ID of the parent post. Stored as a nullable Guid because the seeded data
    /// references DummyJSON integer post IDs which cannot be correlated during seeding.
    /// </summary>
    public Guid? PostId { get; private set; }

    /// <summary>User who authored the comment.</summary>
    public Guid UserId { get; private set; }

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
        Guid userId,
        int likes = 0)
    {
        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure<Comment>(Error.Validation(nameof(body), "Comment body cannot be empty."));

        var comment = new Comment(body, postId, userId);
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
