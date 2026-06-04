using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Posts;

/// <summary>
/// Post aggregate root — corresponds to DummyJSON /posts resource.
/// Stored in MongoDB.
/// </summary>
public sealed class Post : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private readonly List<string> _tags = [];
    private Post() { }

    private Post(Guid id, Guid userId, string title, string body, IEnumerable<string> tags) : base(id)
    {
        UserId = userId;
        Title = title;
        Body = body;
        _tags.AddRange(tags);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public int Views { get; private set; }
    public int Likes { get; private set; }
    public int Dislikes { get; private set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public static Result<Post> Create(Guid userId, string title, string body, IEnumerable<string> tags)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Post>(Error.Validation(nameof(title), "Title cannot be empty."));

        return Result.Success(new Post(Guid.CreateVersion7(), userId, title, body, tags));
    }

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
