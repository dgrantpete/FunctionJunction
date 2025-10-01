#if MAIN_PROJECT
namespace FunctionJunction.Generator;
#else
namespace FunctionJunction.Generator.Internal.Attributes;
#endif

/// <summary>
/// Specifies how polymorphic serialization should be configured for a discriminated union.
/// </summary>
public enum JsonPolymorphism
{
    /// <summary>
    /// Don't generate <c>JsonDerivedType</c> attributes for this discriminated union.
    /// </summary>
    Disabled,
    /// <summary>
    /// Generate <c>JsonDerivedType</c> attributes using the type name as-is (no casing transformation).
    /// </summary>
    Enabled,
    /// <summary>
    /// Generate <c>JsonDerivedType</c> attributes with type discriminators in camelCase (e.g., "successResult").
    /// </summary>
    CamelCase,
    /// <summary>
    /// Generate <c>JsonDerivedType</c> attributes with type discriminators in snake_case (e.g., "success_result").
    /// </summary>
    SnakeCaseLower,
    /// <summary>
    /// Generate <c>JsonDerivedType</c> attributes with type discriminators in SCREAMING_SNAKE_CASE (e.g., "SUCCESS_RESULT").
    /// </summary>
    SnakeCaseUpper,
    /// <summary>
    /// Generate <c>JsonDerivedType</c> attributes with type discriminators in kebab-case (e.g., "success-result").
    /// </summary>
    KebabCaseLower,
    /// <summary>
    /// Generate <c>JsonDerivedType</c> attributes with type discriminators in SCREAMING-KEBAB-CASE (e.g., "SUCCESS-RESULT").
    /// </summary>
    KebabCaseUpper
}
