using DummyJson.Application.Common.CQRS;
using SharedKernel.Results;

namespace DummyJson.Application.Products.Queries;

// ── Queries ──────────────────────────────────────────────────────────────────

public sealed record GetProductsPagedQuery(int Page = 1, int PageSize = 30, string? Category = null, string? Search = null)
    : IQuery<Result<PagedList<ProductDto>>>;

public sealed record GetProductsQuery(int skip,int limit,string? Category = null, string? Search = null)
    :IQuery<Result<PagedList<ProductDto>>>;

public sealed record GetProductByIdQuery(Guid Id)
    : IQuery<Result<ProductDto>>;

public sealed record GetProductCategoriesQuery()
    : IQuery<Result<IReadOnlyList<string>>>;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record ProductDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    decimal DiscountPercentage,
    double Rating,
    int Stock,
    string Brand,
    string Category,
    string Thumbnail,
    List<string> Images,
    List<string> Tags,
    string Sku,
    string AvailabilityStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

