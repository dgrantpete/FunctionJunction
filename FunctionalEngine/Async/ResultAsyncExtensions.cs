using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Async;

public static partial class ResultAsyncExtensions
{
    public static Task<Result<TOk, TError>> Sequence<TOk, TError>(this Result<Task<TOk>, TError> result) =>
        result.Map(Identity);

    public static Task<Result<TOk, TError>> Sequence<TOk, TError>(this Result<TOk, Task<TError>> result) =>
        result.MapError(Identity);

    public static async IAsyncEnumerable<TOk> ToAsyncEnumerable<TOk, TError>(this Task<Result<TOk, TError>> resultTask)
    {
        var result = await resultTask;

        if (result.TryUnwrap(out var ok))
        {
            yield return ok;
        }
    }

    public static async IAsyncEnumerable<TError> ToErrorAsyncEnumerable<TOk, TError>(this Task<Result<TOk, TError>> resultTask)
    {
        var result = await resultTask;

        if (result.TryUnwrapError(out var error))
        {
            yield return error;
        }
    }
}
