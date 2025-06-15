using FunctionalEngine.Generator;
using static FunctionalEngine.GlobalDefaults;

namespace FunctionalEngine;

public static class ResultExtensions
{
    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, TError, TResult>(this Result<(T1, T2), TError> result, Func<T1, T2, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, T3, TError, TResult>(this Result<(T1, T2, T3), TError> result, Func<T1, T2, T3, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TResult, TError> FlatMapTuple<T1, T2, T3, T4, TError, TResult>(this Result<(T1, T2, T3, T4), TError> result, Func<T1, T2, T3, T4, Result<TResult, TError>> mapper) =>
        result.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TResult, TError> MapTuple<T1, T2, TError, TResult>(this Result<(T1, T2), TError> result, Func<T1, T2, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TResult, TError> MapTuple<T1, T2, T3, TError, TResult>(this Result<(T1, T2, T3), TError> result, Func<T1, T2, T3, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TResult, TError> MapTuple<T1, T2, T3, T4, TError, TResult>(this Result<(T1, T2, T3, T4), TError> result, Func<T1, T2, T3, T4, TResult> mapper) =>
        result.Map(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, TOk, TResult>(this Result<TOk, (T1, T2)> result, Func<T1, T2, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, T3, TOk, TResult>(this Result<TOk, (T1, T2, T3)> result, Func<T1, T2, T3, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, TResult> MapErrorTuple<T1, T2, T3, T4, TOk, TResult>(this Result<TOk, (T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, TResult> mapper) =>
        result.MapError(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, TOk, TResult>(this Result<TOk, (T1, T2)> result, Func<T1, T2, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, T3, TOk, TResult>(this Result<TOk, (T1, T2, T3)> result, Func<T1, T2, T3, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, TResult> RecoverTuple<T1, T2, T3, T4, TOk, TResult>(this Result<TOk, (T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Result<TOk, TResult>> recoverer) =>
        result.Recover(tuple => recoverer(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3), TError> Coalesce<T1, T2, T3, TError>(this Result<((T1, T2), T3), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3), TError> Coalesce<T1, T2, T3, TError>(this Result<(T1, (T2, T3)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2), T3, T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2, T3), T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, (T2, T3), T4), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, (T2, T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<(T1, T2, (T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<(T1, T2, T3, T4), TError> Coalesce<T1, T2, T3, T4, TError>(this Result<((T1, T2), (T3, T4)), TError> result) =>
        result.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3)> CoalesceError<T1, T2, T3, TOk>(this Result<TOk, ((T1, T2), T3)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3)> CoalesceError<T1, T2, T3, TOk>(this Result<TOk, (T1, (T2, T3))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2), T3, T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2, T3), T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, (T2, T3), T4)> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, (T2, T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, (T1, T2, (T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());

    [GenerateAsyncExtension(ExtensionClassName = PlainAsyncExtensionName)]
    public static Result<TOk, (T1, T2, T3, T4)> CoalesceError<T1, T2, T3, T4, TOk>(this Result<TOk, ((T1, T2), (T3, T4))> result) =>
        result.MapError(tuple => tuple.Coalesce());
}
