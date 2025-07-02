using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal;

internal static class Diagnostics
{
    public static DiagnosticDescriptor MissingDerivedTypes { get; } = new(
        id: "FJ0001",
        title: "Discriminated union has no variants",
        messageFormat: "The discriminated union '{0}' declares no variants; add at least 1 variant by creating a type which is defined inside and inherits from '{0}'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor NotMarkedPartial { get; } = new(
        id: "FJ0002",
        title: "Discriminated union not marked partial",
        messageFormat: "The discriminated union '{0}' must have 'partial' in its declaration",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ConstructorAlreadyExists { get; } = new(
        id: "FJ0003",
        title: "Discriminated union constructor already exists",
        messageFormat: "If 'GeneratePrivateConstructor' is set to 'true', no constructor should be explicitly defined for '{0}'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DerivedTypeAttributeNotFound { get; } = new(
        id: "FJ0004",
        title: "'JsonDerivedTypeAttribute' not found",
        messageFormat: "The 'JsonDerivedTypeAttribute' was not found but 'GeneratePolymorphicSerialization' was set to 'true' for '{0}'; " +
        "install a version of 'System.Text.Json' which supports polymorphic serialization or set 'GeneratePolymorphicSerialization' to 'false'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor SwitchExpressionsNotSupported { get; } = new(
        id: "FJ0005",
        title: "Switch expressions not supported on targeted C# version",
        messageFormat: "Switch expressions are required to generate 'Match' method but are not supported on the C# version you've set ({0}); " +
        "upgrade C# version to 8.0 or greater or set 'MatchOn' to 'None'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor GenericsIncompatibleWithSerialization { get; } = new(
        id: "FJ0006",
        title: "Can't generate 'JsonDerivedTypeAttribute' for generic type",
        messageFormat: "'JsonDerivedTypeAttribute' cannot be generated for a generic discriminated union; " +
        "set 'GeneratePolymorphicSerialization' to 'false'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor MemberAccessibilityInvalid { get; } = new(
        id: "FJ0007",
        title: "Invalid member accessibility",
        messageFormat: "Accessibility for member '{0}' must be marked 'public' or 'internal'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ObjectKindInvalid { get; } = new(
        id: "FJ0008",
        title: "Invalid object kind",
        messageFormat: "'{0}' must be a 'class' or a 'record' to be a discriminated union",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DeconstructMethodNotFound { get; } = new(
        id: "FJ0009",
        title: "Deconstruct method not found",
        messageFormat: "Member '{0}' does not have a valid 'Deconstruct' method for properties matching",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static Diagnostic Create(
        DiagnosticDescriptor descriptor,
        ImmutableArray<Location> locations,
        params object?[]? messageArgs
    )
    {
        var (firstLocation, restOfLocations) = locations switch
        {
            [var first, .. var rest] => (first, rest),
            _ => (default, [])
        };

        return Diagnostic.Create(
            descriptor,
            firstLocation,
            restOfLocations,
            messageArgs
        );
    }
}
