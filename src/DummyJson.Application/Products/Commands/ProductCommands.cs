using DummyJson.Application.Common.CQRS;
using SharedKernel.Results;

namespace DummyJson.Application.Products.Commands;

// ── Create ───────────────────────────────────────────────────────────────────

public sealed record CreateProductCommand(
    string Title,
    string Description,
    decimal Price,
    decimal DiscountPercentage,
    int Stock,
    string Brand,
    string Category,
    string Thumbnail,
    List<string> Images,
    List<string> Tags,
    string Sku = "",
    string Barcode = "",
    int MinimumOrderQuantity = 1,
    string WarrantyInformation = "",
    string ShippingInformation = "",
    string AvailabilityStatus = "In Stock",
    string ReturnPolicy = "") : ICommand<Result<Guid>>;

// ── Update ───────────────────────────────────────────────────────────────────

public sealed record UpdateProductCommand(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    int Stock) : ICommand<Result>;

// ── Delete ───────────────────────────────────────────────────────────────────

public sealed record DeleteProductCommand(Guid Id) : ICommand<Result>;

// ── Restore ──────────────────────────────────────────────────────────────────

public sealed record RestoreProductCommand(Guid Id) : ICommand<Result>;
