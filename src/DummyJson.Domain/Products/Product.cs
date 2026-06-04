using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Products.Events;

namespace DummyJson.Domain.Products;

/// <summary>
/// Product aggregate root — corresponds to DummyJSON /products resource.
/// Products are stored in MongoDB (see MongoDbContext).
/// </summary>
public sealed class Product : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private Product() { } // Required for MongoDB driver deserialization

    private Product(
        Guid id,
        string title,
        string description,
        decimal price,
        decimal discountPercentage,
        double rating,
        int stock,
        string brand,
        string category,
        string thumbnail,
        IEnumerable<string> images,
        IEnumerable<string> tags,
        string sku,
        string barcode,
        int minimumOrderQuantity,
        string warrantyInformation,
        string shippingInformation,
        string availabilityStatus,
        string returnPolicy) : base(id)
    {
        Title = title;
        Description = description;
        Price = price;
        DiscountPercentage = discountPercentage;
        Rating = rating;
        Stock = stock;
        Brand = brand;
        Category = category;
        Thumbnail = thumbnail;
        Images = images.ToList();
        Tags = tags.ToList();
        Sku = sku;
        Barcode = barcode;
        MinimumOrderQuantity = minimumOrderQuantity;
        WarrantyInformation = warrantyInformation;
        ShippingInformation = shippingInformation;
        AvailabilityStatus = availabilityStatus;
        ReturnPolicy = returnPolicy;
    }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public double Rating { get; private set; }
    public int Stock { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Thumbnail { get; private set; } = string.Empty;
    public List<string> Images { get; private set; } = [];
    public List<string> Tags { get; private set; } = [];
    public string Sku { get; private set; } = string.Empty;
    public string Barcode { get; private set; } = string.Empty;
    public int MinimumOrderQuantity { get; private set; }
    public string WarrantyInformation { get; private set; } = string.Empty;
    public string ShippingInformation { get; private set; } = string.Empty;
    public string AvailabilityStatus { get; private set; } = string.Empty;
    public string ReturnPolicy { get; private set; } = string.Empty;
    public List<ProductReview> Reviews { get; private set; } = [];

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

    public static Result<Product> Create(
        string title,
        string description,
        decimal price,
        decimal discountPercentage,
        int stock,
        string brand,
        string category,
        string thumbnail,
        IEnumerable<string> images,
        IEnumerable<string> tags,
        string sku = "",
        string barcode = "",
        int minimumOrderQuantity = 1,
        string warrantyInformation = "",
        string shippingInformation = "",
        string availabilityStatus = "In Stock",
        string returnPolicy = "")
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Product>(Error.Validation(nameof(title), "Title cannot be empty."));

        if (price < 0)
            return Result.Failure<Product>(Error.Validation(nameof(price), "Price cannot be negative."));

        var product = new Product(
            Guid.CreateVersion7(),
            title, description, price, discountPercentage,
            0, stock, brand, category, thumbnail,
            images, tags, sku, barcode, minimumOrderQuantity,
            warrantyInformation, shippingInformation, availabilityStatus, returnPolicy)
        {
            CreatedAt = DateTimeOffset.UtcNow
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, product.Title));
        return Result.Success(product);
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result UpdateDetails(string title, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.Validation(nameof(title), "Title cannot be empty."));

        if (price < 0)
            return Result.Failure(Error.Validation(nameof(price), "Price cannot be negative."));

        Title = title;
        Description = description;
        Price = price;
        Stock = stock;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ProductUpdatedEvent(Id, Title));
        return Result.Success();
    }

    public void AddReview(ProductReview review)
    {
        Reviews.Add(review);
        Rating = Reviews.Count > 0 ? Reviews.Average(r => r.Rating) : 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        RaiseDomainEvent(new ProductDeletedEvent(Id));
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Called by persistence layer for auditing
    internal void SetAuditCreated(string? createdBy)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
    }

    internal void SetAuditUpdated(string? updatedBy)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }
}
