using System;
using System.Threading.Tasks;

namespace SharedKernel.Results;

public static class ResultExtensions
{
    // ── Tap ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    public static Result Tap(this Result result, Action action)
    {
        if (result.IsSuccess) action();
        return result;
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes the given async action if the result is successful.
    /// </summary>
    public static async Task<Result> TapAsync(this Task<Result> resultTask, Func<Task> action)
    {
        var result = await resultTask;
        if (result.IsSuccess) await action();
        return result;
    }

    /// <summary>
    /// Executes the given async action if the result is successful.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask;
        if (result.IsSuccess) await action(result.Value);
        return result;
    }

    // ── Map ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps the value of a successful result to a new value.
    /// </summary>
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapping)
    {
        return result.IsSuccess ? Result.Success(mapping(result.Value)) : Result.Failure<U>(result.Error);
    }

    /// <summary>
    /// Maps the value of a successful result to a new value asynchronously.
    /// </summary>
    public static async Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<U>> mapping)
    {
        var result = await resultTask;
        return result.IsSuccess ? Result.Success(await mapping(result.Value)) : Result.Failure<U>(result.Error);
    }

    // ── Ensure ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Ensures that the specified condition is met; otherwise returns a failure with the specified error.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        if (result.IsFailure) return result;
        return predicate(result.Value) ? result : Result.Failure<T>(error);
    }

    /// <summary>
    /// Ensures that the specified condition is met asynchronously; otherwise returns a failure with the specified error.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error error)
    {
        var result = await resultTask;
        if (result.IsFailure) return result;
        return await predicate(result.Value) ? result : Result.Failure<T>(error);
    }

}
