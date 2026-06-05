using DummyJson.Application.Common.CQRS;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Products.Queries;
using SharedKernel.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DummyJson.Application.Products.Handlers;

public sealed class ProductQueryHandlers :
    IQueryHandler<GetProductsQuery, Result<PagedList<ProductDto>>>,
    IQueryHandler<GetProductByIdQuery, Result<ProductDto>>,
    IQueryHandler<GetProductCategoriesQuery, Result<IReadOnlyList<string>>>
{
    private readonly IProductQueryRepository _queryRepo;

    public ProductQueryHandlers(IProductQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async Task<Result<PagedList<ProductDto>>> HandleAsync(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryRepo.GetProductsAsync(request, cancellationToken);
        return Result.Success(result);
    }

    public async Task<Result<ProductDto>> HandleAsync(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _queryRepo.GetProductByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure<ProductDto>(SharedKernel.Results.Error.NotFound("Product.NotFound", $"Product with ID {request.Id} was not found."));

        return Result.Success(product);
    }

    public async Task<Result<IReadOnlyList<string>>> HandleAsync(GetProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _queryRepo.GetProductCategoriesAsync(cancellationToken);
        return Result.Success(categories);
    }
}
