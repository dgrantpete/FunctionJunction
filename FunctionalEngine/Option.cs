using FunctionalEngine.Generator;
using static FunctionalEngine.Functions;

namespace FunctionalEngine;

public readonly record struct Option<T> where T : notnull
{
    private readonly T internalValue;

    public bool IsSome { get; }

    public bool IsNone => !IsSome;

    internal Option(T value)
    {
        IsSome = true;
        internalValue = value;
    }

    public static implicit operator Option<T>(T? value) => value switch
    {
        not null => new Option<T>(value),
        null => default
    };

    public TResult Match<TResult>(
        Func<T, TResult> onSome,
        Func<TResult> onNone
    ) => IsSome switch
    {
        true => onSome(internalValue),
        false => onNone()
    };

    [GenerateAsyncExtension]
    public Option<TResult> FlatMap<TResult>(Func<T, Option<TResult>> mapper) where TResult : notnull =>
        Match(
            mapper,
            () => default
        );

    [GenerateAsyncExtension]
    public Task<Option<TResult>> FlatMapAsync<TResult>(Func<T, Task<Option<TResult>>> mapperAsync) where TResult : notnull =>
        Match(
            mapperAsync,
            () => Task.FromResult(default(Option<TResult>))
        );

    [GenerateAsyncExtension]
    public Option<TResult> Map<TResult>(Func<T, TResult> mapper) where TResult : notnull =>
        FlatMap(value => new Option<TResult>(mapper(value)));

    [GenerateAsyncExtension]
    public Task<Option<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapperAsync) where TResult : notnull =>
        FlatMapAsync(async value => new Option<TResult>(await mapperAsync(value)));

    [GenerateAsyncExtension]
    public Option<T> Filter(Func<T, bool> filter) =>
        FlatMap(value => filter(value) switch
        {
            true => new Option<T>(value),
            false => default
        });

    [GenerateAsyncExtension]
    public Task<Option<T>> FilterAsync(Func<T, Task<bool>> filterAsync) =>
        FlatMapAsync(async value => await filterAsync(value) switch
        {
            true => new Option<T>(value),
            false => default
        });

    [GenerateAsyncExtension]
    public Option<T> Or(Func<Option<T>> alternativeProvider) =>
        Match(
            value => new(value),
            alternativeProvider
        );

    [GenerateAsyncExtension]
    public Task<Option<T>> OrAsync(Func<Task<Option<T>>> alternateProviderAsync) =>
        Match(
            value => Task.FromResult(new Option<T>(value)),
            alternateProviderAsync
        );

    [GenerateAsyncExtension]
    public Option<(T Left, TOther Right)> And<TOther>(Func<Option<TOther>> optionProvider) where TOther : notnull =>
        FlatMap(value => optionProvider().Map(otherValue => (value, otherValue)));

    [GenerateAsyncExtension]
    public Task<Option<(T Left, TOther Right)>> AndAsync<TOther>(Func<Task<Option<TOther>>> optionProvider) where TOther : notnull =>
        FlatMapAsync(async value => 
            (await optionProvider())
                .Map(otherValue => (value, otherValue))
        );

    [GenerateAsyncExtension]
    public Option<T> Tap(Action<T> tapper) =>
        Map(value =>
        {
            tapper(value);
            return value;
        });

    [GenerateAsyncExtension]
    public Task<Option<T>> TapAsync(Func<T, Task> tapperAsync) =>
        MapAsync(async value =>
        {
            await tapperAsync(value);
            return value;
        });

    [GenerateAsyncExtension]
    public Option<T> TapNone(Action tapper) =>
        Or(() =>
        {
            tapper();
            return default;
        });

    [GenerateAsyncExtension]
    public Task<Option<T>> TapNoneAsync(Func<Task> tapperAsync) =>
        OrAsync(async () =>
        {
            await tapperAsync();
            return default;
        });

    [GenerateAsyncExtension]
    public T UnwrapOr(Func<T> defaultValueProvider) =>
        Match(
            Identity,
            defaultValueProvider
        );

    [GenerateAsyncExtension]
    public Task<T> UnwrapOrAsync(Func<Task<T>> defaultValueProviderAsync) =>
        Match(
            Task.FromResult,
            defaultValueProviderAsync
        );

    [GenerateAsyncExtension]
    public T UnwrapOrThrow<TException>(Func<TException> exceptionProvider) where TException : Exception =>
        UnwrapOr(() => throw exceptionProvider());

    [GenerateAsyncExtension]
    public Task<T> UnwrapOrThrowAsync<TException>(Func<Task<TException>> exceptionProvider) where TException : Exception =>
        UnwrapOrAsync(async () => throw await exceptionProvider());

    [GenerateAsyncExtension]
    public T UnwrapOrThrow() => UnwrapOrThrow(() => 
        new InvalidOperationException($"Could not unwrap Option<{typeof(T).Name}> because it doesn't contain a value")
    );

    public IEnumerable<T> ToEnumerable() =>
        Match<IEnumerable<T>>(
            value => [value],
            () => []
        );
}

public static class Option
{
    public static Option<T> None<T>() where T : notnull => default;

    public static Option<T> Some<T>(T value) where T : notnull => new(value);

    public static T? UnwrapNullable<T>(this Option<T> option) where T : class =>
        option.Match<T?>(
            value => value,
            () => null
        );

    public static T? UnwrapNullableValue<T>(this Option<T> option) where T : struct =>
        option.Match<T?>(
            value => value,
            () => null
        );

    public static Option<T> FromNullable<T>(T? value) where T : class => value switch
    {
        not null => new(value),
        null => default
    };

    public static Option<T> FromNullable<T>(T? value) where T : struct => value switch
    {
        { } notNullValue => new(notNullValue),
        null => default
    };

    public static Option<T> Flatten<T>(this Option<Option<T>> option) where T : notnull =>
        option.FlatMap(Identity);

    public static Option<TResult> FlatMapTuple<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2));

    public static Option<TResult> FlatMapTuple<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3));

    public static Option<TResult> FlatMapTuple<T1, T2, T3, T4, TResult>(this Option<(T1, T2, T3, T4)> option, Func<T1, T2, T3, T4, Option<TResult>> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where TResult : notnull
    =>
        option.FlatMap(tuple => mapper(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

    public static Option<TResult> MapTuple<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    public static Option<TResult> MapTuple<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));

    public static Option<TResult> MapTuple<T1, T2, T3, T4, TResult>(this Option<(T1, T2, T3, T4)> option, Func<T1, T2, T3, T4, TResult> mapper)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where TResult : notnull
    =>
        option.FlatMapTuple(Compose(mapper, Some));
}