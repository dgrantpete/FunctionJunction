using System.Collections;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal.Models;

internal readonly record struct EquatableArray<T>(ImmutableArray<T> Array) : IReadOnlyList<T>, IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    public T this[int index] => Array[index];

    public int Count => Array.IsDefault switch
    {
        true => 0,
        false => Array.Length
    };

    public bool Equals(EquatableArray<T> other) =>
        Array.AsSpan().SequenceEqual(other.Array.AsSpan());

    public EquatableArray<T> AddRange(EquatableArray<T> array) =>
        new(Array.AddRange(array.Array));

    public IEnumerator<T> GetEnumerator()
    {
        if (Array.IsDefault)
        {
            return EmptyEnumerator();
        }

        return Array.AsEnumerable()
            .GetEnumerator();

        static IEnumerator<T> EmptyEnumerator()
        {
            yield break;
        }
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Enumerable.Aggregate(Array, 17, (current, element) => current * 31 + (element?.GetHashCode() ?? 0));
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);
}
