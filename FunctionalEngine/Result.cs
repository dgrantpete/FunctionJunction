using FunctionalEngine.Generator;

namespace FunctionalEngine;

[DiscriminatedUnion]
public partial record Result<TOk, TError>
{
    public record Ok(TOk Value) : Result<TOk, TError>;

    public record Error(TError Value) : Result<TOk, TError>;

    public Result<TResult, TError> FlatMap<TResult>(Func<TOk, Result<TResult, TError>> mapper) =>
        Match(
            ok => mapper(ok),
            error => new Result<TResult, TError>.Error(error)
        );
}
