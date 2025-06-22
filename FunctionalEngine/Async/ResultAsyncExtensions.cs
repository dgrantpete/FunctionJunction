using System.Collections.Immutable;
using static FunctionalEngine.Result;
using static FunctionalEngine.Prelude;
using FunctionalEngine.Extensions;

namespace FunctionalEngine.Async;

/// <summary>
/// Provides asynchronous extension methods for <see cref="Result{TOk, TError}"/> and related async operations.
/// These methods handle async scenarios like sequencing async results, converting to async enumerables, and combining async result collections.
/// </summary>
public static partial class ResultAsyncExtensions
{
    /// <summary>
    /// Sequences a <see cref="Result{TOk, TError}"/> containing a <see cref="Task{T}"/> in the success position into a <see cref="Task{T}"/> containing a <see cref="Result{TOk, TError}"/>.
    /// This transforms <c>Result&lt;Task&lt;TOk&gt;, TError&gt;</c> into <c>Task&lt;Result&lt;TOk, TError&gt;&gt;</c>, allowing you to await the result's inner task.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns a completed <see cref="Task{T}"/> containing the error.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value inside the task.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a task to sequence.</param>
    /// <returns>A <see cref="Task{T}"/> that completes with a <see cref="Result{TOk, TError}"/> containing the task's result, or the original error.</returns>
    public static Task<Result<TOk, TError>> Sequence<TOk, TError>(this Result<Task<TOk>, TError> result) =>
        result.Map(Identity);

    /// <summary>
    /// Sequences a <see cref="Result{TOk, TError}"/> containing a <see cref="Task{T}"/> in the error position into a <see cref="Task{T}"/> containing a <see cref="Result{TOk, TError}"/>.
    /// This transforms <c>Result&lt;TOk, Task&lt;TError&gt;&gt;</c> into <c>Task&lt;Result&lt;TOk, TError&gt;&gt;</c>, allowing you to await the result's inner error task.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns a completed <see cref="Task{T}"/> containing the success value.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value inside the task.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error task to sequence.</param>
    /// <returns>A <see cref="Task{T}"/> that completes with a <see cref="Result{TOk, TError}"/> containing the original success or the task's error.</returns>
    public static Task<Result<TOk, TError>> Sequence<TOk, TError>(this Result<TOk, Task<TError>> result) =>
        result.MapError(Identity);

    /// <summary>
    /// Converts a <see cref="Task{T}"/> containing a <see cref="Result{TOk, TError}"/> into an <see cref="IAsyncEnumerable{T}"/> of success values.
    /// If the task's <see cref="Result{TOk, TError}"/> is <c>Ok</c>, yields the single success value. If <c>Error</c>, yields nothing.
    /// Useful for integrating async results into async enumerable processing pipelines.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="resultTask">The task containing a result to convert.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the success value if present, or nothing if an error.</returns>
    public static async IAsyncEnumerable<TOk> ToAsyncEnumerable<TOk, TError>(this Task<Result<TOk, TError>> resultTask)
    {
        var result = await resultTask;

        if (result.TryUnwrap(out var ok))
        {
            yield return ok;
        }
    }

    /// <summary>
    /// Converts a <see cref="Task{T}"/> containing a <see cref="Result{TOk, TError}"/> into an <see cref="IAsyncEnumerable{T}"/> of error values.
    /// If the task's <see cref="Result{TOk, TError}"/> is <c>Error</c>, yields the single error value. If <c>Ok</c>, yields nothing.
    /// Useful for extracting and processing error values from async result collections.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="resultTask">The task containing a result to convert.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the error value if present, or nothing if successful.</returns>
    public static async IAsyncEnumerable<TError> ToErrorAsyncEnumerable<TOk, TError>(this Task<Result<TOk, TError>> resultTask)
    {
        var result = await resultTask;

        if (result.TryUnwrapError(out var error))
        {
            yield return error;
        }
    }

    /// <summary>
    /// Asynchronously combines a sequence of <see cref="Result{TOk, TError}"/> values into a single result containing a list of all success values.
    /// If all results are successful, returns <c>Ok</c> containing all success values. If any result is an error, returns the first error encountered.
    /// This provides "all-or-nothing" semantics for async result collections.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="results">The async sequence of results to combine.</param>
    /// <returns>A <see cref="Task{T}"/> containing either <c>Ok</c> with all success values, or the first error encountered.</returns>
    public static async Task<Result<IImmutableList<TOk>, TError>> All<TOk, TError>(IAsyncEnumerable<Result<TOk, TError>> results) =>
        await results
            .Scan(
                Ok<IImmutableList<TOk>, TError>([]),
                (previousResults, result) =>
                    previousResults.And(() => result)
                        .MapTuple((previousOks, ok) => previousOks.Add(ok))
            )
            .TakeWhileInclusive(result => result.IsOk)
            .LastAsync();

    /// <summary>
    /// Asynchronously combines results from a collection of async result providers into a single result containing a list of all success values.
    /// Each provider function is executed and awaited. If all results are successful, returns <c>Ok</c> containing all success values.
    /// If any result is an error, returns the first error encountered.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="resultProvidersAsync">The collection of async functions that provide results when executed.</param>
    /// <returns>A <see cref="Task{T}"/> containing either <c>Ok</c> with all success values, or the first error encountered.</returns>
    public static Task<Result<IImmutableList<TOk>, TError>> All<TOk, TError>(params IEnumerable<Func<Task<Result<TOk, TError>>>> resultProvidersAsync) =>
        All(
            resultProvidersAsync.ToAsyncEnumerable()
                .Select(async (Func<Task<Result<TOk, TError>>> resultProvider, CancellationToken _) => await resultProvider())
        );

    /// <summary>
    /// Asynchronously finds the first successful result from a sequence of <see cref="Result{TOk, TError}"/> values.
    /// If any result is successful, returns the first success value found. If all results are errors, returns an error containing all error values.
    /// This provides "first-success" semantics for async result collections.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="results">The async sequence of results to search.</param>
    /// <returns>A <see cref="Task{T}"/> containing either the first success value, or all error values if none succeed.</returns>
    public static async Task<Result<TOk, IImmutableList<TError>>> Any<TOk, TError>(IAsyncEnumerable<Result<TOk, TError>> results) =>
        await results
            .Scan(
                Error<TOk, IImmutableList<TError>>([]),
                (previousResults, result) =>
                    previousResults.Or(() => result)
                        .MapErrorTuple((previousErrors, error) => previousErrors.Add(error))
            )
            .TakeWhileInclusive(result => result.IsError)
            .LastAsync();

    /// <summary>
    /// Asynchronously finds the first successful result from a collection of async result providers.
    /// Each provider function is executed and awaited. If any result is successful, returns the first success value found.
    /// If all results are errors, returns an error containing all error values.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="resultProvidersAsync">The collection of async functions that provide results when executed.</param>
    /// <returns>A <see cref="Task{T}"/> containing either the first success value, or all error values if none succeed.</returns>
    public static Task<Result<TOk, IImmutableList<TError>>> Any<TOk, TError>(params IEnumerable<Func<Task<Result<TOk, TError>>>> resultProvidersAsync) =>
        Any(
            resultProvidersAsync.ToAsyncEnumerable()
                .Select(async (Func<Task<Result<TOk, TError>>> resultProvider, CancellationToken _) => await resultProvider())
        );
}
