namespace SharedKernel.Results;

/// <summary>
/// Represents the result of an operation that returns a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Success(mapper(Value)) : Failure<TOut>(Error);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess ? binder(Value) : Failure<TOut>(Error);

    public Result Bind(Func<T, Result> binder) =>
        IsSuccess ? binder(Value) : Failure(Error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess) action(Value);
        return this;
    }

    public new Result<T> TapError(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (IsFailure) return this;
        return predicate(Value) ? this : Failure<T>(error);
    }

    public new Result<T> MapError(Func<Error, Error> mapper) =>
        IsFailure ? Failure<T>(mapper(Error)) : this;

    public static Result<T> Try(Func<T> func, Func<Exception, Error>? onCatch = null)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure<T>(onCatch?.Invoke(ex) ?? CommonErrors.Unexpected(ex.Message));
        }
    }

    public static async Task<Result<T>> TryAsync(Func<Task<T>> func, Func<Exception, Error>? onCatch = null)
    {
        try
        {
            var value = await func();
            return Success(value);
        }
        catch (Exception ex)
        {
            return Failure<T>(onCatch?.Invoke(ex) ?? CommonErrors.Unexpected(ex.Message));
        }
    }
}
