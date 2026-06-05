using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Domain.Tags;

/// <summary>
/// Shared tag entity.
/// Used by both products (via <c>ProductTag</c> join) and posts (via <c>PostTag</c> join).
///
/// <para>
/// <b>Design rationale — one table, not two:</b><br/>
/// Product and post tags share a large vocabulary ("beauty", "history", etc.).
/// Keeping them in one <see cref="Tag"/> table with a <see cref="TagType"/>
/// discriminator:
/// <list type="bullet">
///   <item>Avoids duplication of cross-domain tags.</item>
///   <item>Enables a single admin UI / API for tag management.</item>
///   <item>Allows future features like tag-based cross-entity search.</item>
/// </list>
/// The join tables <c>ProductTag</c> and <c>PostTag</c> keep the many-to-many
/// relationships clean and independently queryable.
/// </para>
/// </summary>
public sealed class Tag : MongoEntity
{
    private Tag() { }

    private Tag(Guid id, string name, TagType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    /// <summary>Normalized lower-case tag name (e.g. "beauty").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Indicates which domain(s) this tag belongs to.</summary>
    public TagType Type { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>Creates a new tag with a normalised (trimmed, lower-case) name.</summary>
    public static Result<Tag> Create(string name, TagType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tag>(Error.Validation(nameof(name), "Tag name cannot be empty."));

        return Result.Success(new Tag(Guid.CreateVersion7(), name.Trim().ToLowerInvariant(), type));
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Updates the tag type (e.g. from Product to Shared when also used on a post).</summary>
    public void PromoteToShared() => Type = TagType.Shared;

    /// <summary>Renames the tag (normalised).</summary>
    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(Error.Validation(nameof(newName), "Tag name cannot be empty."));

        Name = newName.Trim().ToLowerInvariant();
        return Result.Success();
    }

    public override string ToString() => Name;
}

/// <summary>
/// Join entity between <see cref="Tag"/> and a product (referenced by <see cref="ProductId"/>
/// — this is the MongoDB Product's Guid ID, stored by value since Products live in MongoDB).
/// </summary>
public sealed class ProductTag : MongoEntity
{
    private ProductTag() { }

    public ProductTag(Guid productId, Guid tagId)
    {
        Id = Guid.CreateVersion7();
        ProductId = productId;
        TagId = tagId;
    }

    /// <summary>MongoDB Product ID (external reference — no FK constraint).</summary>
    public Guid ProductId { get; private set; }

    public Guid TagId { get; private set; }
}

/// <summary>
/// Join entity between <see cref="Tag"/> and a post (referenced by <see cref="PostId"/>
/// — MongoDB Post ID, stored by value).
/// </summary>
public sealed class PostTag : MongoEntity
{
    private PostTag() { }

    public PostTag(Guid postId, Guid tagId)
    {
        Id = Guid.CreateVersion7();
        PostId = postId;
        TagId = tagId;
    }

    /// <summary>MongoDB Post ID (external reference — no FK constraint).</summary>
    public Guid PostId { get; private set; }

    public Guid TagId { get; private set; }
}
