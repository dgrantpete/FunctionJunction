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
    /// <summary>
    /// Creates a new <see cref="Enumerated{T}"/> with the next index and the specified value.
    /// </summary>
    /// <param name="nextValue">The value for the next position.</param>
    /// <returns>A new <see cref="Enumerated{T}"/> with <see cref="Index"/> incremented by 1 and <see cref="Value"/> set to <paramref name="nextValue"/>.</returns>
    public Enumerated<T> Next(T nextValue) =>
        new(Index + 1, nextValue);

    /// <summary>
    /// Creates a new <see cref="Enumerated{T}"/> with the next index and a value computed by applying the specified function to the current value.
    /// </summary>
    /// <param name="nextMapper">A function that transforms the current value to produce the next value.</param>
    /// <returns>A new <see cref="Enumerated{T}"/> with <see cref="Index"/> incremented by 1 and <see cref="Value"/> set to the result of <paramref name="nextMapper"/>(<see cref="Value"/>).</returns>
    public Enumerated<T> Next(Func<T, T> nextMapper) =>
        new(Index + 1, nextMapper(Value));
}

/// <summary>
/// Provides static factory methods for creating <see cref="Enumerated{T}"/> instances.
/// </summary>
public static class Enumerated
{
    /// <summary>
    /// Creates a new <see cref="Enumerated{T}"/> starting at index 0 with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="startValue">The initial value.</param>
    /// <returns>A new <see cref="Enumerated{T}"/> with <see cref="Enumerated{T}.Index"/> set to 0 and <see cref="Enumerated{T}.Value"/> set to <paramref name="startValue"/>.</returns>
    public static Enumerated<T> Start<T>(T startValue) =>
        new(0, startValue);

    /// <summary>
    /// Creates a new <see cref="Enumerated{T}"/> starting at the specified index with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="startIndex">The initial index position.</param>
    /// <param name="startValue">The initial value.</param>
    /// <returns>A new <see cref="Enumerated{T}"/> with <see cref="Enumerated{T}.Index"/> set to <paramref name="startIndex"/> and <see cref="Enumerated{T}.Value"/> set to <paramref name="startValue"/>.</returns>
    public static Enumerated<T> StartAt<T>(int startIndex, T startValue) =>
        new(startIndex, startValue);
}
