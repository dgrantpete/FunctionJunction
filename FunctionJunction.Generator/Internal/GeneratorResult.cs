using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal;

internal readonly record struct GeneratorResult<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    private readonly ImmutableArray<Diagnostic> diagnostics;

    public ImmutableArray<Diagnostic> Diagnostics
    {
        get => diagnostics.IsDefault switch
        {
            true => [],
            false => diagnostics
        };
        init
        {
            diagnostics = value;
        }
    }

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

    public bool TryGetValue(out T? result)
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
    {
        if (TryGetValue(out var value))
        {
            var newResult = mapper(value!);

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
        Func<GeneratorResult<TOther>> otherProvider, 
        Func<T, TOther, TResult> otherSelector
    ) =>
        FlatMap(value => otherProvider().Map(otherValue => otherSelector(value, otherValue)));
}

internal static class GeneratorResult
{
    public static GeneratorResult<T> Ok<T>(T value, ImmutableArray<Diagnostic> diagnostics = default) =>
        new(value) { Diagnostics = diagnostics };

    public static GeneratorResult<T> Error<T>(ImmutableArray<Diagnostic> diagnostics) =>
        new() { Diagnostics = diagnostics };

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
