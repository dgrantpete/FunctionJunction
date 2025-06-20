using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Async;

public static partial class ResultAsyncExtensions
{
    public static Task<Result<TOk, TError>> Sequence<TOk, TError>(this Result<Task<TOk>, TError> result) =>
        result.Map(Identity);

    public static Task<Result<TOk, TError>> Sequence<TOk, TError>(this Result<TOk, Task<TError>> result) =>
        result.MapError(Identity);
}
