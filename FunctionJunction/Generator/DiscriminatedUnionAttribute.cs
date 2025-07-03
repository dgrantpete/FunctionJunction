#if MAIN_PROJECT
namespace FunctionJunction.Generator;
#else
namespace FunctionJunction.Generator.Internal.Attributes;
#endif

/// <summary>
/// An attribute used on classes to opt-in to discriminated union code generation.
/// The top-level class will be defined as the discriminated union type, while nested classes will be defined as members of that union.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatedUnionAttribute : Attribute
{
    /// <summary>
    /// The type of <c>Match</c> method to generate for this discriminated union.
    /// </summary>
    public MatchUnionOn MatchOn { get; set; } = MatchUnionOn.Type;

    /// <summary>
    /// <para>Whether or not to generate polymorphic serialization attributes (<c>JsonDerivedType</c>) for the members of the discriminated union.</para>
    /// <para>Any members which have <c>JsonDerivedType</c> already specified will be skipped during generation.</para>
    /// </summary>
    public bool GeneratePolymorphicSerialization { get; set; } = true;

    /// <summary>
    /// Whether or not a private constructor should be generated for this discriminated union (this prevents anybody outside of the union from adding more members).
    /// </summary>
    public bool GeneratePrivateConstructor { get; set; } = true;
}
