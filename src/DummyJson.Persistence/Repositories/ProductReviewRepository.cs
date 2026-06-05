using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Products;
using DummyJson.Persistence.Context;

namespace DummyJson.Persistence.Repositories;

public sealed class ProductReviewRepository : MongoRepository<ProductReview>, IProductReviewRepository
{
    public ProductReviewRepository(MongoDbContext context) : base(context)
    {
    }
}
