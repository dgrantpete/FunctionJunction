using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal;

internal readonly record struct GeneratorResult<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; init; }

    public GeneratorResult(T value)
    {
        IsSuccess = true;
        Value = value;
    }
}
