using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Products;
using DummyJson.Persistence.Context;

namespace DummyJson.Persistence.Repositories;

public sealed class ProductImageRepository : MongoRepository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(MongoDbContext dbContext) : base(dbContext)
    {
    }
}
