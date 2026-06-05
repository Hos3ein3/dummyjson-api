using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Products.Events;

namespace DummyJson.Domain.Products;

/// <summary>
/// Product aggregate root — corresponds to DummyJSON /products resource.
/// Stored in <b>PostgreSQL</b> via EF Core.
///
/// <para>
/// <b>Category:</b> Normalised via <see cref="CategoryId"/> FK to the
/// <c>ProductCategories</c> lookup table. The <see cref="Category"/>
/// navigation property is available for eager/lazy loading.
/// </para>
/// <para>
/// <b>Images:</b> Stored as a <c>jsonb</c> array column — no separate table
/// needed for a simple string list.
/// </para>
/// <para>
/// <b>Tags:</b> Managed via the <c>ProductTags</c> join table (see
/// <see cref="Tags"/> domain entities).
/// </para>
/// <para>
/// <b>Reviews:</b> Moved to MongoDB — stored as standalone
/// <see cref="ProductReview"/> documents in the <c>product_reviews</c>
/// collection. Not loaded here; query the review repository separately.
/// </para>
/// </summary>
public sealed class Product : AggregateRoot<Guid>, IAuditable, ISoftDelete, IConcurrent
{
    private Product() { }   // Required by EF Core

    private Product(
        Guid id,
        string title,
        string description,
        decimal price,
        decimal discountPercentage,
        double rating,
        int stock,
        string brand,
        Guid categoryId,
        string thumbnail,
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
        CategoryId = categoryId;

        Thumbnail = thumbnail;
        Sku = sku;
        Barcode = barcode;
        MinimumOrderQuantity = minimumOrderQuantity;
        WarrantyInformation = warrantyInformation;
        ShippingInformation = shippingInformation;
        AvailabilityStatus = availabilityStatus;
        ReturnPolicy = returnPolicy;
    }

    // ── Scalar properties ─────────────────────────────────────────────────────

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public double Rating { get; private set; }
    public int Stock { get; private set; }
    public string Brand { get; private set; } = string.Empty;

    // ── Category (normalised FK) ───────────────────────────────────────────────

    /// <summary>FK to the <c>ProductCategories</c> lookup table.</summary>
    public Guid CategoryId { get; private set; }

    /// <summary>EF Core navigation — loaded on demand.</summary>
    public ProductCategory? Category { get; private set; }

    // ── Media ─────────────────────────────────────────────────────────────────

    public string Thumbnail { get; private set; } = string.Empty;

    // ── Commerce ──────────────────────────────────────────────────────────────

    public string Sku { get; private set; } = string.Empty;
    public string Barcode { get; private set; } = string.Empty;
    public int MinimumOrderQuantity { get; private set; }
    public string WarrantyInformation { get; private set; } = string.Empty;
    public string ShippingInformation { get; private set; } = string.Empty;
    public string AvailabilityStatus { get; private set; } = string.Empty;
    public string ReturnPolicy { get; private set; } = string.Empty;

    // ── IAuditable ────────────────────────────────────────────────────────────
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ── ISoftDelete ───────────────────────────────────────────────────────────
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // ── IConcurrent ───────────────────────────────────────────────────────────
    public Guid ConcurrencyStamp { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Product"/> and raises a
    /// <see cref="ProductCreatedEvent"/> domain event.
    /// </summary>
    /// <param name="categoryId">PK of the <see cref="ProductCategory"/> row.</param>
    public static Result<Product> Create(
        string title,
        string description,
        decimal price,
        decimal discountPercentage,
        int stock,
        string brand,
        Guid categoryId,
        string thumbnail,
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
            0, stock, brand, categoryId, thumbnail,
            sku, barcode, minimumOrderQuantity,
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

    /// <summary>Updates the aggregate rating (called after a review is added/removed).</summary>
    public void UpdateRating(double newRating)
    {
        Rating = newRating;
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

    // Called by persistence interceptors for auditing
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
