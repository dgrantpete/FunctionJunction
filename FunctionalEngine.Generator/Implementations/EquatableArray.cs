using System;
using System.Collections.Immutable;
using System.Linq;

namespace FunctionalEngine.Generator.Implementations;

internal readonly record struct EquatableArray<T>(ImmutableArray<T> Array) : IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    public bool Equals(EquatableArray<T> other) =>
        Array.AsSpan().SequenceEqual(other.Array.AsSpan());

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var element in Array)
        {
            hash.Add(element);
        }

        return hash.ToHashCode();
    }

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);
}
