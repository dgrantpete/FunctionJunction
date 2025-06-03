using FunctionalEngine.Generator.Implementations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
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

                    namedType.GetTypeMembers()
                        .Where(typeMember => SymbolEqualityComparer.Default.Equals(typeMember.BaseType, unionSymbol))


                    var unionAttribute = namedType.GetAttributes();

                    throw new InvalidOperationException(
                        $"'{declaration}' must be either a {typeof(RecordDeclarationSyntax)} or {typeof(ClassDeclarationSyntax)}."
                    );
                });
    }

    private readonly record struct UnionDefinition(
        string Name, 
        UnionType Type, 
        Accessibility Accessibility,
        string Namespace,
        EquatableArray<UnionMember> Members
    );

    private readonly record struct UnionMember(
        string Name,
        Accessibility Accessibility
    );

    private enum UnionType
    {
        Class,
        Struct
    }

    private enum Accessibility
    {
        Internal,
        Public
    }
}
