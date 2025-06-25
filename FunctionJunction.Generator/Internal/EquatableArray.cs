using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FunctionJunction.Generator.Internal;

internal readonly record struct EquatableArray<T>(ImmutableArray<T> Array) : IReadOnlyList<T>, IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    public T this[int index] => Array[index];

    public int Count => Array.Length;

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
