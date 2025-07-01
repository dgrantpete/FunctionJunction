using FunctionJunction.Generator;
using static FunctionJunction.Option;
using static FunctionJunction.Prelude;

namespace FunctionJunction.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Option{T}"/> that enable working with tuple values more ergonomically.
/// These methods allow mapping, flat-mapping, and tuple coalescing operations to work directly with tuple elements instead of requiring manual deconstruction.
/// </summary>
[GenerateAsyncExtension(ExtensionClassName = "OptionAsyncExtensions", Namespace = "FunctionJunction.Async")]
public static class OptionExtensions
{
    /// <summary>
    /// Creates an <see cref="Option{T}"/> based on the provided <paramref name="condition"/>, calling <paramref name="valueProvider"/> and wrapping it in a <c>Some</c> value if it's <see langword="true"/> and <c>None</c> if it's <see langword="false"/>.
    /// </summary>
    /// <param name="condition">Condition used to determine if the returned value should be <c>Some</c> or <c>None</c>.</param>
    /// <param name="valueProvider">The function called when <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <returns>A <c>Some</c> value when <paramref name="condition"/> is <see langword="true"/>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<T> ToOption<T>(this bool condition, Func<T> valueProvider) where T : notnull =>
        condition switch
        {
            true => valueProvider(),
            false => default(Option<T>)
        };

    /// <summary>
    /// Creates an <see cref="Option{T}"/> based on the provided <paramref name="condition"/>, asyncronously calling <paramref name="valueProviderAsync"/> and wrapping it in a <c>Some</c> value if it's <see langword="true"/> and <c>None</c> if it's <see langword="false"/>.
    /// </summary>
    /// <param name="condition">Condition used to determine if the returned value should be <c>Some</c> or <c>None</c>.</param>
    /// <param name="valueProviderAsync">The asyncronous function called when <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <returns>A <see cref="ValueTask"/> containing <c>Some</c> value when <paramref name="condition"/> is <see langword="true"/>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static async ValueTask<Option<T>> ToOption<T>(this bool condition, Func<ValueTask<T>> valueProviderAsync) where T : notnull =>
        condition switch
        {
            true => await valueProviderAsync(),
            false => default(Option<T>)
        };

    /// <summary>
    /// Applies a function that returns an <see cref="Option{T}"/> to the tuple elements inside this <see cref="Option{T}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
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

    /// <summary>
    /// Applies a function that returns an <see cref="Option{T}"/> to the tuple elements inside this <see cref="Option{T}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns an <c>Option&lt;TResult&gt;</c>.</param>
    /// <returns>The <see cref="Option{T}"/> returned by the mapper if this <see cref="Option{T}"/> is <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TResult> FlatMapTuple<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    /// <summary>
    /// Applies a function that returns an <see cref="Option{T}"/> to the tuple elements inside this <see cref="Option{T}"/>, flattening the result.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to operate on.</param>
    /// <param name="mapper">A function that takes the tuple elements and returns an <c>Option&lt;TResult&gt;</c>.</param>
    /// <returns>The <see cref="Option{T}"/> returned by the mapper if this <see cref="Option{T}"/> is <c>Some</c>, otherwise <c>None</c>.</returns>
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

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Option{T}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>An <see cref="Option{T}"/> containing the transformed value if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TResult> MapTuple<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    /// <summary>
    /// Transforms the tuple elements inside this <see cref="Option{T}"/> using the provided function.
    /// This allows working with tuple components directly without manual deconstruction.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <param name="mapper">A function that transforms the tuple elements to a new value.</param>
    /// <returns>An <see cref="Option{T}"/> containing the transformed value if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TResult> MapTuple<T1, T2, T3, T4, TResult>(this Option<(T1, T2, T3, T4)> option, Func<T1, T2, T3, T4, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3)> Coalesce<T1, T2, T3>(this Option<((T1, T2), T3)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3)> Coalesce<T1, T2, T3>(this Option<(T1, (T2, T3))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<((T1, T2), T3, T4)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<((T1, T2, T3), T4)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<(T1, (T2, T3), T4)> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<(T1, (T2, T3, T4))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<(T1, T2, (T3, T4))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());

    /// <summary>
    /// Takes the nested tuple elements inside of this <see cref="Option{T}"/> and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> containing a tuple to transform.</param>
    /// <returns>An <see cref="Option{T}"/> containing the flattened tuple if <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<(T1, T2, T3, T4)> Coalesce<T1, T2, T3, T4>(this Option<((T1, T2), (T3, T4))> option)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    =>
        option.Map(tuple => tuple.Coalesce());
}
