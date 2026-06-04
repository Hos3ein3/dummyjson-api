using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("Users");

        group.MapGet("/", (int? page, int? pageSize) =>
            Results.Ok(new { message = "GetUsers endpoint — wire up handler", page = page ?? 1, pageSize = pageSize ?? 30 }))
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        group.MapGet("/{id:guid}", (Guid id) =>
            Results.Ok(new { message = "GetUserById endpoint — wire up handler", id }))
        .RequireAuthorization();

        group.MapGet("/search", (string q) =>
            Results.Ok(new { message = "SearchUsers endpoint — wire up handler", q }));

        group.MapPut("/{id:guid}", (Guid id, UpdateUserRequest request) =>
            Results.NoContent())
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", (Guid id) =>
            Results.NoContent())
        .RequireAuthorization(policy => policy.RequireRole("admin"));
    }
}

public sealed record UpdateUserRequest(
    string FirstName, string LastName, string Phone,
    string? Image, string? Street, string? City,
    string? State, string? PostalCode, string? Country);
