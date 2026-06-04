using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class PostsEndpoints
{
    public static void MapPostsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/posts").WithTags("Posts");

        group.MapGet("/", (int? page, int? pageSize) =>
            Results.Ok(new { message = "GetPosts endpoint — wire up handler", page = page ?? 1, pageSize = pageSize ?? 30 }));

        group.MapGet("/{id:guid}", (Guid id) =>
            Results.Ok(new { message = "GetPostById endpoint — wire up handler", id }));

        group.MapPost("/", (CreatePostRequest request) =>
            Results.Ok(new { message = "CreatePost endpoint — wire up handler", request }))
        .RequireAuthorization();

        group.MapPut("/{id:guid}", (Guid id, UpdatePostRequest request) =>
            Results.NoContent())
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", (Guid id) =>
            Results.NoContent())
        .RequireAuthorization();
    }
}

public sealed record CreatePostRequest(string Title, string Body, Guid UserId, List<string> Tags);
public sealed record UpdatePostRequest(string Title, string Body);
