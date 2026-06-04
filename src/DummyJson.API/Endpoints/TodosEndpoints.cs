using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class TodosEndpoints
{
    public static void MapTodosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/todos").WithTags("Todos");

        group.MapGet("/", (int? page, int? pageSize) =>
            Results.Ok(new { message = "GetTodos endpoint — wire up handler", page = page ?? 1, pageSize = pageSize ?? 30 }));

        group.MapGet("/{id:guid}", (Guid id) =>
            Results.Ok(new { message = "GetTodoById endpoint — wire up handler", id }));

        group.MapPost("/", (CreateTodoRequest request) =>
            Results.Ok(new { message = "CreateTodo endpoint — wire up handler", request }))
        .RequireAuthorization();

        group.MapPut("/{id:guid}", (Guid id, UpdateTodoRequest request) =>
            Results.NoContent())
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", (Guid id) =>
            Results.NoContent())
        .RequireAuthorization();
    }
}

public sealed record CreateTodoRequest(string Todo, Guid UserId, bool Completed);
public sealed record UpdateTodoRequest(string Todo, bool Completed);
