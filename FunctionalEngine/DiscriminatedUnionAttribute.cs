namespace FunctionalEngine;

[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatedUnionAttribute : Attribute
{
    public bool GenerateMatch { get; init; } = true;

    public bool GeneratePolymorphicSerialization { get; init; } = true;

    public bool GeneratePrivateConstructor { get; init; } = true;
}
