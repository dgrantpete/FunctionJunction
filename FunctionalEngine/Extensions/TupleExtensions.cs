namespace FunctionalEngine.Extensions;

#pragma warning disable CS1591

/// <summary>
/// Provides extension methods for tuples that enable flattening nested tuple structures.
/// These methods help transform complex nested tuples into simpler flat tuple structures.
/// </summary>
public static class TupleExtensions
{
    /// <summary>
    /// Flattens a nested tuple structure ((T1, T2), T3) into a flat 3-tuple (T1, T2, T3).
    /// This provides a convenient way to unnest tuple structures created by tuple composition.
    /// </summary>
    /// <typeparam name="T1">The type of the first element.</typeparam>
    /// <typeparam name="T2">The type of the second element.</typeparam>
    /// <typeparam name="T3">The type of the third element.</typeparam>
    /// <param name="tuple">The nested tuple to flatten.</param>
    /// <returns>A flat 3-tuple containing all the elements.</returns>
    public static (T1, T2, T3) Coalesce<T1, T2, T3>(this ((T1, T2), T3) tuple) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item2);

    public static (T1, T2, T3) Coalesce<T1, T2, T3>(this (T1, (T2, T3)) tuple) =>
        (tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2);

    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this ((T1, T2), T3, T4) tuple
    ) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item2, tuple.Item3);

    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this ((T1, T2, T3), T4) tuple
    ) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item1.Item3, tuple.Item2);

    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this (T1, (T2, T3), T4) tuple
    ) =>
        (tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2, tuple.Item3);

    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this (T1, (T2, T3, T4)) tuple
    ) =>
        (tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2, tuple.Item2.Item3);

    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this (T1, T2, (T3, T4)) tuple
    ) =>
        (tuple.Item1, tuple.Item2, tuple.Item3.Item1, tuple.Item3.Item2);

    public static (T1, T2, T3, T4) Coalesce<T1, T2, T3, T4>(
        this ((T1, T2), (T3, T4)) tuple
    ) =>
        (tuple.Item1.Item1, tuple.Item1.Item2, tuple.Item2.Item1, tuple.Item2.Item2);
}
