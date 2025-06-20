using FunctionalEngine.Extensions;
using System.Collections.Immutable;
using static FunctionalEngine.Option;
using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Async;

public static partial class OptionAsyncExtensions
{
    public static Task<Option<T>> Sequence<T>(this Option<Task<T>> option) where T : notnull =>
        option.Map(Identity);

    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<Option<T>> optionTask) where T : notnull
    {
        var option = await optionTask;

        if (option.TryUnwrap(out var value))
        {
            yield return value;
        }
    }

    public static async Task<Option<IImmutableList<T>>> All<T>(IAsyncEnumerable<Option<T>> options) where T : notnull =>
        await options
            .Scan(
                Some<IImmutableList<T>>([]),
                (previousOptions, option) => previousOptions.And(() => option)
                    .MapTuple((previousValues, value) => previousValues.Add(value))
            )
            .TakeWhileInclusive(option => option.IsSome)
            .LastAsync();

    public static Task<Option<IImmutableList<T>>> All<T>(params IEnumerable<Func<Task<Option<T>>>> optionProvidersAsync) where T : notnull =>
        All(
            optionProvidersAsync.ToAsyncEnumerable()
                .Select(async (Func<Task<Option<T>>> optionProvider, CancellationToken _) => await optionProvider())
        );
     
    public static async Task<Option<T>> Any<T>(IAsyncEnumerable<Option<T>> options) where T : notnull =>
        await options.FirstOrDefaultAsync(option => option.IsSome);

    public static Task<Option<T>> Any<T>(params IEnumerable<Func<Task<Option<T>>>> optionProvidersAsync) where T : notnull =>
        Any(
            optionProvidersAsync.ToAsyncEnumerable()
                .Select(async (Func<Task<Option<T>>> optionProvider, CancellationToken _) => await optionProvider())
        );
}
