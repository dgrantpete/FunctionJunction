using FunctionJunction.Generator.Internal.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal.Helpers;

internal static class GeneratorHelper
{
    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> enumerable) where T : IEquatable<T> =>
        new([.. enumerable]);

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> source) where T : struct =>
        source.SelectMany((maybeValue, _) => maybeValue switch
        {
            { } value => Enumerable.Repeat(value, 1),
            null => []
        });

    public static ImmutableArray<TResult>? SelectAll<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> selector) where TResult : class
    {
        var resultBuilder = ImmutableArray.CreateBuilder<TResult>();

        foreach (var value in source)
        {
            if (selector(value) is not { } result)
            {
                return null;
            }

            resultBuilder.Add(result);
        }

        return resultBuilder.DrainToImmutable();
    }

    public static ImmutableArray<TResult>? SelectAll<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> selector) where TResult : struct
    {
        var resultBuilder = ImmutableArray.CreateBuilder<TResult>();

        foreach (var value in source)
        {
            if (selector(value) is not { } result)
            {
                return null;
            }

            resultBuilder.Add(result);
        }

        return resultBuilder.DrainToImmutable();
    }

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> source) where T : class =>
        source.SelectMany((maybeValue, _) => maybeValue switch
        {
            { } value => Enumerable.Repeat(value, 1),
            null => []
        });
}
