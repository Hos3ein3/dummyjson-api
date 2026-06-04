using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class CartsEndpoints
{
    public static void MapCartsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/carts").WithTags("Carts").RequireAuthorization();

        group.MapGet("/", (int? page, int? pageSize) =>
            Results.Ok(new { message = "GetCarts endpoint — wire up handler", page = page ?? 1, pageSize = pageSize ?? 30 }));

        group.MapGet("/{id:guid}", (Guid id) =>
            Results.Ok(new { message = "GetCartById endpoint — wire up handler", id }));

        group.MapPost("/", (CreateCartRequest request) =>
            Results.Ok(new { message = "CreateCart endpoint — wire up handler", request }));

        group.MapPut("/{id:guid}", (Guid id, UpdateCartRequest request) =>
            Results.NoContent());

        group.MapDelete("/{id:guid}", (Guid id) =>
            Results.NoContent());
    }
}

public sealed record CreateCartRequest(Guid UserId, List<CartItemRequest> Products);
public sealed record UpdateCartRequest(List<CartItemRequest> Products);
public sealed record CartItemRequest(Guid ProductId, int Quantity);
