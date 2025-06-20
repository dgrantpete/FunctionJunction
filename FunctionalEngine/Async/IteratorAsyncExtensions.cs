namespace FunctionalEngine.Async;

public static class IteratorAsyncExtensions
{
    public static async IAsyncEnumerable<Enumerated<T>> Enumerate<T>(this IAsyncEnumerable<T> source)
    {
        var index = 0;

        await foreach (var item in source)
        {
            yield return new(item, index);

            index++;
        }
    }

    public static async IAsyncEnumerable<TResult> Scan<TSource, TResult>(this IAsyncEnumerable<TSource> source, TResult seed, Func<TResult, TSource, Task<TResult>> scannerAsync)
    {
        var current = seed;

        yield return current;

        await foreach (var item in source)
        {
            current = await scannerAsync(current, item);

            yield return current;
        }
    }

    public static async IAsyncEnumerable<TResult> Scan<TSource, TResult>(this IAsyncEnumerable<TSource> source, TResult seed, Func<TResult, TSource, TResult> scanner)
    {
        var current = seed;

        yield return current;

        await foreach (var item in source)
        {
            current = scanner(current, item);

            yield return current;
        }
    }

    public static async IAsyncEnumerable<T> Scan<T>(this IAsyncEnumerable<T> source, Func<T, T, Task<T>> scannerAsync)
    {
        await using var asyncEnumerator = source.GetAsyncEnumerator();

        if (!await asyncEnumerator.MoveNextAsync())
        {
            yield break;
        }

        yield return asyncEnumerator.Current;

        var current = asyncEnumerator.Current;

        while (await asyncEnumerator.MoveNextAsync())
        {
            current = await scannerAsync(current, asyncEnumerator.Current);

            yield return current;
        }
    }

    public static async IAsyncEnumerable<T> Scan<T>(this IAsyncEnumerable<T> source, Func<T, T, T> scanner)
    {
        await using var asyncEnumerator = source.GetAsyncEnumerator();

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

    public static async IAsyncEnumerable<TResult> SelectWhere<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, Task<Option<TResult>>> conditionalSelectorAsync)
        where TResult : notnull
    {
        await foreach (var item in source)
        {
            var option = await conditionalSelectorAsync(item);

            if (option.TryUnwrap(out var value))
            {
                yield return value;
            }
        }
    }

    public static async IAsyncEnumerable<TResult> SelectWhere<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, Option<TResult>> conditionalSelector)
        where TResult : notnull
    {
        await foreach (var item in source)
        {
            var option = conditionalSelector(item);

            if (option.TryUnwrap(out var value))
            {
                yield return value;
            }
        }
    }

    public static async IAsyncEnumerable<T> TakeWhileInclusive<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicateAsync)
    {
        await foreach (var item in source)
        {
            yield return item;

            if (!await predicateAsync(item))
            {
                yield break;
            }
        }
    }

    public static async IAsyncEnumerable<T> TakeWhileInclusive<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            yield return item;

            if (!predicate(item))
            {
                yield break;
            }
        }
    }
}
