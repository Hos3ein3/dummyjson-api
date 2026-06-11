using DummyJson.Application.Common.CQRS;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Products.Queries;
using SharedKernel.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Repository;

namespace DummyJson.Application.Products.Handlers;

public sealed class ProductQueryHandlers :
    IQueryHandler<GetProductsQuery, Result<PagedList<ProductDto>>>,
    IQueryHandler<GetProductByIdQuery, Result<ProductDto>>,
    IQueryHandler<GetProductCategoriesQuery, Result<IReadOnlyList<string>>>
{
    private readonly IProductQueryRepository _queryRepo;
    
private readonly IProductRepository _repo;

    public ProductQueryHandlers(IProductQueryRepository queryRepo, IProductRepository repo)
    {
        _queryRepo = queryRepo;
        _repo = repo;
    }
    public async Task<Result<PagedList<ProductDto>>> HandleAsync(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repo.GetPagedResultByOffsetAsync(request.skip, request.limit, cancellationToken);

        return Result.Success(result).Map(paged =>
        {
            var items = paged.Items.Select(_repo.MapToDto).ToList();
            return new PagedList<ProductDto>(
                items,
                paged.Page,
                paged.PageSize,
                paged.TotalCount);
        });
    }

    public async Task<Result<PagedList<ProductDto>>> HandleAsync(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var result = await _repo.GetPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(result).Map(paged =>
        {
            var items = paged.Items.Select(_repo.MapToDto).ToList();
            return new PagedList<ProductDto>(
                items,
                paged.Page,
                paged.PageSize,
                paged.TotalCount);
        });
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
