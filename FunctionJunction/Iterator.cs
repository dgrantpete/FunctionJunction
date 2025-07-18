namespace FunctionJunction;

/// <summary>
/// Provides static methods for creating various types of iterators and generators.
/// </summary>
public static class Iterator
{
    /// <summary>
    /// Creates an enumerable that iterates using the specified function, starting with the seed value.
    /// The iteration continues until the iterator function returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the sequence.</typeparam>
    /// <param name="seed">The initial value for the iteration.</param>
    /// <param name="iterator">A function that computes the next value from the current value, or returns <c>None</c> to end the iteration.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that yields the iteration sequence.</returns>
    public static IEnumerable<T> Iterate<T>(T seed, Func<T, Option<T>> iterator) where T : notnull
    {
        yield return seed;

        var currentValue = seed;

        while (iterator(currentValue).TryUnwrap(out currentValue))
        {
            yield return currentValue;
        }
    }

    /// <summary>
    /// Creates an async enumerable that iterates using the specified async function, starting with the seed value.
    /// The iteration continues until the iterator function returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the sequence.</typeparam>
    /// <param name="seed">The initial value for the iteration.</param>
    /// <param name="iteratorAsync">An async function that computes the next value from the current value, or returns <c>None</c> to end the iteration.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the iteration sequence.</returns>
    public static async IAsyncEnumerable<T> Iterate<T>(T seed, Func<T, ValueTask<Option<T>>> iteratorAsync) where T : notnull
    {
        yield return seed;

        var currentValue = seed;

        while ((await iteratorAsync(currentValue)).TryUnwrap(out currentValue))
        {
            yield return currentValue;
        }
    }

    /// <summary>
    /// Creates an enumerable that supports hierarchical traversal of data structures.
    /// Returns an <see cref="IterateManyEnumerable{T}"/> that can be configured with different traversal strategies.
    /// </summary>
    /// <typeparam name="T">The type of values in the hierarchy.</typeparam>
    /// <param name="seed">The root value to start the traversal from.</param>
    /// <param name="iterator">A function that returns the children of a given node.</param>
    /// <returns>An <see cref="IterateManyEnumerable{T}"/> that can be configured for different traversal patterns.</returns>
    public static IterateManyEnumerable<T> IterateMany<T>(T seed, Func<T, IEnumerable<T>> iterator) =>
        new(seed, iterator);

    /// <summary>
    /// Creates an infinite enumerable that generates values using the specified function.
    /// </summary>
    /// <typeparam name="T">The type of values to generate.</typeparam>
    /// <param name="generator">A function that produces values.</param>
    /// <returns>An infinite <see cref="IEnumerable{T}"/> that yields generated values.</returns>
    public static IEnumerable<T> Generate<T>(Func<T> generator)
    {
        while (true)
        {
            yield return generator();
        }
    }

    /// <summary>
    /// Creates an infinite async enumerable that generates values using the specified async function.
    /// </summary>
    /// <typeparam name="T">The type of values to generate.</typeparam>
    /// <param name="generatorAsync">An async function that produces values.</param>
    /// <returns>An infinite <see cref="IAsyncEnumerable{T}"/> that yields generated values.</returns>
    public static async IAsyncEnumerable<T> Generate<T>(Func<ValueTask<T>> generatorAsync)
    {
        while (true)
        {
            yield return await generatorAsync().ConfigureAwait(false);
        }
    }
}
