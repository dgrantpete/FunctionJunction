using FunctionalEngine.Generator;

namespace FunctionalEngine;

/// <summary>
/// Represents a value paired with its index position, typically used in enumeration scenarios.
/// This type combines a value of type <typeparamref name="T"/> with its zero-based index position,
/// providing a convenient way to track both the value and its position in a sequence.
/// </summary>
/// <typeparam name="T">The type of the value being enumerated.</typeparam>
/// <param name="Value">The value at this position in the sequence.</param>
/// <param name="Index">The zero-based index position of the value in the sequence.</param>
public readonly record struct Enumerated<T>(T Value, int Index);

[DiscriminatedUnion]
public partial record Foo
{
    public record Bar(int A) : Foo;
}