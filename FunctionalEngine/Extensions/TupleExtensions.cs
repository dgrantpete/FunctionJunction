namespace FunctionalEngine.Extensions;

/// <summary>
/// Provides extension methods for tuples that enable flattening nested tuple structures.
/// These methods help transform complex nested tuples into simpler flat tuple structures.
/// </summary>
public static class TupleExtensions
{
    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3) Coalesce<T1, T2, T3>(this ((T1, T2), T3) tuple) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item2);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3) Coalesce<T1, T2, T3>(this (T1, (T2, T3)) tuple) =>
        (tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this ((T1, T2), T3, T4) tuple
    ) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item2, tuple.Item3);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this ((T1, T2, T3), T4) tuple
    ) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item1.Item3, tuple.Item2);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this (T1, (T2, T3), T4) tuple
    ) =>
        (tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2, tuple.Item3);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this (T1, (T2, T3, T4)) tuple
    ) =>
        (tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2, tuple.Item2.Item3);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this (T1, T2, (T3, T4)) tuple
    ) =>
        (tuple.Item1, tuple.Item2, tuple.Item3.Item1, tuple.Item3.Item2);

    /// <summary>
    /// Takes the nested tuple elements and transforms them into a single, flattened tuple.
    /// </summary>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flattened tuple containing all the elements from the nested structure.</returns>
    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this ((T1, T2), (T3, T4)) tuple
    ) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item2.Item1, tuple.Item2.Item2);
}
