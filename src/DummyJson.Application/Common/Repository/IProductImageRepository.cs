using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Products;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Repository for managing ProductImages in MongoDB.
/// </summary>
public interface IProductImageRepository : IMongoRepository<ProductImage>
{
}
