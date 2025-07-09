using FunctionJunction.Generator.Internal.Attributes;
using FunctionJunction.Generator.Internal.Helpers;
using FunctionJunction.Generator.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using static FunctionJunction.Generator.Internal.Helpers.DiagnosticHelper;

namespace FunctionJunction.Generator.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal class DiscriminatedUnionFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [..
        IterateDiagnostics()
            .Select(diagnostic => diagnostic.Id)
    ];

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public async override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            ?? throw new InvalidOperationException("Could not get syntax root from document");

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticNode = root.FindNode(diagnostic.Location.SourceSpan);

            RegisterCodeFix(diagnostic, diagnosticNode, context);
        }
    }

    private static void RegisterCodeFix(
        Diagnostic diagnostic, 
        SyntaxNode diagnosticNode,
        CodeFixContext fixContext
    )
    {
        fixContext.CancellationToken.ThrowIfCancellationRequested();

        var document = fixContext.Document;
        
        if (diagnostic.Id == NotMarkedPartial.Id)
        {
            RegisterFix(
                "Add 'partial' modifier",
                cancellationToken => AddDeclarationModifiers(
                    DeclarationModifiers.Partial,
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        if (diagnostic.Id == DerivedTypeCanBeSealed.Id)
        {
            RegisterFix(
                "Add 'sealed' modifier",
                cancellationToken => AddDeclarationModifiers(
                    DeclarationModifiers.Sealed,
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        if (diagnostic.Id == MissingDerivedTypes.Id)
        {
            RegisterFix(
                "Add derived type",
                cancellationToken => AddDerivedType(
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        if (diagnostic.Id == DerivedTypeAttributeNotFound.Id || diagnostic.Id == GenericsIncompatibleWithSerialization.Id)
        {
            RegisterFix(
                $"Set '{nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization)}' to 'false'",
                cancellationToken => UpdateAttributeArguments(
                    new UnionAttributeInfo
                    {
                        GeneratePolymorphicSerialization = false
                    },
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        if (diagnostic.Id == SwitchExpressionsNotSupported.Id)
        {
            RegisterFix(
                $"Set '{nameof(DiscriminatedUnionAttribute.MatchOn)}' to 'None'",
                cancellationToken => UpdateAttributeArguments(
                    new UnionAttributeInfo
                    {
                        MatchOn = MatchUnionOn.None
                    },
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        if (diagnostic.Id == ConstructorAlreadyDefined.Id)
        {
            RegisterFix(
                $"Set '{nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor)}' to 'false'",
                cancellationToken => UpdateAttributeArguments(
                    new UnionAttributeInfo
                    {
                        GeneratePrivateConstructor = false
                    },
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );

            RegisterFix(
                $"Remove parameterless constructor",
                cancellationToken => RemoveConstructor(
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        if (diagnostic.Id == ConstructorNotPrivate.Id)
        {
            RegisterFix(
                "Make constructor 'private'",
                cancellationToken => UpdateAccessibility(
                    Microsoft.CodeAnalysis.Accessibility.Private,
                    diagnosticNode,
                    document,
                    cancellationToken
                )
            );
        }

        void RegisterFix(string title, Func<CancellationToken, Task<Document>> createChangedDocument) => 
            fixContext.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument,
                    CreateEquivalenceKey(title)
                ),
                diagnostic
            );
    }

    private static async Task<Document> AddDeclarationModifiers(
        DeclarationModifiers modifiers,
        SyntaxNode diagnosticNode,
        Document document,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (diagnosticNode.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } unionDeclaration)
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        var newModifiers = editor.Generator.GetModifiers(unionDeclaration) | modifiers;
        editor.SetModifiers(unionDeclaration, newModifiers);

        return editor.GetChangedDocument();
    }

    private static async Task<Document> AddDerivedType(
        SyntaxNode diagnosticNode,
        Document document,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (diagnosticNode.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } unionDeclaration)
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        var unionName = unionDeclaration.Identifier.ValueText;

        var unionTypeParameters = unionDeclaration.TypeParameterList
            ?.ToString()
            ?? string.Empty;

        var derivedTypeDeclarationText = unionDeclaration switch
        {
            RecordDeclarationSyntax => $"public sealed record {unionName}Variant : {unionName}{unionTypeParameters};",
            _ => $"public sealed class {unionName}Variant : {unionName}{unionTypeParameters} {{ }}"
        };

        if (SyntaxFactory.ParseMemberDeclaration(derivedTypeDeclarationText) is not { } derivedTypeDeclaration)
        {
            return document;
        }

        editor.AddMember(unionDeclaration, derivedTypeDeclaration.WithAdditionalAnnotations(Formatter.Annotation));

        return editor.GetChangedDocument();
    }

    private static async Task<Document> UpdateAttributeArguments(
        UnionAttributeInfo argumentUpdates,
        SyntaxNode diagnosticNode,
        Document document,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (
            diagnosticNode.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } unionDeclaration
                || await document.GetSemanticModelAsync(cancellationToken) is not { } semanticModel
                || semanticModel.Compilation.GetTypeByMetadataName(DiscriminatedUnion.AttributeName) is not { } attributeSymbol
                || semanticModel.GetDeclaredSymbol(unionDeclaration, cancellationToken) is not { } unionSymbol
        )
        {
            return document;
        }

        var attributeData = unionSymbol.GetAttributes()
            .Where(attributeData => SymbolHelper.SymbolEquals(attributeData.AttributeClass, attributeSymbol))
            .FirstOrDefault(attributeData =>
                attributeData.ApplicationSyntaxReference is { SyntaxTree: var syntaxTree, Span: var span }
                    && unionDeclaration.SyntaxTree == syntaxTree
                    && unionDeclaration.FullSpan.Contains(span)
            );

        if (attributeData.ApplicationSyntaxReference is not { Span: var attributeSpan })
        {
            return document;
        }

        var originalArguments = UnionAttributeInfo.FromAttributeData(attributeData, cancellationToken);
        var updatedArguments = argumentUpdates.Or(originalArguments);

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        var generator = editor.Generator;

        // We are getting the syntax via the editor rather than the 'SyntaxReference.GetSyntax'
        // since 'DocumentEditor' doesn't play nicely with references potentially outside its own syntax root
        var attributeSyntax = editor.OriginalRoot.FindNode(attributeSpan);

        var updatedAttributeSyntax = generator.Attribute(
            generator.GetName(attributeSyntax),
            updatedArguments.GenerateAttributeArguments(generator)
        );

        editor.ReplaceNode(attributeSyntax, updatedAttributeSyntax);

        return editor.GetChangedDocument();
    }

    private static async Task<Document> UpdateAccessibility(
        Microsoft.CodeAnalysis.Accessibility accessibility,
        SyntaxNode diagnosticNode,
        Document document,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var declaration = diagnosticNode.FirstAncestorOrSelf<MemberDeclarationSyntax>();

        if (declaration is null)
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        editor.SetAccessibility(declaration, accessibility);

        return editor.GetChangedDocument();
    }

    private static async Task<Document> RemoveConstructor(
        SyntaxNode diagnosticNode,
        Document document,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var constructorDeclaration = diagnosticNode
            .FirstAncestorOrSelf<ConstructorDeclarationSyntax>();

        var unionDeclaration = constructorDeclaration
            ?.FirstAncestorOrSelf<TypeDeclarationSyntax>();

        if (constructorDeclaration is null || unionDeclaration is null )
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        editor.RemoveNode(constructorDeclaration, SyntaxRemoveOptions.KeepNoTrivia);

        return editor.GetChangedDocument();
    }

    private static string CreateEquivalenceKey(string title) => string.Concat(
            title.Select(char? (c) =>
            {
                if (c is ' ')
                {
                    return '_';
                }

                if (char.IsLetter(c))
                {
                    return char.ToUpperInvariant(c);
                }

                return null;
            })
            .OfType<char>()
        );

}
