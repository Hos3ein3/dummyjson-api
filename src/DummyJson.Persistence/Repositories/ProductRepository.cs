using DummyJson.Application.Common.Repository;
using DummyJson.Application.Products.Queries;
using DummyJson.Domain.Products;
using DummyJson.Persistence.Context;

namespace DummyJson.Persistence.Repositories;

public sealed class ProductRepository:GenericRepository<Product,Guid>,IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }
    public  ProductDto MapToDto(Product p)
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