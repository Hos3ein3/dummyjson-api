using DummyJson.Application.Products.Queries;
using SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DummyJson.Application.Common.Interfaces;

public interface IProductQueryRepository
{
    Task<PagedList<ProductDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetProductCategoriesAsync(CancellationToken cancellationToken = default);
}
