using FunctionalEngine.Generator;
using static FunctionalEngine.Option;
using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Extensions;

#pragma warning disable CS1591

/// <summary>
/// Provides extension methods for <see cref="Option{T}"/> that enable working with tuple values more ergonomically.
/// These methods allow mapping, flat-mapping, and tuple coalescing operations to work directly with tuple elements instead of requiring manual deconstruction.
/// </summary>
[GenerateAsyncExtension(ExtensionClassName = "OptionAsyncExtensions", Namespace = "FunctionalEngine.Async")]
public static class OptionExtensions
{
    /// <summary>
    /// Applies a function that returns an <see cref="Option{T}"/> to the tuple elements inside this <see cref="Option{T}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element. Must be non-null.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element. Must be non-null.</typeparam>
    /// <typeparam name="TResult">The type of the value in the <see cref="Option{T}"/> returned by the mapper. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns an <c>Option&lt;TResult&gt;</c>.</param>
    /// <returns>The <see cref="Option{T}"/> returned by the mapper if this <see cref="Option{T}"/> is <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TResult> FlatMapTuple<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2));

    [GenerateAsyncExtension]
    public static Option<TResult> FlatMapTuple<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    [GenerateAsyncExtension]
    public static Option<TResult> FlatMapTuple<T1, T2, T3, T4, TResult>(this Option<(T1, T2, T3, T4)> option, Func<T1, T2, T3, T4, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Option{T}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element. Must be non-null.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element. Must be non-null.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>An <see cref="Option{T}"/> containing the transformed value if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TResult> MapTuple<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    [GenerateAsyncExtension]
    public static Option<TResult> MapTuple<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    [GenerateAsyncExtension]
    public static Option<TResult> MapTuple<T1, T2, T3, T4, TResult>(this Option<(T1, T2, T3, T4)> option, Func<T1, T2, T3, T4, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3)> Coalesce<T1, T2, T3>(this Option<((T1, T2), T3)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3)> Coalesce<T1, T2, T3>(this Option<(T1, (T2, T3))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<((T1, T2), T3, T4)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<((T1, T2, T3), T4)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<(T1, (T2, T3), T4)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<(T1, (T2, T3, T4))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<(T1, T2, (T3, T4))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<((T1, T2), (T3, T4))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());
}
