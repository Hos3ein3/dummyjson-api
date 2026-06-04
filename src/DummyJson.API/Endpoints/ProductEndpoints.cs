using Api.Extensions;
using DummyJson.Application.Common.Dispatcher;
using DummyJson.Application.Products.Commands;
using DummyJson.Application.Products.Queries;
using Application.Common.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Results;

namespace DummyJson.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products").WithTags("Products");

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? category,
            string? search,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var query = new GetProductsQuery(page ?? 1, pageSize ?? 30, category, search);
            var result = await dispatcher.QueryAsync<GetProductsQuery, Result<PagedList<ProductDto>>>(query, ct);
            return result.ToIResult(context);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var query = new GetProductByIdQuery(id);
            var result = await dispatcher.QueryAsync<GetProductByIdQuery, Result<ProductDto>>(query, ct);
            return result.ToIResult(context);
        });

        group.MapGet("/categories", async (
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var query = new GetProductCategoriesQuery();
            var result = await dispatcher.QueryAsync<GetProductCategoriesQuery, Result<IReadOnlyList<string>>>(query, ct);
            return result.ToIResult(context);
        });

        group.MapPost("/", async (
            CreateProductCommand command,
            FluentValidation.IValidator<CreateProductCommand> validator,
            global::Application.Common.Errors.IErrorFactory errorFactory,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateToResultAsync(command, errorFactory, ct);
            if (validationResult.IsFailure) return validationResult.ToIResult(context);

            var result = await dispatcher.SendAsync<CreateProductCommand, Result<Guid>>(command, ct);
            if (result.IsFailure) return result.ToIResult(context);

            return result.ToCreatedIResult(context, $"/api/v1/products/{result.Value}");
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductCommand command,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            if (id != command.Id) return Results.BadRequest();

            var result = await dispatcher.SendAsync<UpdateProductCommand, Result>(command, ct);
            return result.ToIResult(context);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync<DeleteProductCommand, Result>(new DeleteProductCommand(id), ct);
            return result.ToIResult(context);
        }).RequireAuthorization();
    }
}
