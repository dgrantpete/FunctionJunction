using System.Collections.Immutable;
using static FunctionalEngine.Result;
using static FunctionalEngine.Prelude;
using FunctionalEngine.Extensions;

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

    public static async Task<Result<IImmutableList<TOk>, TError>> All<TOk, TError>(IAsyncEnumerable<Result<TOk, TError>> results) =>
        await results
            .Scan(
                Ok<IImmutableList<TOk>, TError>([]),
                (previousResults, result) =>
                    previousResults.And(() => result)
                        .MapTuple((previousOks, ok) => previousOks.Add(ok))
            )
            .TakeWhileInclusive(result => result.IsOk)
            .LastAsync();

    public static Task<Result<IImmutableList<TOk>, TError>> All<TOk, TError>(params IEnumerable<Func<Task<Result<TOk, TError>>>> resultProvidersAsync) =>
        All(
            resultProvidersAsync.ToAsyncEnumerable()
                .Select(async (Func<Task<Result<TOk, TError>>> resultProvider, CancellationToken _) => await resultProvider())
        );

    public static async Task<Result<TOk, IImmutableList<TError>>> Any<TOk, TError>(IAsyncEnumerable<Result<TOk, TError>> results) =>
        await results
            .Scan(
                Error<TOk, IImmutableList<TError>>([]),
                (previousResults, result) =>
                    previousResults.Or(() => result)
                        .MapErrorTuple((previousErrors, error) => previousErrors.Add(error))
            )
            .TakeWhileInclusive(result => result.IsError)
            .LastAsync();

    public static Task<Result<TOk, IImmutableList<TError>>> Any<TOk, TError>(params IEnumerable<Func<Task<Result<TOk, TError>>>> resultProvidersAsync) =>
        Any(
            resultProvidersAsync.ToAsyncEnumerable()
                .Select(async (Func<Task<Result<TOk, TError>>> resultProvider, CancellationToken _) => await resultProvider())
        );
}
