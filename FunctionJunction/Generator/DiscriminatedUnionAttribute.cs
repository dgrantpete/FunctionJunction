using System.Diagnostics;

#if MAIN_PROJECT
namespace FunctionJunction.Generator;
#else
namespace FunctionJunction.Generator.Internal.Attributes;
#endif

/// <summary>
/// An attribute used on classes to opt-in to discriminated union code generation.
/// The top-level class will be defined as the discriminated union type, while nested classes will be defined as derived types of that union.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[Conditional(GeneratorDefault.AttributeInclusionSymbol)]
public class DiscriminatedUnionAttribute : Attribute
{
    /// <summary>
    /// The type of <c>Match</c> method to generate for this discriminated union.
    /// </summary>
    public MatchUnionOn MatchOn { get; set; } = MatchUnionOn.Type;

    /// <summary>
    /// <para>Specifies how polymorphic serialization should be configured for this discriminated union.</para>
    /// <para>Controls whether <c>JsonDerivedType</c> attributes are generated and how type discriminator values are formatted.</para>
    /// <para>Any members which already have <c>JsonDerivedType</c> specified will be skipped during generation.</para>
    /// </summary>
    public JsonPolymorphism JsonPolymorphism { get; set; } = JsonPolymorphism.Enabled;

    /// <summary>
    /// Whether a private constructor should be generated for this discriminated union (this prevents anybody outside of the union from adding more members).
    /// </summary>
    public bool GeneratePrivateConstructor { get; set; } = true;
}
