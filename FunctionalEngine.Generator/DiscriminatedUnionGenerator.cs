using FunctionalEngine.Generator.Implementations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace FunctionalEngine.Generator;

[Generator("C#")]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    private const string FullAttributeName = $"{nameof(FunctionalEngine)}.{nameof(DiscriminatedUnionAttribute)}";

    private const string TemplateName = "DiscriminatedUnion.sbn";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var templateProvider = context.AdditionalTextsProvider
            .Where(additionalText => Path.GetFileName(additionalText.Path) is TemplateName)
            .Select((additionalText, cancellationToken) => Template.Parse(
                additionalText.GetText(cancellationToken)!.ToString()
            ));

        var unionDefinitionProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName<UnionDefinition?>(
                FullAttributeName,
                (node, _) => node is RecordDeclarationSyntax or ClassDeclarationSyntax,
                (context, cancellationToken) =>
                {
                    var declaration = context.TargetNode;
                    var semanticModel = context.SemanticModel;

                    var unionSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

                    if (unionSymbol is not INamedTypeSymbol namedType)
                    {
                        return null;
                    }

                    var members = namedType.GetTypeMembers()
                        .Where(typeMember => SymbolEqualityComparer.Default.Equals(typeMember.BaseType, unionSymbol))
                        .Select(unionMember => new UnionMember(
                            Name: unionMember.Name,
                            Accessibility: GetUnionAccessibility(unionMember)
                        ))
                        .ToImmutableArray();

                    return new(
                        Name: namedType.Name,
                        Type: GetUnionType(namedType),
                        Accessibility: GetUnionAccessibility(namedType),
                        Namespace: unionSymbol.ContainingNamespace.ToDisplayString(),
                        AttributeSettings: GetAttributeSettings(namedType),
                        Members: members
                    );
                }
            )
            .Where(definition => definition is not null)
            .Select((definition, _) => definition!.Value);
            
    }

    private static UnionAccessibility GetUnionAccessibility(INamedTypeSymbol type) => type.DeclaredAccessibility switch
    {
        Accessibility.Public => UnionAccessibility.Public,
        Accessibility.Internal => UnionAccessibility.Internal,
        _ => throw new InvalidOperationException($"Member accessibility must be 'public' or 'internal'.")
    };

    private static UnionType GetUnionType(INamedTypeSymbol type) => type.IsRecord switch
    {
        true => UnionType.Record,
        false => UnionType.Class
    };

    private static AttributeSettings GetAttributeSettings(INamedTypeSymbol type)
    {
        var unionArguments = type.GetAttributes()
            .SingleOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == FullAttributeName)
            ?.NamedArguments
            .ToImmutableDictionary(
                argument => argument.Key, 
                argument => (TypedConstant?)argument.Value
            );

        return new(
            GenerateMatch: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GenerateMatch))
                ?.Value as bool?
                ?? AttributeDefaults.GenerateMatch,
            GeneratePolymorphicSerialization: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization))
                ?.Value as bool?
                ?? AttributeDefaults.GeneratePolymorphicSerialization,
            GeneratePrivateConstructor: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor))
                ?.Value as bool?
                ?? AttributeDefaults.GeneratePrivateConstructor
        );
    }

    private readonly record struct UnionDefinition(
        string Name, 
        UnionType Type, 
        UnionAccessibility Accessibility,
        string Namespace,
        AttributeSettings AttributeSettings,
        EquatableArray<UnionMember> Members
    );

    private readonly record struct UnionMember(
        string Name,
        UnionAccessibility Accessibility
    );

    private enum UnionType
    {
        Class,
        Record
    }

    private enum UnionAccessibility
    {
        Internal,
        Public
    }

    private readonly record struct AttributeSettings(
        bool GenerateMatch,
        bool GeneratePolymorphicSerialization,
        bool GeneratePrivateConstructor
    );

    private static class AttributeDefaults
    {
        private static readonly DiscriminatedUnionAttribute DefaultInstance = new();

        public static bool GenerateMatch => DefaultInstance.GenerateMatch;

        public static bool GeneratePolymorphicSerialization => DefaultInstance.GeneratePolymorphicSerialization;

        public static bool GeneratePrivateConstructor => DefaultInstance.GeneratePrivateConstructor;
    }
}
