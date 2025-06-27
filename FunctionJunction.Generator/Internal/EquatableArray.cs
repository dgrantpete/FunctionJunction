using System.Collections;
using System.Collections.Immutable;

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
        unchecked
        {
            int hash = 17;
            foreach (var element in Array)
            {
                hash = hash * 31 + (element?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);
}
