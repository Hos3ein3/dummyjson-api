namespace SharedKernel.Results;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Successful result cannot contain an error.");
        
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failed result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);

    /// <summary>
    /// Evaluates the result and executes the appropriate function based on success or failure.
    /// </summary>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    public Result Tap(Action action)
    {
        if (IsSuccess) action();
        return this;
    }

    /// <summary>
    /// Executes the given action with the error if the result is a failure.
    /// </summary>
    public Result TapError(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    /// <summary>
    /// Ensures that the specified condition is met; otherwise returns a failure with the specified error.
    /// </summary>
    public Result Ensure(Func<bool> predicate, Error error)
    {
        if (IsFailure) return this;
        return predicate() ? this : Failure(error);
    }

    /// <summary>
    /// Transforms the error of a failed result into a new error.
    /// </summary>
    public Result MapError(Func<Error, Error> mapper) =>
        IsFailure ? Failure(mapper(Error)) : this;

    /// <summary>
    /// Combines multiple results into a single result. If any result fails, it returns a validation summary.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failedResults = results.Where(x => x.IsFailure).ToList();
        
        if (failedResults.Count == 0) return Success();
        if (failedResults.Count == 1) return failedResults[0];

        var errors = failedResults.Select(x => x.Error).ToList();
        return Failure(Error.ValidationSummary("General.MultipleErrors", "Multiple errors occurred.", errors));
    }

    /// <summary>
    /// Executes the given action inside a try-catch block and returns a Result.
    /// </summary>
    public static Result Try(Action action, Func<Exception, Error>? onCatch = null)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(onCatch?.Invoke(ex) ?? CommonErrors.Unexpected(ex.Message));
        }
    }

    /// <summary>
    /// Executes the given async action inside a try-catch block and returns a Result.
    /// </summary>
    public static async Task<Result> TryAsync(Func<Task> func, Func<Exception, Error>? onCatch = null)
    {
        try
        {
            await func();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(onCatch?.Invoke(ex) ?? CommonErrors.Unexpected(ex.Message));
        }
    }

    /// <summary>
    /// Retries an operation returning a Result up to the specified number of times if it fails.
    /// </summary>
    public static async Task<Result<T>> RetryAsync<T>(Func<Task<Result<T>>> operation, int maxAttempts, TimeSpan delay)
    {
        Result<T>? lastResult = null;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            lastResult = await operation();
            if (lastResult.IsSuccess) return lastResult;

            if (attempt < maxAttempts)
            {
                await Task.Delay(delay);
            }
        }
        return lastResult ?? Result.Failure<T>(Error.Failure("Retry.Failed", "Operation failed after maximum retries."));
    }
}
