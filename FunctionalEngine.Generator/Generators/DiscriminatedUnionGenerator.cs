using FunctionalEngine.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace FunctionalEngine.Generator.Generators;

[Generator("C#")]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
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
            .ForAttributeWithMetadataName<UnionDefinition?>(
                DiscriminatedUnionDefaults.AttributeName,
                static (node, _) => node is RecordDeclarationSyntax or ClassDeclarationSyntax,
                static (context, cancellationToken) =>
                {
                    var declaration = (TypeDeclarationSyntax)context.TargetNode;

                    var semanticModel = context.SemanticModel;

                    var unionSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

                    if (unionSymbol is not INamedTypeSymbol namedType)
                    {
                        return null;
                    }

                    if (!declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        var diagnostic = Diagnostic.Create(
                            Diagnostics.NotMarkedPartial,
                            declaration.Identifier.GetLocation(),
                            namedType.Name
                        );

                        return Failure(diagnostic);
                    }

                    var attributeData = namedType.GetAttributes()
                        .SingleOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == DiscriminatedUnionDefaults.AttributeName)!;

                    var attributeSettings = GetAttributeSettings(attributeData);

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
                        .OfType<INamedTypeSymbol>();

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
                        .Select(unionMember => 
                            new UnionMember(
                                unionMember.Name, 
                                !existingJsonDerivedTypes.Any(derivedType => SymbolEqualityComparer.Default.Equals(
                                    unionMember,
                                    derivedType
                                ))
                            )
                        )
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
                        Name: namedType.Name,
                        Type: GetUnionType(namedType),
                        Namespace: unionSymbol.ContainingNamespace.ToDisplayString(),
                        AttributeSettings: GetAttributeSettings(attributeData),
                        Members: members,
                        AttributeLocation: attributeData.ApplicationSyntaxReference!
                            .GetSyntax()
                            .GetLocation()
                    );
                }
            )
            .Where(static definition => definition is not null)
            .Select(static (definition, _) => definition!.Value);

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

            if (definition.AttributeSettings.GenerateMatch && !parse.SwitchExpressionsSupported)
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.SwitchExpressionsNotSupported,
                    definition.AttributeLocation,
                    parse.LanguageVersion.ToDisplayString()
                );

                context.ReportDiagnostic(diagnostic);

                definition = definition with
                {
                    AttributeSettings = definition.AttributeSettings with { GenerateMatch = false }
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

            var generatedCode = template.Value.Render(definition);

            context.AddSource($"{definition.Name}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        });
    }

    private static UnionType GetUnionType(INamedTypeSymbol type) => type.IsRecord switch
    {
        true => UnionType.Record,
        false => UnionType.Class
    };

    private static AttributeSettings GetAttributeSettings(AttributeData attribute)
    {
        var unionArguments = attribute.NamedArguments
            .ToImmutableDictionary(
                argument => argument.Key, 
                argument => (TypedConstant?)argument.Value
            );

        return new(
            GenerateMatch: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GenerateMatch))
                ?.Value as bool?
                ?? DiscriminatedUnionDefaults.GenerateMatch,
            GeneratePolymorphicSerialization: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization))
                ?.Value as bool?
                ?? DiscriminatedUnionDefaults.GeneratePolymorphicSerialization,
            GeneratePrivateConstructor: unionArguments
                ?.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor))
                ?.Value as bool?
                ?? DiscriminatedUnionDefaults.GeneratePrivateConstructor
        );
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
        string Name,
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
        string Name,
        bool ShouldGenerateSerializerAttribute
    );

    private enum UnionType
    {
        Class,
        Record
    }

    private readonly record struct AttributeSettings(
        bool GenerateMatch,
        bool GeneratePolymorphicSerialization,
        bool GeneratePrivateConstructor
    );
}
