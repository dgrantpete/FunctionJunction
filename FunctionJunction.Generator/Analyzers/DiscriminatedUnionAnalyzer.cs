using FunctionJunction.Generator.Internal.Attributes;
using FunctionJunction.Generator.Internal.Helpers;
using FunctionJunction.Generator.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class DiscriminatedUnionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [..
        DiagnosticHelper.IterateDiagnostics()
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (GetCompilationContext(compilationStartContext) is not { } compilationContext)
            {
                return;
            }

            compilationStartContext.RegisterSymbolAction(
                analysisContext =>
                {
                    if (GetUnionContext(analysisContext, compilationContext) is not { } unionContext)
                    {
                        return;
                    }

                    AnalyzeUnionDefinition(unionContext);
                    AnalyzeJsonPolymorphic(unionContext);
                    AnalyzeUnionMembers(unionContext);
                },
                SymbolKind.NamedType
            );
        });
    }

    private static UnionAttributeInfo? TryGetAttributeInfo(INamedTypeSymbol unionSymbol, CompilationContext compilation)
    {
        var maybeAttributeData = unionSymbol.GetAttributes()
            .FirstOrDefault(attribute =>
                SymbolEquals(attribute.AttributeClass, compilation.Constants.DiscriminatedUnionAttribute)
            );

        return maybeAttributeData switch
        {
            null => null,
            { } attributeData => UnionAttributeInfo.FromAttributeData(attributeData)
        };
    }

    private static void AnalyzeUnionDefinition(UnionContext union)
    {
        if (union.Symbol.GetObjectType() is null)
        {
            var notValidObjectDiagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.ObjectKindInvalid,
                union.Symbol.Locations,
                union.Symbol.Name
            );

            union.Analysis.ReportDiagnostic(notValidObjectDiagnostic);
        }

        if (union.Symbol.GetAccessibility() is null)
        {
            var accessibilityDiagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.DerivedTypeCanBeSealed,
                union.Symbol.Locations,
                union.Symbol.Name
            );

            union.Analysis.ReportDiagnostic(accessibilityDiagnostic);
        }

        var nonPartialSyntax = union.Symbol.DeclaringSyntaxReferences
            .Select(syntaxReference => syntaxReference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Where(syntax => !syntax.Modifiers.Any(SyntaxKind.PartialKeyword));

        foreach (var needsPartialSyntax in nonPartialSyntax)
        {
            var partialMissingDiagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.NotMarkedPartial,
                [needsPartialSyntax.Identifier.GetLocation()],
                needsPartialSyntax.Identifier.Text
            );

            union.Analysis.ReportDiagnostic(partialMissingDiagnostic);
        }

        if (union.Settings.MatchOn is not MatchUnionOn.None && union.Compilation.LanguageVersion < LanguageVersion.CSharp8)
        {
            var languageVersionDiagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.SwitchExpressionsNotSupported,
                union.Symbol.Locations,
                union.Compilation.LanguageVersion.ToDisplayString()
            );

            union.Analysis.ReportDiagnostic(languageVersionDiagnostic);
        }
    }

    private static void AnalyzeJsonPolymorphic(UnionContext union)
    {
        if (union.UserSettings.GeneratePolymorphicSerialization is not true)
        {
            return;
        }

        if (union.Compilation.Constants.JsonDerivedTypeAttribute is null)
        {
            var notFoundDiagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.DerivedTypeAttributeNotFound,
                union.Symbol.Locations,
                union.Symbol.Name
            );

            union.Analysis.ReportDiagnostic(notFoundDiagnostic);
        }

        if (union.Symbol.IsGenericType)
        {
            var genericDiagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.GenericsIncompatibleWithSerialization,
                union.Symbol.Locations,
                union.Symbol.Name
            );

            union.Analysis.ReportDiagnostic(genericDiagnostic);
        }
    }

    private static void AnalyzeUnionMembers(UnionContext union)
    {
        var memberSymbols = union.Symbol.GetTypeMembers()
            .Where(member => SymbolEquals(member.BaseType, union.Symbol))
            .ToImmutableArray();

        if (memberSymbols is [])
        {
            var diagnostic = DiagnosticHelper.Create(
                DiagnosticHelper.MissingDerivedTypes,
                union.Symbol.Locations,
                union.Symbol.Name
            );

            union.Analysis.ReportDiagnostic(diagnostic);
        }

        foreach (var memberSymbol in memberSymbols)
        {
            if (memberSymbol.GetAccessibility() is null)
            {
                var accessibilityDiagnostic = DiagnosticHelper.Create(
                    DiagnosticHelper.DerivedTypeAccessibilityInvalid,
                    memberSymbol.Locations,
                    memberSymbol.Name
                );

                union.Analysis.ReportDiagnostic(accessibilityDiagnostic);
            }

            var unsealedMembers = memberSymbol.DeclaringSyntaxReferences
                .Select(syntaxReference => syntaxReference.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .Where(syntax => !syntax.Modifiers.Any(SyntaxKind.SealedKeyword));

            foreach (var unsealedMember in unsealedMembers)
            {
                var unsealedMemberDiagnostic = DiagnosticHelper.Create(
                    DiagnosticHelper.DerivedTypeCanBeSealed,
                    [unsealedMember.Identifier.GetLocation()],
                    unsealedMember.Identifier.Text
                );

                union.Analysis.ReportDiagnostic(unsealedMemberDiagnostic);
            }
        }
    }

#pragma warning disable RS1012
    private static CompilationContext? GetCompilationContext(CompilationStartAnalysisContext startAnalysisContext) => 
        startAnalysisContext.Compilation switch
        {
            CSharpCompilation cSharpCompilation => new(
                GetConstantSymbols(cSharpCompilation),
                cSharpCompilation.LanguageVersion,
                UnionAttributeInfo.GetDefaults(startAnalysisContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions)
            ),
            _ => null
        };
#pragma warning restore RS1012

    private static ConstantSymbols GetConstantSymbols(Compilation compilation)
    {
        var unionAttribute = compilation.GetTypeByMetadataName(DiscriminatedUnion.AttributeName)
            ?? throw new TypeLoadException($"Could not find the unionSymbol for {DiscriminatedUnion.AttributeName}");

        var jsonDerivedTypeAttribute = compilation.GetTypeByMetadataName(TypeName.JsonDerivedTypeAttribute);

        return new(unionAttribute, jsonDerivedTypeAttribute);
    }

    private static UnionContext? GetUnionContext(SymbolAnalysisContext analysisContext, CompilationContext compilation)
    {
        var unionSymbol = (INamedTypeSymbol)analysisContext.Symbol;

        if (TryGetAttributeInfo(unionSymbol, compilation) is not { } attributeInfo)
        {
            return null;
        }

        var userSettings = attributeInfo.Or(compilation.DefaultSettings);
        var settings = userSettings.ToSettings();

        var unionContext = new UnionContext(
            unionSymbol,
            settings,
            userSettings,
            analysisContext,
            compilation
        );

        return unionContext;
    }

    private static bool SymbolEquals(ISymbol? first, ISymbol? second) => SymbolEqualityComparer.Default.Equals(first, second);

    private readonly record struct ConstantSymbols(
        INamedTypeSymbol DiscriminatedUnionAttribute,
        INamedTypeSymbol? JsonDerivedTypeAttribute
    );

    private readonly record struct UnionContext(
        INamedTypeSymbol Symbol,
        UnionSettings Settings,
        UnionAttributeInfo UserSettings,
        SymbolAnalysisContext Analysis,
        CompilationContext Compilation
    );

    private readonly record struct CompilationContext(
        ConstantSymbols Constants,
        LanguageVersion LanguageVersion,
        UnionAttributeInfo DefaultSettings
    );
}
