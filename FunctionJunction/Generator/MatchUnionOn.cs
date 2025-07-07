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
    /// Don't generate a <c>Match</c> function for this discriminated union.
    /// </summary>
    None,
    /// <summary>
    /// Generate a <c>Match</c> function where the derived type itself is passed directly into a <see cref="Func{TDerivedType, TResult}"/>.
    /// </summary>
    Type,
    /// <summary>
    /// <para>Generate a <c>Match</c> function that calls the <c>Deconstruct</c> method for each derived type, then passes these deconstructed values together into their respective <c>Func</c> (in the same order as the <see langword="out"/> parameters).</para>
    /// <para>If no <c>Deconstruct</c> method is defined, a parameterless <see cref="Func{TResult}"/> will be used.</para>
    /// <para>Useful for simple derived types whose type is a simple wrapper around its contents.</para>
    /// </summary>
    Deconstruct
}
