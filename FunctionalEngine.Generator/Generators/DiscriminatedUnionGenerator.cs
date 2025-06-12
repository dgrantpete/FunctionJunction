using FunctionalEngine.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FunctionalEngine.Generator.Generators;

[Generator("C#")]
internal class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    private const string JsonDerivedTypeAttribute = "System.Text.Json.Serialization.JsonDerivedTypeAttribute";

    private static readonly Lazy<Template> template = new(() =>
    {
        var assembly = typeof(DiscriminatedUnionGenerator).Assembly;

        var resourceName = assembly.GetManifestResourceNames()
            .Single(resource => resource.EndsWith(DiscriminatedUnionDefaults.TemplateName));

        using var templateStream = assembly.GetManifestResourceStream(resourceName);

        using var reader = new StreamReader(templateStream);

        var templateText = reader.ReadToEnd();

        return Template.Parse(templateText);
    });

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var defaultAttributeProvider = context.AnalyzerConfigOptionsProvider
            .Select((options, _) => GetDefaultAttributeSettings(options.GlobalOptions));

        var parseInfoProvider = context.ParseOptionsProvider
            .Select(static (parseOptions, _) =>
            {
                var languageVersion = ((CSharpParseOptions)parseOptions)
                    .LanguageVersion;

                var switchExpressionsSupported = languageVersion >= LanguageVersion.CSharp8;

                return new ParseInfo(languageVersion, switchExpressionsSupported);
            });

        var compilationInfoProvider = context.CompilationProvider
            .Select(static (compilation, _) =>
            {
                var polymorphicAttribute = compilation
                    .GetTypeByMetadataName(JsonDerivedTypeAttribute);

                var serializationAttributeExists = polymorphicAttribute is not null;

                return new CompilationInfo(serializationAttributeExists);
            });

        var unionDefinitionProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DiscriminatedUnionDefaults.AttributeName,
                static (node, _) => node is RecordDeclarationSyntax or ClassDeclarationSyntax,
                static (context, _) => context
            )
            .Combine(defaultAttributeProvider)
            .Select(static (data, cancellationToken) => GetUnionDefinition(data.Left, data.Right, cancellationToken))
            .SelectMany<UnionDefinition?, UnionDefinition>((maybeDefinition, _) => maybeDefinition switch
            {
                null => [],
                { } definition => [definition]
            });

        var unionProvider = unionDefinitionProvider.Combine(compilationInfoProvider)
            .Combine(parseInfoProvider)
            .Select(static (data, _) =>
                (
                    UnionDefinition: data.Left.Left,
                    CompilationInfo: data.Left.Right,
                    ParseInfo: data.Right
                )
            );

        context.RegisterSourceOutput(unionProvider, static (context, data) =>
        {
            var definition = data.UnionDefinition;
            var compilation = data.CompilationInfo;
            var parse = data.ParseInfo;

            if (definition.Failure is { } failure)
            {
                context.ReportDiagnostic(failure);
                return;
            }

            if (definition.AttributeSettings.MatchOn is not MatchUnionOn.None && !parse.SwitchExpressionsSupported)
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.SwitchExpressionsNotSupported,
                    definition.AttributeLocation,
                    parse.LanguageVersion.ToDisplayString()
                );

                context.ReportDiagnostic(diagnostic);

                definition = definition with
                {
                    AttributeSettings = definition.AttributeSettings with { MatchOn = MatchUnionOn.None }
                };
            }

            if (definition.AttributeSettings.GeneratePolymorphicSerialization && !compilation.SerializationAttributesExist)
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.DerivedTypeAttributeNotFound,
                    definition.AttributeLocation,
                    definition.Name
                );

                context.ReportDiagnostic(diagnostic);

                definition = definition with 
                { 
                    AttributeSettings = definition.AttributeSettings with { GeneratePolymorphicSerialization = false } 
                };
            }

            if (definition.AttributeSettings.GeneratePolymorphicSerialization && (definition.Name.Plain != definition.Name.WithGenerics))
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.GenericsIncompatibleWithSerialization,
                    definition.AttributeLocation
                );

                context.ReportDiagnostic(diagnostic);

                definition = definition with
                {
                    AttributeSettings = definition.AttributeSettings with { GeneratePolymorphicSerialization = false }
                };
            }

            var generatedCode = template.Value.Render(definition);

            context.AddSource($"{definition.Name.Plain}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        });
    }

    private static UnionDefinition? GetUnionDefinition(GeneratorAttributeSyntaxContext context, AttributeSettings defaults, CancellationToken cancellationToken)
    {
        var declaration = (TypeDeclarationSyntax)context.TargetNode;

        var semanticModel = context.SemanticModel;

        var unionSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

        if (unionSymbol is not INamedTypeSymbol namedType)
        {
            return null;
        }

        var unionName = GetTypeName(namedType);

        if (!declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.NotMarkedPartial,
                declaration.Identifier.GetLocation(),
                namedType.Name
            );

            return Failure(diagnostic);
        }

        var attributeData = context.Attributes.First();

        var attributeSettings = GetAttributeSettings(attributeData, defaults);

        var existingJsonDerivedTypes = namedType.GetAttributes()
            .Where(data => data.AttributeClass?.ToDisplayString() == JsonDerivedTypeAttribute)
            .Select(data =>
            {
                if (data.ConstructorArguments is not [var typeArgument, ..])
                {
                    return null;
                }

                if (typeArgument is not { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol typeSymbol })
                {
                    return null;
                }

                return typeSymbol;
            })
            .OfType<INamedTypeSymbol>()
            .ToArray();

        var explicitConstructor = namedType.InstanceConstructors
            .FirstOrDefault(constructor => !constructor.IsImplicitlyDeclared);
        
        if (attributeSettings.GeneratePrivateConstructor && explicitConstructor is { })
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.ConstructorAlreadyExists,
                explicitConstructor.Locations.First(),
                namedType.Name
            );

            return Failure(diagnostic);
        }

        var members = namedType.GetTypeMembers()
            .Where(typeMember => SymbolEqualityComparer.Default.Equals(typeMember.BaseType, unionSymbol))
            .Select(unionMember => GetUnionMember(unionMember, existingJsonDerivedTypes))
            .ToImmutableArray();

        if (members is [])
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.MissingDerivedTypes,
                declaration.Identifier.GetLocation(),
                unionSymbol.Name
            );

            return Failure(diagnostic);
        }

        return new(
            Name: unionName,
            Type: GetUnionType(namedType),
            Namespace: unionSymbol.ContainingNamespace.ToDisplayString(),
            AttributeSettings: attributeSettings,
            Members: members,
            AttributeLocation: attributeData.ApplicationSyntaxReference!
                .GetSyntax()
                .GetLocation()
        );
    }

    private static UnionMember GetUnionMember(INamedTypeSymbol member, IEnumerable<INamedTypeSymbol> existingJsonDerivedTypes)
    {
        var unionMember = new UnionMember(
            Name: GetTypeName(member),
            ShouldGenerateSerializerAttribute: 
                !existingJsonDerivedTypes.Any(derivedType => SymbolEqualityComparer.Default.Equals(member, derivedType)),
            MatchableProperties: [.. GetMatchableProperties(member)]
        );

        return unionMember;
    }

    private static IEnumerable<MatchableProperty> GetMatchableProperties(INamedTypeSymbol member) => member.GetMembers()
        .OfType<IPropertySymbol>()
        .Where(property => property.GetMethod?.DeclaredAccessibility is Accessibility.Public)
        .Select(property => new MatchableProperty(
            Name: property.Name,
            Type: GetTypeName(property.Type)
        ));

    private static UnionType GetUnionType(INamedTypeSymbol type) => type.IsRecord switch
    {
        true => UnionType.Record,
        false => UnionType.Class
    };

    private static TypeName GetTypeName(ITypeSymbol type)
    {
        var plainFormat = new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.None,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
        );

        var qualifiedFormat = new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes
        );

        return new(
            type.ToDisplayString(plainFormat),
            type.ToDisplayString(qualifiedFormat)
        );
    }

    private static AttributeSettings GetAttributeSettings(AttributeData attribute, AttributeSettings defaults)
    {
        var unionArguments = attribute.NamedArguments
            .ToImmutableDictionary(
                argument => argument.Key, 
                argument => (TypedConstant?)argument.Value
            );

        return new(
            MatchOn: (MatchUnionOn)(unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.MatchOn))
                ?.Value
                ?? defaults.MatchOn),
            GeneratePolymorphicSerialization: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization))
                ?.Value as bool?
                ?? defaults.GeneratePolymorphicSerialization,
            GeneratePrivateConstructor: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor))
                ?.Value as bool?
                ?? defaults.GeneratePrivateConstructor
        );
    }

    private static AttributeSettings GetDefaultAttributeSettings(AnalyzerConfigOptions options)
    {
        options.TryGetValue("build_property.FunctionalEngine_Defaults_MatchOn", out string? match);
        options.TryGetValue("build_property.FunctionalEngine_Defaults_GeneratePolymorphicSerialization", out string? polymorphicSerialization);
        options.TryGetValue("build_property.FunctionalEngine_Defaults_GeneratePrivateConstructor", out string? privateConstructor);

        return new AttributeSettings(
            MatchOn: TryParseEnum<MatchUnionOn>(match) ?? DiscriminatedUnionDefaults.MatchOn,
            GeneratePolymorphicSerialization: TryParseBool(polymorphicSerialization) ?? DiscriminatedUnionDefaults.GeneratePolymorphicSerialization,
            GeneratePrivateConstructor: TryParseBool(privateConstructor) ?? DiscriminatedUnionDefaults.GeneratePrivateConstructor
        );

        static bool? TryParseBool(string? text)
        {
            if (text is null or "")
            {
                return null;
            }

            if (!bool.TryParse(text, out bool value))
            {
                return null;
            }

            return value;
        }

        static T? TryParseEnum<T>(string? text) where T : struct, Enum
        {
            if (text is null or "")
            {
                return null;
            }

            if (!Enum.TryParse<T>(text, out T value))
            {
                return null;
            }

            return value;
        }
    }

    private static UnionDefinition Failure(Diagnostic diagnostic) => new() { Failure = diagnostic };

    private readonly record struct CompilationInfo(
        bool SerializationAttributesExist
    );

    private readonly record struct ParseInfo(
        LanguageVersion LanguageVersion,
        bool SwitchExpressionsSupported
    );

    private readonly record struct UnionDefinition(
        TypeName Name,
        UnionType Type,
        string Namespace,
        AttributeSettings AttributeSettings,
        EquatableArray<UnionMember> Members,
        Location AttributeLocation
    )
    {
        public Diagnostic? Failure { get; init; }
    }

    private readonly record struct UnionMember(
        TypeName Name,
        bool ShouldGenerateSerializerAttribute,
        ImmutableArray<MatchableProperty> MatchableProperties
    );

    private enum UnionType
    {
        Class,
        Record
    }

    private readonly record struct AttributeSettings(
        MatchUnionOn MatchOn,
        bool GeneratePolymorphicSerialization,
        bool GeneratePrivateConstructor
    );

    private readonly record struct MatchableProperty(
        string Name,
        TypeName Type
    );

    private readonly record struct TypeName(
        string Plain,
        string WithGenerics
    );
}
