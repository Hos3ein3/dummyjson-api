using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Products;

/// <summary>
/// Product review value object embedded within the Product aggregate.
/// </summary>
public sealed class ProductReview : ValueObject
{
    private ProductReview() { }

    public ProductReview(string reviewerName, string reviewerEmail, double rating, string comment)
    {
        ReviewerName = reviewerName;
        ReviewerEmail = reviewerEmail;
        Rating = rating;
        Comment = comment;
        Date = DateTimeOffset.UtcNow;
    }

    public string ReviewerName { get; private set; } = string.Empty;
    public string ReviewerEmail { get; private set; } = string.Empty;
    public double Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTimeOffset Date { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ReviewerEmail;
        yield return Date;
    }
}
