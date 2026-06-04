using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Todos;

/// <summary>
/// Todo aggregate root — corresponds to DummyJSON /todos resource.
/// </summary>
public sealed class Todo : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private Todo() { }

    private Todo(Guid id, Guid userId, string todo, bool completed) : base(id)
    {
        UserId = userId;
        TodoText = todo;
        Completed = completed;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string TodoText { get; private set; } = string.Empty;
    public bool Completed { get; private set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public static Result<Todo> Create(Guid userId, string todo)
    {
        if (string.IsNullOrWhiteSpace(todo))
            return Result.Failure<Todo>(Error.Validation(nameof(todo), "Todo text cannot be empty."));

        return Result.Success(new Todo(Guid.CreateVersion7(), userId, todo, false));
    }

    public void Complete() { Completed = true; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Uncomplete() { Completed = false; UpdatedAt = DateTimeOffset.UtcNow; }

    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
