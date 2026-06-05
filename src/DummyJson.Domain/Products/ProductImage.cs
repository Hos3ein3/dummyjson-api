using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Products;

/// <summary>
/// Product image — a standalone MongoDB document in the <c>product_images</c> collection.
/// </summary>
public sealed class ProductImage : MongoEntity
{
    private ProductImage() { }   // Required for MongoDB driver deserialization

    public ProductImage(Guid productId, string url)
    {
        ProductId = productId;
        Url = url;
    }

    /// <summary>References the PostgreSQL <c>Products.Id</c> by value.</summary>
    public Guid ProductId { get; private set; }

    public string Url { get; private set; } = string.Empty;
}
