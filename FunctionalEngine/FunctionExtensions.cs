﻿namespace FunctionalEngine;

public static class FunctionExtensions
{
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
