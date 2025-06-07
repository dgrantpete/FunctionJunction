using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FunctionalEngine.Generator.Internal;

internal readonly record struct EquatableArray<T>(ImmutableArray<T> Array) : IEnumerable<T>, IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    public bool Equals(EquatableArray<T> other) =>
        Array.AsSpan().SequenceEqual(other.Array.AsSpan());

    public IEnumerator<T> GetEnumerator() => Array.AsEnumerable()
        .GetEnumerator();

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var element in Array)
        {
            hash.Add(element);
        }

        return hash.ToHashCode();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);
}
