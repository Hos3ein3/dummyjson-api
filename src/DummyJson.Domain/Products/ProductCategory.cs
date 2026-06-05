using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Domain.Products;

/// <summary>
/// Product Category — a normalised lookup table for product categories.
///
/// <para>
/// Categories are extracted into their own table so that:
/// <list type="bullet">
///   <item>Category names can be renamed without touching every product row.</item>
///   <item>Category slugs (URL-friendly identifiers) are stored once.</item>
///   <item>A future category hierarchy (parent/child) can be modelled without migration headaches.</item>
/// </list>
/// Products reference this via <see cref="Product.CategoryId"/>.
/// </para>
/// </summary>
public sealed class ProductCategory : Entity<Guid>
{
    private ProductCategory() { }  // EF Core

    private ProductCategory(Guid id, string name, string slug) : base(id)
    {
        Name = name;
        Slug = slug;
    }

    /// <summary>Display name, e.g. "Kitchen Accessories".</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug, e.g. "kitchen-accessories".
    /// Matches the raw category value in the DummyJSON seed data.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<ProductCategory> Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductCategory>(Error.Validation(nameof(name), "Category name cannot be empty."));

        if (string.IsNullOrWhiteSpace(slug))
            return Result.Failure<ProductCategory>(Error.Validation(nameof(slug), "Category slug cannot be empty."));

        return Result.Success(new ProductCategory(Guid.CreateVersion7(), name.Trim(), slug.Trim().ToLowerInvariant()));
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Renames the category display name without changing the slug.</summary>
    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(Error.Validation(nameof(newName), "Category name cannot be empty."));

        Name = newName.Trim();
        return Result.Success();
    }

    public override string ToString() => Slug;
}
