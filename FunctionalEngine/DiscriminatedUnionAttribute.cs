namespace FunctionalEngine;

[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatedUnionAttribute : Attribute
{
    public bool GenerateMatch { get; set; } = true;

    public bool GeneratePolymorphicSerialization { get; set; } = true;

    public bool GeneratePrivateConstructor { get; set; } = true;
}
