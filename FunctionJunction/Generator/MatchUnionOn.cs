#if MAIN_PROJECT
namespace FunctionJunction.Generator;
#else
namespace FunctionJunction.Generator.Internal.Attributes;
#endif

/// <summary>
/// Specifies how a <c>Match</c> function should be generated.
/// </summary>
public enum MatchUnionOn
{
    /// <summary>
    /// Do not generate a <c>Match</c> function for this discriminated union.
    /// </summary>
    None,
    /// <summary>
    /// Generates a <c>Match</c> function where the nested type itself is passed directly into each <c>Func</c>.
    /// </summary>
    Type,
    /// <summary>
    /// <para>Generates a <c>Match</c> function where the public properties of the nested type are "deconstructed" and passed into the <c>Func</c>.</para>
    /// <para>Useful for simple discriminated union members whose type is a simple wrapper around its contents.</para>
    /// </summary>
    Properties
}
