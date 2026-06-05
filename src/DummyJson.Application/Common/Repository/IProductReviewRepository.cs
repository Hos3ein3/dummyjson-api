using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Products;

namespace DummyJson.Application.Common.Repository;

public interface IProductReviewRepository : IMongoRepository<ProductReview>
{
}
