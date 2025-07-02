using static FunctionJunction.Prelude;

namespace FunctionJunction.Async;

/// <summary>
/// Provides asynchronous extension methods for <see cref="Result{TOk, TError}"/> and related async operations.
/// These methods handle async scenarios like sequencing async results, converting to async enumerables, and combining async result collections.
/// </summary>
public static partial class ResultAsyncExtensions
{
    /// <summary>
    /// Sequences a <see cref="Result{TOk, TError}"/> containing a <see cref="ValueTask{T}"/> in the success position into a <see cref="ValueTask{T}"/> containing a <see cref="Result{TOk, TError}"/>.
    /// This transforms <c>Result&lt;ValueTask&lt;TOk&gt;, TError&gt;</c> into <c>ValueTask&lt;Result&lt;TOk, TError&gt;&gt;</c>, allowing you to await the result's inner task.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns a completed <see cref="ValueTask{T}"/> containing the error.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value inside the task.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a task to sequence.</param>
    /// <returns>A <see cref="ValueTask{T}"/> that completes with a <see cref="Result{TOk, TError}"/> containing the task's result, or the original error.</returns>
    public static ValueTask<Result<TOk, TError>> Sequence<TOk, TError>(this Result<ValueTask<TOk>, TError> result) =>
        result.Map(Identity);

    /// <summary>
    /// Sequences a <see cref="Result{TOk, TError}"/> containing a <see cref="ValueTask{T}"/> in the error position into a <see cref="ValueTask{T}"/> containing a <see cref="Result{TOk, TError}"/>.
    /// This transforms <c>Result&lt;TOk, ValueTask&lt;TError&gt;&gt;</c> into <c>ValueTask&lt;Result&lt;TOk, TError&gt;&gt;</c>, allowing you to await the result's inner error task.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns a completed <see cref="ValueTask{T}"/> containing the success value.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value inside the task.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error task to sequence.</param>
    /// <returns>A <see cref="ValueTask{T}"/> that completes with a <see cref="Result{TOk, TError}"/> containing the original success or the task's error.</returns>
    public static ValueTask<Result<TOk, TError>> Sequence<TOk, TError>(this Result<TOk, ValueTask<TError>> result) =>
        result.MapError(Identity);

    /// <summary>
    /// Converts a <see cref="ValueTask{T}"/> containing a <see cref="Result{TOk, TError}"/> into an <see cref="IAsyncEnumerable{T}"/> of success values.
    /// If the task's <see cref="Result{TOk, TError}"/> is <c>Ok</c>, yields the single success value. If <c>Error</c>, yields nothing.
    /// Useful for integrating async results into async enumerable processing pipelines.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="resultTask">The task containing a result to convert.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the success value if present, or nothing if an error.</returns>
    public static async IAsyncEnumerable<TOk> ToAsyncEnumerable<TOk, TError>(this ValueTask<Result<TOk, TError>> resultTask)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.TryUnwrap(out var ok))
        {
            yield return ok;
        }
    }

    /// <summary>
    /// Converts a <see cref="ValueTask{T}"/> containing a <see cref="Result{TOk, TError}"/> into an <see cref="IAsyncEnumerable{T}"/> of error values.
    /// If the task's <see cref="Result{TOk, TError}"/> is <c>Error</c>, yields the single error value. If <c>Ok</c>, yields nothing.
    /// Useful for extracting and processing error values from async result collections.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="resultTask">The task containing a result to convert.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the error value if present, or nothing if successful.</returns>
    public static async IAsyncEnumerable<TError> ToErrorAsyncEnumerable<TOk, TError>(this ValueTask<Result<TOk, TError>> resultTask)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.TryUnwrapError(out var error))
        {
            yield return error;
        }
    }
}

/// <summary>
/// Asyncronous extension methods specific to <see cref="Result{TOk, TError}"/> which contain value types.
/// </summary>
public static partial class ValueResultAsyncExtensions
{

}
