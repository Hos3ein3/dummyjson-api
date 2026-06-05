namespace DummyJson.Domain.Tags;

/// <summary>
/// Discriminator for which domain a <see cref="Tag"/> belongs to.
/// A single tag (e.g. "beauty") may appear in multiple types simultaneously
/// via separate join rows in ProductTag / PostTag.
/// </summary>
public enum TagType : byte
{
    /// <summary>Tag used to classify products (e.g. "mascara", "beauty").</summary>
    Product = 1,

    /// <summary>Tag used to classify blog posts (e.g. "history", "crime").</summary>
    Post = 2,

    /// <summary>Tag that belongs to both products and posts (shared vocabulary).</summary>
    Shared = 3
}
