namespace FunctionJunction.Async;

/// <summary>
/// Provides asynchronous extension methods for <see cref="IAsyncEnumerable{T}"/> that add functional programming capabilities.
/// These methods are the async equivalents of the synchronous iterator extensions, designed for working with asynchronous data streams.
/// </summary>
public static class IteratorAsyncExtensions
{
    /// <summary>
    /// Asynchronously transforms a sequence into an <see cref="IAsyncEnumerable{T}"/> of <see cref="Enumerated{T}"/> values, pairing each element with its zero-based index.
    /// This is the async equivalent of the synchronous <c>Enumerate</c> method.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The async sequence to enumerate with indices.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="Enumerated{T}"/> containing each value paired with its index.</returns>
    public static async IAsyncEnumerable<Enumerated<T>> Enumerate<T>(this IAsyncEnumerable<T> source)
    {
        var index = 0;

        await foreach (var item in source.ConfigureAwait(false))
        {
            yield return new(index, item);

            index++;
        }
    }

    /// <summary>
    /// Asynchronously applies an accumulator function over a sequence, returning each intermediate result.
    /// The scanner function is asynchronous, allowing for async computations during accumulation.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the accumulator and result values.</typeparam>
    /// <param name="source">The async sequence to scan over.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="scannerAsync">The async accumulator function to apply to each element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing the seed and all intermediate accumulation results.</returns>
    public static async IAsyncEnumerable<TResult> Scan<TSource, TResult>(this IAsyncEnumerable<TSource> source, TResult seed, Func<TResult, TSource, ValueTask<TResult>> scannerAsync)
    {
        var current = seed;

        yield return current;

        await foreach (var item in source.ConfigureAwait(false))
        {
            current = await scannerAsync(current, item).ConfigureAwait(false);

            yield return current;
        }
    }

    /// <summary>
    /// Asynchronously applies a synchronous accumulator function over a sequence, returning each intermediate result.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the accumulator and result values.</typeparam>
    /// <param name="source">The async sequence to scan over.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="scanner">The synchronous accumulator function to apply to each element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing the seed and all intermediate accumulation results.</returns>
    public static async IAsyncEnumerable<TResult> Scan<TSource, TResult>(this IAsyncEnumerable<TSource> source, TResult seed, Func<TResult, TSource, TResult> scanner)
    {
        var current = seed;

        yield return current;

        await foreach (var item in source.ConfigureAwait(false))
        {
            current = scanner(current, item);

            yield return current;
        }
    }

    /// <summary>
    /// Asynchronously applies an async accumulator function over a sequence, using the first element as the seed and returning each intermediate result.
    /// If the sequence is empty, returns an empty <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The async sequence to scan over.</param>
    /// <param name="scannerAsync">The async accumulator function to apply to each element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing the first element and all intermediate accumulation results.</returns>
    public static async IAsyncEnumerable<T> Scan<T>(this IAsyncEnumerable<T> source, Func<T, T, ValueTask<T>> scannerAsync)
    {
        await using var asyncEnumerator = source.ConfigureAwait(false).GetAsyncEnumerator();

        if (!await asyncEnumerator.MoveNextAsync())
        {
            yield break;
        }

        yield return asyncEnumerator.Current;

        var current = asyncEnumerator.Current;

        while (await asyncEnumerator.MoveNextAsync())
        {
            current = await scannerAsync(current, asyncEnumerator.Current).ConfigureAwait(false);

            yield return current;
        }
    }

    /// <summary>
    /// Asynchronously applies a synchronous accumulator function over a sequence, using the first element as the seed and returning each intermediate result.
    /// If the sequence is empty, returns an empty <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The async sequence to scan over.</param>
    /// <param name="scanner">The synchronous accumulator function to apply to each element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing the first element and all intermediate accumulation results.</returns>
    public static async IAsyncEnumerable<T> Scan<T>(this IAsyncEnumerable<T> source, Func<T, T, T> scanner)
    {
        await using var asyncEnumerator = source.ConfigureAwait(false).GetAsyncEnumerator();

        if (!await asyncEnumerator.MoveNextAsync())
        {
            yield break;
        }

        yield return asyncEnumerator.Current;

        var current = asyncEnumerator.Current;

        while (await asyncEnumerator.MoveNextAsync())
        {
            current = scanner(current, asyncEnumerator.Current);

            yield return current;
        }
    }

    /// <summary>
    /// Asynchronously transforms and filters a sequence using an async <see cref="Option{T}"/>-returning selector.
    /// Elements that produce <c>Some</c> values are included with their transformed values, while <c>None</c> results are filtered out.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the transformed elements. Must be non-null.</typeparam>
    /// <param name="source">The async sequence to transform and filter.</param>
    /// <param name="conditionalSelectorAsync">An async function that transforms elements and determines inclusion via <see cref="Option{T}"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing only the transformed values from <c>Some</c> results.</returns>
    public static async IAsyncEnumerable<TResult> SelectWhere<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<Option<TResult>>> conditionalSelectorAsync)
        where TResult : notnull
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            var option = await conditionalSelectorAsync(item).ConfigureAwait(false);

            if (option.TryUnwrap(out var value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Asynchronously transforms and filters a sequence using a synchronous <see cref="Option{T}"/>-returning selector.
    /// Elements that produce <c>Some</c> values are included with their transformed values, while <c>None</c> results are filtered out.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the transformed elements. Must be non-null.</typeparam>
    /// <param name="source">The async sequence to transform and filter.</param>
    /// <param name="conditionalSelector">A synchronous function that transforms elements and determines inclusion via <see cref="Option{T}"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing only the transformed values from <c>Some</c> results.</returns>
    public static async IAsyncEnumerable<TResult> SelectWhere<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, Option<TResult>> conditionalSelector)
        where TResult : notnull
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            var option = conditionalSelector(item);

            if (option.TryUnwrap(out var value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Asynchronously returns elements from the start of a sequence as long as an async condition is true, including the first element that fails the condition.
    /// This provides "take until" semantics with async predicate evaluation.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The async sequence to take elements from.</param>
    /// <param name="predicateAsync">The async condition to test each element against.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing elements while the predicate is true, plus the first failing element.</returns>
    public static async IAsyncEnumerable<T> TakeWhileInclusive<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask<bool>> predicateAsync)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            yield return item;

            if (!await predicateAsync(item).ConfigureAwait(false))
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Asynchronously returns elements from the start of a sequence as long as a synchronous condition is true, including the first element that fails the condition.
    /// This provides "take until" semantics with synchronous predicate evaluation.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The async sequence to take elements from.</param>
    /// <param name="predicate">The synchronous condition to test each element against.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing elements while the predicate is true, plus the first failing element.</returns>
    public static async IAsyncEnumerable<T> TakeWhileInclusive<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            yield return item;

            if (!predicate(item))
            {
                yield break;
            }
        }
    }
}
