using FunctionalEngine.Generator;

namespace FunctionalEngine.Extensions;

#pragma warning disable CS1591

/// <summary>
/// Provides extension methods for <see cref="Result{TOk, TError}"/> that enable working with tuple values more ergonomically.
/// These methods allow mapping, flat-mapping, and error handling operations to work directly with tuple elements instead of requiring manual deconstruction.
/// </summary>
[GenerateAsyncExtension(ExtensionClassName = "ResultAsyncExtensions", Namespace = "FunctionalEngine.Async")]
public static class ResultExtensions
{
    /// <summary>
    /// Applies a function that returns a <see cref="Result{TOk, TError}"/> to the tuple elements inside this <see cref="Result{TOk, TError}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <typeparam name="TResult">The type of the success value in the result returned by the mapper.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns a <c>Result&lt;TResult, TError&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the mapper if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, TError, TResult>(this Result<(T1, T2), TError> result, Func<T1, T2, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, T3, TError, TResult>(this Result<(T1, T2, T3), TError> result, Func<T1, T2, T3, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, T3, T4, TError, TResult>(this Result<(T1, T2, T3, T4), TError> result, Func<T1, T2, T3, T4, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the transformed value if <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TResult, TError> MapTuple<T1, T2, TError, TResult>(this Result<(T1, T2), TError> result, Func<T1, T2, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension]
    public static Result<TResult, TError> MapTuple<T1, T2, T3, TError, TResult>(this Result<(T1, T2, T3), TError> result, Func<T1, T2, T3, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension]
    public static Result<TResult, TError> MapTuple<T1, T2, T3, T4, TError, TResult>(this Result<(T1, T2, T3, T4), TError> result, Func<T1, T2, T3, T4, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, TOk, TResult>(this Result<TOk, (T1, T2)> result, Func<T1, T2, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, T3, TOk, TResult>(this Result<TOk, (T1, T2, T3)> result, Func<T1, T2, T3, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, T3, T4, TOk, TResult>(this Result<TOk, (T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, TOk, TResult>(this Result<TOk, (T1, T2)> result, Func<T1, T2, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, T3, TOk, TResult>(this Result<TOk, (T1, T2, T3)> result, Func<T1, T2, T3, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, T3, T4, TOk, TResult>(this Result<TOk, (T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3), TError> Coalesce<T1, T2, T3, TError>(this Result<((T1, T2), T3), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3), TError> Coalesce<T1, T2, T3, TError>(this Result<(T1, (T2, T3)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2), T3, T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2, T3), T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, (T2, T3), T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, (T2, T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, T2, (T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2), (T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3)> CoalesceError<T1, T2, T3, TOk>(this Result<TOk, ((T1, T2), T3)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3)> CoalesceError<T1, T2, T3, TOk>(this Result<TOk, (T1, (T2, T3))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2), T3, T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2, T3), T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, (T2, T3), T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, (T2, T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, T2, (T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2), (T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());
}
