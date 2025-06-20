using FunctionalEngine.Generator;
using static FunctionalEngine.Option;
using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Extensions;

[GenerateAsyncExtension(ExtensionClassName = "OptionAsyncExtensions", Namespace = "FunctionalEngine.Async")]
public static class OptionExtensions
{
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
