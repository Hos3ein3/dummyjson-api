using Refit;

namespace DummyJson.Application.Common.Interfaces;

public interface ISampleRefitClient
{
    [Get("/todos/1")]
    Task<TodoDto> GetSampleTodoAsync(CancellationToken cancellationToken = default);

    [Get("/posts/{id}")]
    Task<PostDto> GetPostAsync(int id, CancellationToken cancellationToken = default);
}

public record TodoDto(int UserId, int Id, string Title, bool Completed);

public record PostDto(int UserId, int Id, string Title, string Body);
