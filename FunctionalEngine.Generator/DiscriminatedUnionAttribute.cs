using System;

namespace FunctionalEngine.Generator;

[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatedUnionAttribute : Attribute
{
    public MatchUnionOn MatchOn { get; set; }

    public bool GeneratePolymorphicSerialization { get; set; }

    public bool GeneratePrivateConstructor { get; set; }
}
