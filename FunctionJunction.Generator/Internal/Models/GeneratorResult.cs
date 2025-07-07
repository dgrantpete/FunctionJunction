using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FunctionJunction.Generator.Internal.Models;

internal readonly record struct GeneratorResult<T> : IEquatable<GeneratorResult<T>> where T : notnull
{
    public bool IsSuccess { get; }

    public T Value { get; }

    public EquatableArray<Diagnostic> Diagnostics { get; init; }

    public GeneratorResult(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    public static implicit operator GeneratorResult<T>(T value) => new(value);

    public static implicit operator GeneratorResult<T>(ImmutableArray<Diagnostic> diagnostics) => new()
    {
        Diagnostics = diagnostics
    };

    public bool TryGetValue([NotNullWhen(true)] out T? result)
    {
        if (IsSuccess)
        {
            result = Value;
            return true;
        }

        result = default;
        return false;
    }

    public GeneratorResult<TResult> FlatMap<TResult>(Func<T, GeneratorResult<TResult>> mapper)
        where TResult : notnull
    {
        if (TryGetValue(out var value))
        {
            var newResult = mapper(value);

            return newResult with
            {
                Diagnostics = Diagnostics.AddRange(newResult.Diagnostics)
            };
        }

        return new()
        {
            Diagnostics = Diagnostics
        };
    }

    public GeneratorResult<TResult> Map<TResult>(Func<T, TResult> mapper)
        where TResult : notnull
    {
        if (TryGetValue(out var value))
        {
            return new(mapper(value!))
            {
                Diagnostics = Diagnostics
            };
        }

        return new()
        {
            Diagnostics = Diagnostics
        };
    }

    public GeneratorResult<TResult> And<TOther, TResult>(
        GeneratorResult<TOther> otherResult, 
        Func<T, TOther, TResult> selector
    ) 
        where TResult : notnull
        where TOther : notnull
    {
        var diagnostics = Diagnostics.AddRange(otherResult.Diagnostics);

        if (TryGetValue(out var value) && otherResult.TryGetValue(out var otherValue))
        {
            return new(selector(value, otherValue))
            {
                Diagnostics = diagnostics
            };
        }

        return new()
        {
            Diagnostics = diagnostics
        };
    }
}

internal static class GeneratorResult
{
    public static GeneratorResult<T> Ok<T>(T value, ImmutableArray<Diagnostic> diagnostics = default)
        where T : notnull    
    =>
        new(value) { Diagnostics = diagnostics };

    public static GeneratorResult<T> Error<T>(ImmutableArray<Diagnostic> diagnostics)
        where T : notnull
    =>
        new() { Diagnostics = diagnostics };

    public static GeneratorResult<ImmutableArray<T>> All<T>(IEnumerable<GeneratorResult<T>> results)
        where T : notnull
    {
        var maybeOkValues = ImmutableArray.CreateBuilder<T>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var result in results)
        {
            diagnostics.AddRange(result.Diagnostics);

            if (!result.TryGetValue(out var value))
            {
                maybeOkValues = null;
                continue;
            }

            maybeOkValues?.Add(value);
        }

        return maybeOkValues switch
        {
            { } okValues => new GeneratorResult<ImmutableArray<T>>(okValues.DrainToImmutable())
            {
                Diagnostics = diagnostics.DrainToImmutable()
            },
            null => new GeneratorResult<ImmutableArray<T>>()
            {
                Diagnostics = diagnostics.DrainToImmutable()
            }
        };
    }

    public static GeneratorResult<T> FromNullable<T>(T? maybeValue, Func<ImmutableArray<Diagnostic>> diagnosticsProvider) 
        where T : struct 
    => 
        maybeValue switch
        {
            { } value => Ok(value),
            _ => Error<T>(diagnosticsProvider())
        };

    public static GeneratorResult<T> FromNullable<T>(T? maybeValue, Func<ImmutableArray<Diagnostic>> diagnosticsProvider)
        where T : class 
    =>
        maybeValue switch
        {
            { } value => Ok(value),
            _ => Error<T>(diagnosticsProvider())
        };
}
