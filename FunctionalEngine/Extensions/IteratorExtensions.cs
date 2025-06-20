namespace FunctionalEngine.Extensions;

public static class IteratorExtensions
{
    public static IEnumerable<Enumerated<T>> Enumerate<T>(this IEnumerable<T> source) =>
        source.Select((value, index) => new Enumerated<T>(value, index));

    public static IEnumerable<TResult> Scan<TSource, TResult>(this IEnumerable<TSource> source, TResult seed, Func<TResult, TSource, TResult> scanner)
    {
        var current = seed;

        yield return current;

        foreach (var item in source)
        {
            current = scanner(current, item);

            yield return current;
        }
    }

    public static IEnumerable<T> Scan<T>(this IEnumerable<T> source, Func<T, T, T> scanner)
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        yield return enumerator.Current;

        var current = enumerator.Current;

        while (enumerator.MoveNext())
        {
            current = scanner(current, enumerator.Current);

            yield return current;
        }
    }

    public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Option<TResult>> conditionalSelector) 
        where TResult : notnull 
    =>
        source.SelectMany(item =>
            conditionalSelector(item).ToEnumerable()
        );

    public static IEnumerable<T> TakeWhileInclusive<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            yield return item;

            if (!predicate(item))
            {
                yield break;
            }
        }
    }
}
