using FunctionalEngine.Generator;
using System.Diagnostics.CodeAnalysis;
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
    ) => 
        IsSome switch
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
    public Option<T> Or(Func<Option<T>> otherProvider) =>
        Match(
            value => new(value),
            otherProvider
        );

    [GenerateAsyncExtension]
    public Task<Option<T>> OrAsync(Func<Task<Option<T>>> otherProviderAsync) =>
        Match(
            value => Task.FromResult(new Option<T>(value)),
            otherProviderAsync
        );

    [GenerateAsyncExtension]
    public Option<(T Left, TOther Right)> And<TOther>(Func<Option<TOther>> otherProvider) where TOther : notnull =>
        FlatMap(value => otherProvider().Map(otherValue => (value, otherValue)));

    [GenerateAsyncExtension]
    public Task<Option<(T Left, TOther Right)>> AndAsync<TOther>(Func<Task<Option<TOther>>> otherProviderAsync) where TOther : notnull =>
        FlatMapAsync(async value => 
            (await otherProviderAsync())
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
    public T UnwrapOr(Func<T> defaultProvider) =>
        Match(
            Identity,
            defaultProvider
        );

    [GenerateAsyncExtension]
    public Task<T> UnwrapOrAsync(Func<Task<T>> defaultProviderAsync) =>
        Match(
            Task.FromResult,
            defaultProviderAsync
        );

    [GenerateAsyncExtension]
    public T UnwrapOrThrow<TException>(Func<TException> exceptionProvider) where TException : Exception =>
        UnwrapOr(() => throw exceptionProvider());

    [GenerateAsyncExtension]
    public Task<T> UnwrapOrThrowAsync<TException>(Func<Task<TException>> exceptionProvider) where TException : Exception =>
        UnwrapOrAsync(async () => throw await exceptionProvider());

    [GenerateAsyncExtension]
    public T UnwrapOrThrow() => 
        UnwrapOrThrow(() => 
            new InvalidOperationException($"Could not unwrap 'Option<{typeof(T).Name}>' because it doesn't contain a maybeValue")
        );

    public IEnumerable<T> ToEnumerable()
    {
        if (IsSome)
        {
            yield return internalValue;
        }
    }
}

public static class Option
{
    public static Option<T> None<T>() where T : notnull => default;

    public static Option<T> Some<T>(T value) where T : notnull => new(value);

    public static async Task<Option<T>> SomeAsync<T>(Task<T> valueTask) where T : notnull => 
        Some(await valueTask);

    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, Func<TError> errorProvider)
        where T : notnull
    =>
        option.Match(
            Result.ApplyType<TError>.Ok,
            Compose(errorProvider, Result.ApplyType<T>.Error)
        );

    public static Result<TOk, T> ToErrorResult<TOk, T>(this Option<T> option, Func<TOk> okProvider)
        where T : notnull
    =>
        option.Match(
            Result.ApplyType<TOk>.Error,
            Compose(okProvider, Result.ApplyType<T>.Ok)
        );

    [GenerateAsyncExtension]
    public static T? UnwrapNullable<T>(this Option<T> option) where T : class =>
        option.Match<T?>(
            value => value,
            () => null
        );

    [GenerateAsyncExtension]
    public static T? UnwrapNullableValue<T>(this Option<T> option) where T : struct =>
        option.Match<T?>(
            value => value,
            () => null
        );

    public static bool TryUnwrap<T>(this Option<T> option, [NotNullWhen(true)] out T? value) where T : notnull
    {
        var isSome = false;
        var maybeValue = default(T?);

        option.Tap(value =>
        {
            isSome = true;
            maybeValue = value;
        });

        value = maybeValue;

        return isSome;
    }

    public static Option<T> FromNullable<T>(T? value) where T : class => value switch
    {
        not null => new(value),
        null => default
    };

    public static async Task<Option<T>> FromNullableAsync<T>(Task<T?> valueTask) where T : class => await valueTask switch
    {
        { } value => new(value),
        null => default
    };

    public static async Task<Option<T>> FromNullableAsync<T>(Task<T?> valueTask) where T : struct => await valueTask switch
    {
        { } value => new(value),
        null => default
    };

    [GenerateAsyncExtension]
    public static Option<T> Flatten<T>(this Option<Option<T>> option) where T : notnull =>
        option.FlatMap(Identity);
}