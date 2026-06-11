using DummyJson.Application.Products.Queries;
using DummyJson.Domain.Products;

namespace DummyJson.Application.Common.Repository;

public interface IProductRepository: IRepository<Product,Guid>
{
    ProductDto MapToDto(Product p);
}