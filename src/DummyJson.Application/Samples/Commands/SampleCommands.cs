using DummyJson.Application.Common.CQRS;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Domain.Todos;
using SharedKernel.Results;

namespace DummyJson.Application.Samples.Commands;

public record NormalCommand(string Title) : ICommand<Result<Guid>>;
public record TransactionalCommand(string Title) : ICommand<Result<Guid>>;

public class NormalCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<NormalCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(NormalCommand request, CancellationToken cancellationToken)
    {
        var todoResult = Todo.Create(Guid.NewGuid(), request.Title);
        if (todoResult.IsFailure) return Result.Failure<Guid>(todoResult.Error);

        var todo = todoResult.Value;
        await unitOfWork.Repository<Todo, Guid>().AddAsync(todo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(todo.Id);
    }
}

public class TransactionalSampleCommandHandler(IUnitOfWork unitOfWork) 
    : TransactionalCommandHandler<TransactionalCommand, Result<Guid>>(unitOfWork)
{
    protected override async Task<Result<Guid>> HandleTransactionalAsync(TransactionalCommand request, CancellationToken cancellationToken)
    {
        var todoResult = Todo.Create(Guid.NewGuid(), request.Title);
        if (todoResult.IsFailure) return Result.Failure<Guid>(todoResult.Error);

        var todo = todoResult.Value;
        await UnitOfWork.Repository<Todo, Guid>().AddAsync(todo, cancellationToken);
        
        // This save changes is within the transaction.
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        
        // Simulating a failure that should trigger a rollback
        if (request.Title == "fail")
        {
            throw new Exception("Simulated failure to trigger rollback.");
        }

        return Result.Success(todo.Id);
    }
}
