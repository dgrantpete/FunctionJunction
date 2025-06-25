using FunctionJunction.Generator;

namespace FunctionJunction.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Result{TOk, TError}"/> that enable working with tuple values more ergonomically.
/// These methods allow mapping, flat-mapping, and error handling operations to work directly with tuple elements instead of requiring manual deconstruction.
/// </summary>
[GenerateAsyncExtension(ExtensionClassName = "ResultAsyncExtensions", Namespace = "FunctionJunction.Async")]
public static class ResultExtensions
{
    /// <summary>
    /// Applies a function that returns a <see cref="Result{TOk, TError}"/> to the tuple elements inside this <see cref="Result{TOk, TError}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns a <c>Result&lt;TResult, TError&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the mapper if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, TError, TResult>(this Result<(T1, T2), TError> result, Func<T1, T2, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2));

    /// <summary>
    /// Applies a function that returns a <see cref="Result{TOk, TError}"/> to the tuple elements inside this <see cref="Result{TOk, TError}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns a <c>Result&lt;TResult, TError&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the mapper if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, T3, TError, TResult>(this Result<(T1, T2, T3), TError> result, Func<T1, T2, T3, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    /// <summary>
    /// Applies a function that returns a <see cref="Result{TOk, TError}"/> to the tuple elements inside this <see cref="Result{TOk, TError}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns a <c>Result&lt;TResult, TError&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the mapper if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, T3, T4, TError, TResult>(this Result<(T1, T2, T3, T4), TError> result, Func<T1, T2, T3, T4, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the transformed value if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> MapTuple<T1, T2, TError, TResult>(this Result<(T1, T2), TError> result, Func<T1, T2, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2));

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the transformed value if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> MapTuple<T1, T2, T3, TError, TResult>(this Result<(T1, T2, T3), TError> result, Func<T1, T2, T3, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the transformed value if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> MapTuple<T1, T2, T3, T4, TError, TResult>(this Result<(T1, T2, T3, T4), TError> result, Func<T1, T2, T3, T4, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    /// <summary>
    /// Transforms the error tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with error tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <param name="mapper">A function that transforms the error tuple elements to a new error value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> with the transformed error if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, TOk, TResult>(this Result<TOk, (T1, T2)> result, Func<T1, T2, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2));

    /// <summary>
    /// Transforms the error tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with error tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <param name="mapper">A function that transforms the error tuple elements to a new error value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> with the transformed error if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, T3, TOk, TResult>(this Result<TOk, (T1, T2, T3)> result, Func<T1, T2, T3, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    /// <summary>
    /// Transforms the error tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with error tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <param name="mapper">A function that transforms the error tuple elements to a new error value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> with the transformed error if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, T3, T4, TOk, TResult>(this Result<TOk, (T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    /// <summary>
    /// Recovers from an error by applying a function to the error tuple elements that returns a new <see cref="Result{TOk, TError}"/>.
    /// This allows working with error tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to recover from.</param>
    /// <param name="recoverer">A function that takes the error tuple elements and returns a recovery <c>Result&lt;TOk, TResult&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the recoverer if this <see cref="Result{TOk, TError}"/> is <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, TOk, TResult>(this Result<TOk, (T1, T2)> result, Func<T1, T2, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2));

    /// <summary>
    /// Recovers from an error by applying a function to the error tuple elements that returns a new <see cref="Result{TOk, TError}"/>.
    /// This allows working with error tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to recover from.</param>
    /// <param name="recoverer">A function that takes the error tuple elements and returns a recovery <c>Result&lt;TOk, TResult&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the recoverer if this <see cref="Result{TOk, TError}"/> is <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, T3, TOk, TResult>(this Result<TOk, (T1, T2, T3)> result, Func<T1, T2, T3, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2, tuple.Item3));

    /// <summary>
    /// Recovers from an error by applying a function to the error tuple elements that returns a new <see cref="Result{TOk, TError}"/>.
    /// This allows working with error tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to recover from.</param>
    /// <param name="recoverer">A function that takes the error tuple elements and returns a recovery <c>Result&lt;TOk, TResult&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the recoverer if this <see cref="Result{TOk, TError}"/> is <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, T3, T4, TOk, TResult>(this Result<TOk, (T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3), TError> Coalesce<T1, T2, T3, TError>(this Result<((T1, T2), T3), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3), TError> Coalesce<T1, T2, T3, TError>(this Result<(T1, (T2, T3)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2), T3, T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2, T3), T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, (T2, T3), T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, (T2, T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, T2, (T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened tuple if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2), (T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3)> CoalesceError<T1, T2, T3, TOk>(this Result<TOk, ((T1, T2), T3)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3)> CoalesceError<T1, T2, T3, TOk>(this Result<TOk, (T1, (T2, T3))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2), T3, T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2, T3), T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, (T2, T3), T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, (T2, T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, T2, (T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested error tuple elements inside of this <see cref="Result{TOk, TError}"/> and transforms them into a single, flattened error tuple.
    /// </summary>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing an error tuple to transform.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the flattened error tuple if <c>Error</c>, otherwise the original success value.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2), (T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());
}
