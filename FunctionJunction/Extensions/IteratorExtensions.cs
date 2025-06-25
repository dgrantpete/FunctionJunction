namespace FunctionJunction.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/> that add functional programming capabilities such as scanning, enumeration with indices, and conditional selection.
/// These methods complement the core Iterator functionality and provide useful operations for working with sequences.
/// </summary>
public static class IteratorExtensions
{
    /// <summary>
    /// Transforms a sequence into an enumerable of <see cref="Enumerated{T}"/> values, pairing each element with its zero-based index.
    /// This is similar to LINQ's Select overload that includes an index, but returns a strongly-typed <see cref="Enumerated{T}"/> structure.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The sequence to enumerate with indices.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Enumerated{T}"/> containing each value paired with its index.</returns>
    /// <example>
    /// <code>
    /// var fruits = new[] { "apple", "banana", "cherry" };
    /// var enumerated = fruits.Enumerate();
    /// // Results in: [("apple", 0), ("banana", 1), ("cherry", 2)]
    /// </code>
    /// </example>
    public static IEnumerable<Enumerated<T>> Enumerate<T>(this IEnumerable<T> source) =>
        source.Select((value, index) => new Enumerated<T>(value, index));

    /// <summary>
    /// Applies an accumulator function over a sequence, returning each intermediate result.
    /// Unlike <c>Aggregate</c>, this method yields the seed value first, then each intermediate accumulation result.
    /// Useful for seeing how a computation progresses step by step.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the accumulator and result values.</typeparam>
    /// <param name="source">The sequence to scan over.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="scanner">The accumulator function to apply to each element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the seed and all intermediate accumulation results.</returns>
    /// <example>
    /// <code>
    /// var numbers = new[] { 1, 2, 3, 4 };
    /// var sums = numbers.Scan(0, (acc, x) => acc + x);
    /// // Results in: [0, 1, 3, 6, 10] (running sum)
    /// </code>
    /// </example>
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

    /// <summary>
    /// Applies an accumulator function over a sequence, using the first element as the seed and returning each intermediate result.
    /// If the sequence is empty, returns an empty sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to scan over.</param>
    /// <param name="scanner">The accumulator function to apply to each element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the first element and all intermediate accumulation results.</returns>
    /// <example>
    /// <code>
    /// var numbers = new[] { 10, 2, 3, 4 };
    /// var products = numbers.Scan((acc, x) => acc * x);
    /// // Results in: [10, 20, 60, 240] (running product)
    /// </code>
    /// </example>
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

    /// <summary>
    /// Transforms and filters a sequence in a single operation using an <see cref="Option{T}"/>-returning selector.
    /// Elements that produce <c>Some</c> values are included in the result with their transformed values.
    /// Elements that produce <c>None</c> are filtered out. This combines the functionality of <c>Select</c> and <c>Where</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the transformed elements. Must be non-null.</typeparam>
    /// <param name="source">The sequence to transform and filter.</param>
    /// <param name="conditionalSelector">A function that transforms elements and determines inclusion via <see cref="Option{T}"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing only the transformed values from <c>Some</c> results.</returns>
    /// <example>
    /// <code>
    /// var strings = new[] { "1", "abc", "42", "xyz" };
    /// var numbers = strings.SelectWhere(s => int.TryParse(s, out var n) ? Some(n) : None&lt;int&gt;());
    /// // Results in: [1, 42] (only successfully parsed integers)
    /// </code>
    /// </example>
    public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Option<TResult>> conditionalSelector) 
        where TResult : notnull 
    =>
        source.SelectMany(item =>
            conditionalSelector(item).ToEnumerable()
        );

    /// <summary>
    /// Returns elements from the start of a sequence as long as a condition is true, including the first element that fails the condition.
    /// This differs from <c>TakeWhile</c> by including the element that causes the predicate to fail, providing "take until" semantics.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to take elements from.</param>
    /// <param name="predicate">The condition to test each element against.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing elements while the predicate is true, plus the first failing element.</returns>
    /// <example>
    /// <code>
    /// var numbers = new[] { 1, 3, 5, 6, 8, 10 };
    /// var result = numbers.TakeWhileInclusive(x => x % 2 == 1);
    /// // Results in: [1, 3, 5, 6] (includes 6, the first even number)
    /// </code>
    /// </example>
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
