namespace FunctionJunction;

/// <summary>
/// Represents a value paired with its index position; typically used in enumeration scenarios.
/// This type combines a value of type <typeparamref name="T"/> with its zero-based index position,
/// providing a convenient way to track both the value and its position in a sequence.
/// </summary>
/// <remarks>
/// <b>Default instance: </b>
/// <see langword="default"/>(<see cref="Enumerated{T}"/>) is equivalent to (<see langword="default"/>(<typeparamref name="T"/>), 0).
/// Dereference <paramref name="Value"/> only if you understand the semantics of <see langword="default"/>(<typeparamref name="T"/>); nullable warnings won't be enforced if <typeparamref name="T"/> is a non-nullable <see langword="class"/>.
/// </remarks>
/// <param name="Index">The zero-based index position of the value in the sequence.</param>
/// <param name="Value">The value at this position in the sequence.</param>
public readonly record struct Enumerated<T>(int Index, T Value)
{
    public Enumerated<T> Next(T nextValue) =>
        new(Index + 1, nextValue);

    public Enumerated<T> Next(Func<T, T> nextMapper) =>
        new(Index + 1, nextMapper(Value));
}

public static class Enumerated
{
    public static Enumerated<T> Start<T>(T startValue) =>
        new(0, startValue);

    public static Enumerated<T> StartAt<T>(int startIndex, T startValue) =>
        new(startIndex, startValue);
}
