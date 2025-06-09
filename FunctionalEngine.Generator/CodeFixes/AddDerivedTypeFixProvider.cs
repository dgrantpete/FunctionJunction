using FunctionalEngine.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalEngine.Generator.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddDerivedTypeFixProvider)), Shared]
internal class AddDerivedTypeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        Diagnostics.MissingDerivedTypes.Id
    ];

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public async override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken))!;

        var diagnostic = context.Diagnostics.First();

        var declaration = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!
            .FirstAncestorOrSelf<TypeDeclarationSyntax>()!;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add a derived type",
                cancellationToken => AddDerivedType(context.Document, declaration, cancellationToken),
                "AddDerivedType"
            ),
            diagnostic
        );
    }

    private static async Task<Document> AddDerivedType(
        Document document, 
        TypeDeclarationSyntax declaration,
        CancellationToken cancellationToken = default
    )
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

        var typeSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

        var format = new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        );

        var unionName = typeSymbol.ToDisplayString(format);

        var derivedName = $"{declaration.Identifier.Text}Variant";

        var memberDeclarationText = declaration.Kind() switch
        {
            SyntaxKind.RecordDeclaration => $"public record {derivedName} : {unionName};",
            _ => $"public class {derivedName} : {unionName} {{ }}"
        };

        var derivedDeclaration = SyntaxFactory.ParseMemberDeclaration(memberDeclarationText)!
                .WithAdditionalAnnotations(Formatter.Annotation);

        editor.AddMember(declaration, derivedDeclaration);

        return editor.GetChangedDocument();
    }
}
