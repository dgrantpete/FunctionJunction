using FunctionalEngine.Generator;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using static FunctionalEngine.Functions;

namespace FunctionalEngine;

[DiscriminatedUnion]
public abstract partial record Result<TOk, TError>
{
    public bool IsOk => this is Ok;

    public bool IsError => !IsOk;

    public record Ok : Result<TOk, TError>
    {
        public TOk Value { get; internal init; }

        internal Ok(TOk value)
        {
            Value = value;
        }

        public void Deconstruct(out TOk value)
        {
            value = Value;
        }
    }

    public record Error : Result<TOk, TError>
    {
        public TError Value { get; internal init; }

        internal Error(TError value)
        {
            Value = value;
        }

        public void Deconstruct(out TError value)
        {
            value = Value;
        }
    }

    public static implicit operator Result<TOk, TError>(TOk ok) => 
        new Result<TOk, TError>.Ok(ok);

    public static implicit operator Result<TOk, TError>(TError error) => 
        new Result<TOk, TError>.Error(error);

    [GenerateAsyncExtension]
    public Result<TResult, TError> FlatMap<TResult>(Func<TOk, Result<TResult, TError>> mapper) =>
        Match(
            mapper,
            error => new Result<TResult, TError>.Error(error)
        );

    [GenerateAsyncExtension]
    public Task<Result<TResult, TError>> FlatMapAsync<TResult>(Func<TOk, Task<Result<TResult, TError>>> mapperAsync) =>
        Match(
            mapperAsync,
            error => Task.FromResult<Result<TResult, TError>>(new Result<TResult, TError>.Error(error))
        );

    [GenerateAsyncExtension]
    public Result<TResult, TError> Map<TResult>(Func<TOk, TResult> mapper) =>
        FlatMap(ok => 
            NewOk(mapper(ok))
        );

    [GenerateAsyncExtension]
    public Task<Result<TResult, TError>> MapAsync<TResult>(Func<TOk, Task<TResult>> mapperAsync) =>
        FlatMapAsync(async ok => NewOk(await mapperAsync(ok)));

    [GenerateAsyncExtension]
    public Result<TOk, TResult> Recover<TResult>(Func<TError, Result<TOk, TResult>> recoverer) =>
        Match(
            ok => new Result<TOk, TResult>.Ok(ok),
            recoverer
        );

    [GenerateAsyncExtension]
    public Task<Result<TOk, TResult>> RecoverAsync<TResult>(Func<TError, Task<Result<TOk, TResult>>> recovererAsync) =>
        Match(
            ok => Task.FromResult<Result<TOk, TResult>>(new Result<TOk, TResult>.Ok(ok)),
            recovererAsync
        );

    [GenerateAsyncExtension]
    public Result<TOk, TResult> MapError<TResult>(Func<TError, TResult> mapper) =>
        Recover(error =>
            NewError(mapper(error))
        );

    [GenerateAsyncExtension]
    public Task<Result<TOk, TResult>> MapErrorAsync<TResult>(Func<TError, Task<TResult>> mapperAsync) =>
        RecoverAsync(async error =>
            NewError(await mapperAsync(error))
        );

    [GenerateAsyncExtension]
    public Result<TOk, TError> Validate(Func<TOk, bool> validator, Func<TOk, TError> errorMapper) =>
        FlatMap(ok => validator(ok) switch
        {
            true => this,
            false => NewError(errorMapper(ok))
        });

    [GenerateAsyncExtension]
    public Task<Result<TOk, TError>> ValidateAsync(Func<TOk, Task<bool>> validatorAsync, Func<TOk, Task<TError>> errorMapperAsync) =>
        FlatMapAsync(async ok => await validatorAsync(ok) switch
        {
            true => this,
            false => NewError(await errorMapperAsync(ok))
        });

    [GenerateAsyncExtension]
    public Result<(TOk Left, TOther Right), TError> And<TOther>(Func<Result<TOther, TError>> otherProvider) =>
        FlatMap(ok => otherProvider().Map(otherOk => (ok, otherOk)));

    [GenerateAsyncExtension]
    public Task<Result<(TOk Left, TOther Right), TError>> AndAsync<TOther>(Func<Task<Result<TOther, TError>>> otherProviderAsync) =>
        FlatMapAsync(async ok => (await otherProviderAsync()).Map(otherOk => (ok, otherOk)));

    [GenerateAsyncExtension]
    public Result<TOk, (TError Left, TOther Right)> Or<TOther>(Func<Result<TOk, TOther>> otherProvider) =>
        Recover(error => otherProvider().MapError(otherError => (error, otherError)));

    [GenerateAsyncExtension]
    public Task<Result<TOk, (TError Left, TOther Right)>> OrAsync<TOther>(Func<Task<Result<TOk, TOther>>> otherProviderAsync) =>
        RecoverAsync(async error => (await otherProviderAsync()).MapError(otherError => (error, otherError)));

    [GenerateAsyncExtension]
    public Result<TError, TOk> Swap() =>
        Match<Result<TError, TOk>>(
            value => new Result<TError, TOk>.Error(value),
            value => new Result<TError, TOk>.Ok(value)
        );

    [GenerateAsyncExtension]
    public Result<TOk, TError> Tap(Action<TOk> tapper) =>
        Map(ok =>
        {
            tapper(ok);
            return ok;
        });

    [GenerateAsyncExtension]
    public Task<Result<TOk, TError>> TapAsync(Func<TOk, Task> tapperAsync) =>
        MapAsync(async ok =>
        {
            await tapperAsync(ok);
            return ok;
        });

    [GenerateAsyncExtension]
    public Result<TOk, TError> TapError(Action<TError> tapper) =>
        MapError(error =>
        {
            tapper(error);
            return error;
        });

    [GenerateAsyncExtension]
    public Task<Result<TOk, TError>> TapErrorAsync(Func<TError, Task> tapperAsync) =>
        MapErrorAsync(async error =>
        {
            await tapperAsync(error);
            return error;
        });

    [GenerateAsyncExtension]
    public TOk UnwrapOr(Func<TError, TOk> defaultProvider) =>
        Match(
            Identity,
            defaultProvider
        );

    [GenerateAsyncExtension]
    public Task<TOk> UnwrapOrAsync(Func<TError, Task<TOk>> defaultProviderAsync) =>
        Match(
            Task.FromResult,
            defaultProviderAsync
        );

    [GenerateAsyncExtension]
    public TOk UnwrapOrThrow<TException>(Func<TError, TException> exceptionProvider) where TException : Exception =>
        UnwrapOr(error => throw exceptionProvider(error));

    [GenerateAsyncExtension]
    public Task<TOk> UnwrapOrThrowAsync<TException>(Func<TError, Task<TException>> exceptionProviderAsync) where TException : Exception =>
        UnwrapOrAsync(async error => throw await exceptionProviderAsync(error));

    public IEnumerable<TOk> ToEnumerable()
    {
        if (this is Ok(var ok))
        {
            yield return ok;
        }
    }

    public IEnumerable<TError> ToErrorEnumerable()
    {
        if (this is Error(var error))
        {
            yield return error;
        }
    }

    private static Result<TResult, TError> NewOk<TResult>(TResult value) =>
        new Result<TResult, TError>.Ok(value);

    private static Result<TOk, TResult> NewError<TResult>(TResult value) =>
        new Result<TOk, TResult>.Error(value);
}

public static class Result
{
    public static class ApplyType<TPartial>
    {
        public static Result<TPartial, TError> Error<TError>(TError error) =>
            new Result<TPartial, TError>.Error(error);

        public static Result<TOk, TPartial> Ok<TOk>(TOk ok) =>
            new Result<TOk, TPartial>.Ok(ok);
    }

    public static Result<TOk, TError> Ok<TOk, TError>(TOk ok) => new Result<TOk, TError>.Ok(ok);

    public static async Task<Result<TOk, TError>> OkAsync<TOk, TError>(Task<TOk> okTask) => new Result<TOk, TError>.Ok(await okTask);

    public static Result<TOk, TError> Error<TOk, TError>(TError error) => new Result<TOk, TError>.Error(error);

    public static async Task<Result<TOk, TError>> ErrorAsync<TOk, TError>(Task<TError> errorTask) => new Result<TOk, TError>.Error(await errorTask);

    [GenerateAsyncExtension]
    public static Option<TOk> ToOption<TOk, TError>(this Result<TOk, TError> result) where TOk : notnull =>
        result.Match(
            Option.Some,
            _ => default
        );

    [GenerateAsyncExtension]
    public static Option<TError> ToErrorOption<TOk, TError>(this Result<TOk, TError> result) where TError : notnull =>
        result.Match(
            _ => default,
            Option.Some
        );

    [GenerateAsyncExtension]
    public static Result<TOk, TError> Validate<TOk, TError>(this Result<TOk, TError> result, Func<TOk, Option<TError>> validator)
        where TError : notnull
    =>
        result.FlatMap(ok =>
            validator(ok).ToErrorResult(() => ok)
        );

    [GenerateAsyncExtension]
    public static Result<TOk, TError> Recover<TOk, TError>(this Result<TOk, TError> result, Func<TError, Option<TOk>> recoverer)
        where TOk : notnull
    =>
        result.Recover(error =>
            recoverer(error).ToResult(() => error)
        );

    [GenerateAsyncExtension]
    public static TOk? UnwrapNullable<TOk, TError>(this Result<TOk, TError> result) where TOk : class =>
        result.Match<TOk?>(
            value => value,
            _ => null
        );

    [GenerateAsyncExtension]
    public static TOk? UnwrapNullableValue<TOk, TError>(this Result<TOk, TError> result) where TOk : struct =>
        result.Match<TOk?>(
            value => value,
            _ => null
        );

    [GenerateAsyncExtension]
    public static TOk UnwrapOrThrow<TOk, TError>(this Result<TOk, TError> result) =>
        result.UnwrapOrThrow(error =>
            new InvalidOperationException($"Could not unwrap 'Result<{typeof(TOk).Name}, {typeof(TError).Name}>' because it contains an error: {error}")
        );

    [GenerateAsyncExtension]
    public static TOk UnwrapOrThrowException<TOk, TException>(this Result<TOk, TException> result) where TException : Exception =>
        result.UnwrapOrThrow(Identity);

    public static bool TryUnwrap<TOk, TError>(this Result<TOk, TError> result, [NotNullWhen(true)] out TOk? ok)
    {
        var isOk = false;
        var maybeOk = default(TOk?);

        result.Tap(ok =>
        {
            isOk = true;
            maybeOk = ok;
        });

        ok = maybeOk;

        return isOk;
    }

    public static bool TryUnwrapError<TOk, TError>(this Result<TOk, TError> result, [NotNullWhen(true)] out TError? error)
    {
        var isError = false;
        var maybeError = default(TError?);

        result.TapError(error =>
        {
            isError = true;
            maybeError = error;
        });

        error = maybeError;

        return isError;
    }

    public static Result<IImmutableList<TOk>, TError> All<TOk, TError>(params IEnumerable<Func<Result<TOk, TError>>> resultProviders) =>
        resultProviders
            .Aggregate(
                Ok<IImmutableList<TOk>, TError>([]),
                (previous, current) =>
                    previous.And(current)
                        .MapTuple((errors, newError) => errors.Add(newError))
            );

    public static Result<TOk, IImmutableList<TError>> Any<TOk, TError>(params IEnumerable<Func<Result<TOk, TError>>> resultProviders) =>
        resultProviders
            .Aggregate(
                Error<TOk, IImmutableList<TError>>([]),
                (previous, current) =>
                    previous.Or(current)
                        .MapErrorTuple((errors, newError) => errors.Add(newError))
            );
}
