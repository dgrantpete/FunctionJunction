using FunctionJunction.Generator.Internal.Attributes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal.Helpers;

internal static class DiagnosticHelper
{
    public static DiagnosticDescriptor MissingDerivedTypes { get; } = new(
        id: "FJ0001",
        title: "Discriminated union has no derived types",
        messageFormat: "The discriminated union '{0}' declares no derived types; add at least 1 derived type by creating a type which is defined inside and inherits from '{0}'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor NotMarkedPartial { get; } = new(
        id: "FJ0002",
        title: "Discriminated union not marked partial",
        messageFormat: "The discriminated union '{0}' must have 'partial' in its declaration",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DerivedTypeAttributeNotFound { get; } = new(
        id: "FJ0003",
        title: "'JsonDerivedTypeAttribute' not found",
        messageFormat: $"'{TypeName.JsonDerivedTypeAttribute}' was not found but '{nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization)}' was set to 'true' for '{{0}}'; " +
        $"install a version of 'System.Text.Json' that supports polymorphic serialization or set '{nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization)}' to 'false'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor SwitchExpressionsNotSupported { get; } = new(
        id: "FJ0004",
        title: "Switch expressions not supported on targeted C# version",
        messageFormat: "Switch expressions are required to generate 'Match' method but are not supported on the C# version you've set ({0}); " +
        $"upgrade C# version to 8.0 or greater or set '{nameof(DiscriminatedUnionAttribute.MatchOn)}' to '{nameof(MatchUnionOn.None)}'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor GenericsIncompatibleWithSerialization { get; } = new(
        id: "FJ0005",
        title: $"Can't generate '{nameof(TypeName.JsonDerivedTypeAttribute)}' for generic type",
        messageFormat: $"'{TypeName.JsonDerivedTypeAttribute}' cannot be generated for the generic discriminated union '{{0}}'; " +
        $"set '{nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization)}' to 'false'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DerivedTypeAccessibilityInvalid { get; } = new(
        id: "FJ0006",
        title: "Invalid member accessibility",
        messageFormat: "Accessibility for derived type '{0}' must be 'public' or 'internal'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ObjectKindInvalid { get; } = new(
        id: "FJ0007",
        title: "Invalid object kind",
        messageFormat: "'{0}' must be a 'class' or a 'record' to be a discriminated union",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DerivedTypeCanBeSealed { get; } = new(
        id: "FJ0008",
        title: "Derived type can be sealed",
        messageFormat: "Consider adding 'sealed' to derived type '{0}' to prevent additional derived types being added via inheritance",
        category: "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static IEnumerable<DiagnosticDescriptor> IterateDiagnostics()
    {
        yield return MissingDerivedTypes;
        yield return NotMarkedPartial;
        yield return DerivedTypeAttributeNotFound;
        yield return SwitchExpressionsNotSupported;
        yield return GenericsIncompatibleWithSerialization;
        yield return DerivedTypeAccessibilityInvalid;
        yield return ObjectKindInvalid;
        yield return DerivedTypeCanBeSealed;
    }

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
