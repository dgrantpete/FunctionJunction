using Microsoft.CodeAnalysis;

namespace FunctionalEngine.Generator.Internal;

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
        "upgrade C# version to 8.0 or greater or set 'GenerateMatch' to 'false'",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}
