using FunctionJunction.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;

namespace FunctionJunction.Generator.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddPartialModifierFixProvider)), Shared]
internal class AddPartialModifierFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        Diagnostics.NotMarkedPartial.Id
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
                "Add partial modifier",
                cancellationToken => AddPartial(context.Document, declaration, cancellationToken),
                "AddPartialModifier"
            ),
            diagnostic
        );
    }

    private static async Task<Document> AddPartial(
        Document document,
        TypeDeclarationSyntax declaration,
        CancellationToken cancellationToken = default
    )
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        var generator = editor.Generator;

        var modifiers = generator.GetModifiers(declaration)
            .WithPartial(true);

        editor.SetModifiers(declaration, modifiers);

        return editor.GetChangedDocument();
    }
}
