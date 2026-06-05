using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Products.Queries;
using DummyJson.Domain.Products;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DummyJson.Persistence.Repositories;

public sealed class ProductQueryRepository : IProductQueryRepository
{
    private readonly AppDbContext _context;

    public ProductQueryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<ProductDto>> GetProductsAsync(GetProductsQuery request, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Product>()
            .Include(p => p.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category != null && p.Category.Slug == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(search) || p.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = products.Select(MapToDto).ToList();

        return new PagedList<ProductDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
            return null;

        return MapToDto(product);
    }

    public async Task<IReadOnlyList<string>> GetProductCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Set<ProductCategory>()
            .Select(c => c.Slug)
            .Distinct()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return categories;
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto(
            p.Id,
            p.Title,
            p.Description,
            p.Price,
            p.DiscountPercentage,
            p.Rating,
            p.Stock,
            p.Brand,
            p.Category?.Slug ?? "",
            p.Thumbnail,
            new List<string>(), // Images are in MongoDB, fetched separately if needed
            new List<string>(), // Tags are fetched separately from MongoDB if needed, but for now returned empty
            p.Sku,
            p.AvailabilityStatus,
            p.CreatedAt,
            p.UpdatedAt
        );
    }
}
