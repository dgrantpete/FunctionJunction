using FunctionalEngine.Generator;
using static FunctionalEngine.Result<TOk, TError>;

namespace FunctionalEngine;

[DiscriminatedUnion(GeneratePolymorphicSerialization = false)]
public partial record Result<TOk, TError>
{
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

    public static implicit operator Result<TOk, TError>(TOk ok) => new Result<TOk, TError>.Ok(ok);

    public static implicit operator Result<TOk, TError>(TError error) => new Result<TOk, TError>.Error(error);

    public Result<TResult, TError> FlatMap<TResult>(Func<TOk, Result<TResult, TError>> mapper) =>
        Match(
            mapper,
            error => new Result<TResult, TError>.Error(error)
        );

    public Result<TResult, TError> Map<TResult>(Func<TOk, TResult> mapper) =>
        FlatMap(ok => 
            NewOk(mapper(ok))
        );

    public Result<TOk, TResult> Recover<TResult>(Func<TError, Result<TOk, TResult>> mapper) =>
        Match(
            ok => new Result<TOk, TResult>.Ok(ok),
            mapper
        );

    public Result<TOk, TResult> MapError<TResult>(Func<TError, TResult> mapper) =>
        Recover(error =>
            NewError(mapper(error))
        );

    public Result<TOk, TError> Validate(Func<TOk, bool> validator, Func<TOk, TError> errorMapper) =>
        FlatMap(ok => validator(ok) switch
        {
            true => this,
            false => NewError(errorMapper(ok))
        });

    public Result<(TOk Left, TOther Right), TError> And<TOther>(Func<Result<TOther, TError>> otherProvider) =>
        FlatMap(ok => otherProvider().Map(otherOk => (ok, otherOk)));

    public Result<TOk, (TError Left, TOther Right)> Or<TOther>(Func<Result<TOk, TOther>> otherProvider) =>
        Recover(error => otherProvider().MapError(otherError => (error, otherError)));

    public Result<TError, TOk> Swap() =>
        Match<Result<TError, TOk>>(
            value => new Result<TError, TOk>.Error(value),
            value => new Result<TError, TOk>.Ok(value)
        );

    private static Result<TResult, TError> NewOk<TResult>(TResult value) =>
        new Result<TResult, TError>.Ok(value);

    private static Result<TOk, TResult> NewError<TResult>(TResult value) =>
        new Result<TOk, TResult>.Error(value);
}

public static class Result
{
    public class WithOkType<TOk>
    {
        public static Result<TOk, TError> Error<TError>(TError error) =>
            new Result<TOk, TError>.Error(error);
    }

    public static class WithErrorType<TError>
    {
        public static Result<TOk, TError> Ok<TOk>(TOk ok) =>
            new Result<TOk, TError>.Ok(ok);
    }

    public static Result<TOk, TError> Ok<TOk, TError>(TOk ok) => new Result<TOk, TError>.Ok(ok);

    public static Result<TOk, TError> Error<TOk, TError>(TError error) => new Result<TOk, TError>.Error(error);
}
