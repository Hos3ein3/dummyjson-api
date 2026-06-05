using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Products;

/// <summary>
/// Product review — a standalone MongoDB document in the <c>product_reviews</c> collection.
///
/// <para>
/// Moved out of the Product aggregate (where it was an embedded value object array)
/// so that reviews can be paginated, queried and managed independently of the
/// Product aggregate. Products are now stored in PostgreSQL; reviews remain in MongoDB
/// to exploit schema-less storage for variable review structures.
/// </para>
/// </summary>
public sealed class ProductReview : MongoEntity
{
    private ProductReview() { }   // Required for MongoDB driver deserialization

    public ProductReview(
        Guid productId,
        Guid userId,
        double rating,
        string comment)
    {
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        Date = DateTimeOffset.UtcNow;
    }

    /// <summary>References the PostgreSQL <c>Products.Id</c> by value (no FK constraint).</summary>
    public Guid ProductId { get; private set; }

    /// <summary>References the PostgreSQL <c>AspNetUsers.Id</c> by value.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Star rating 1–5.</summary>
    public double Rating { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    /// <summary>UTC timestamp of when the review was submitted.</summary>
    public DateTimeOffset Date { get; private set; }
}
