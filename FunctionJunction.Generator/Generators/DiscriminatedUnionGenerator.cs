using FunctionJunction.Generator.Internal.Attributes;
using FunctionJunction.Generator.Internal.Helpers;
using FunctionJunction.Generator.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Collections.Immutable;
using System.Text;
using Accessibility = FunctionJunction.Generator.Internal.Models.Accessibility;
using static FunctionJunction.Generator.Internal.Helpers.TypeName;

namespace FunctionJunction.Generator.Generators;

[Generator("C#")]
internal class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    #region Template and Initialization

    private static readonly Lazy<Template> template = new(() =>
    {
        var assembly = typeof(DiscriminatedUnionGenerator).Assembly;

        var resourceName = assembly.GetManifestResourceNames()
            .Single(resource => resource.EndsWith(DiscriminatedUnion.TemplateName));

        using var templateStream = assembly.GetManifestResourceStream(resourceName);

        using var reader = new StreamReader(templateStream);

        var templateText = reader.ReadToEnd();

        return Template.Parse(templateText);
    });

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var projectAttributeDefaultsProvider = context.AnalyzerConfigOptionsProvider
            .Select((options, cancellationToken) =>
                UnionAttributeInfo.GetDefaults(options.GlobalOptions, cancellationToken)
            );

        var constantSymbolsProvider = context.CompilationProvider
            .Select(GetConstantSymbols);

        var unionInfoProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                DiscriminatedUnion.AttributeName,
                (syntaxNode, _) => syntaxNode is RecordDeclarationSyntax or ClassDeclarationSyntax,
                (context, _) => context
            )
            .Combine(constantSymbolsProvider)
            .Select((value, cancellationToken) =>
            {
                var (context, constantSymbols) = value;

                return GetUnionInfo(context, constantSymbols, cancellationToken);
            })
            .WhereNotNull();

        var renderModelProvider = unionInfoProvider.Combine(projectAttributeDefaultsProvider)
            .Combine(constantSymbolsProvider)
            .Combine(context.CompilationProvider)
            .Select((value, cancellationToken) =>
            {
                var (((unionInfo, constantSymbols), projectAttributeDefaults), compilation) = value;

                return CreateUnionModel(unionInfo, compilation, constantSymbols, projectAttributeDefaults, cancellationToken);
            })
            .WhereNotNull();

        context.RegisterSourceOutput(renderModelProvider, static (context, renderModel) =>
        {
            var generatedCode = template.Value.Render(renderModel);

            context.AddSource($"{renderModel.Name}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        });
    }

    #endregion

    #region Data Extraction

    private static UnionInfo? GetUnionInfo(
        GeneratorAttributeSyntaxContext context,
        ConstantSymbols constantSymbols,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var modifiers = context.TargetNode switch
        {
            RecordDeclarationSyntax recordSyntax => recordSyntax.Modifiers,
            ClassDeclarationSyntax classDeclaration => classDeclaration.Modifiers,
            _ => throw new InvalidOperationException("Syntax node was not a record or class declaration")
        };

        if (!modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return null;
        }

        if (
            context.TargetSymbol is not INamedTypeSymbol unionSymbol
                || unionSymbol.GetObjectType() is not { } objectType
                || context.Attributes is not [var attribute]
                || unionSymbol.GetAccessibility() is not { } accessibility
                || unionSymbol.ContainingType is not null
        )
        {
            return null;
        }

        var jsonMemberSymbols = unionSymbol.GetAttributes()
            .Where(attribute =>
                SymbolEqualityComparer.Default.Equals(
                    attribute.AttributeClass,
                    constantSymbols.JsonDerivedTypeAttribute
                )
            )
            .Select(attribute => attribute.ConstructorArguments switch
            {
                [{ Value: ITypeSymbol jsonMemberSymbol }, ..] => jsonMemberSymbol,
                _ => null
            })
            .OfType<ITypeSymbol>()
            .ToArray();

        var attributeInfo = UnionAttributeInfo.FromAttributeData(attribute, cancellationToken);

        var memberInfos = unionSymbol.GetDerivedTypeSymbols()
            .Select(GetMemberInfo)
            .OfType<MemberInfo>()
            .ToImmutableArray();

        MemberInfo? GetMemberInfo(INamedTypeSymbol memberSymbol)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!SymbolEqualityComparer.Default.Equals(memberSymbol.BaseType, unionSymbol))
            {
                return null;
            }

            if (memberSymbol.GetAccessibility() is not { } accessibility)
            {
                return null;
            }

            return new(
                memberSymbol.Name,
                accessibility,
                SymbolId.Create(memberSymbol),
                jsonMemberSymbols.Contains(memberSymbol, SymbolEqualityComparer.Default)
            )
            {
                DeconstructInfo = GetDeconstructInfo(memberSymbol)
            };
        }

        var hasUserDefinedConstructor = unionSymbol.Constructors
            .Any(constructor => !constructor.IsImplicitlyDeclared);

        return new(
            unionSymbol.Name,
            accessibility,
            SymbolId.Create(unionSymbol),
            unionSymbol.ContainingNamespace.ToDisplayString(),
            objectType,
            hasUserDefinedConstructor,
            memberInfos,
            attributeInfo
        );
    }

    private static DeconstructInfo? GetDeconstructInfo(INamedTypeSymbol memberSymbol, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var maybeDeconstructSymbol = memberSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(methodSymbol =>
                methodSymbol is { ReturnsVoid: true, Name: "Deconstruct", IsGenericMethod: false }
            );

        if (maybeDeconstructSymbol is null || memberSymbol.GetAccessibility() is not { } accessibility)
        {
            return null;
        }

        var parameters = maybeDeconstructSymbol.Parameters
            .Select(parameter =>
                new ParameterInfo(parameter.Name, SymbolId.Create(parameter))
            )
            .ToImmutableArray();

        return new(parameters, accessibility);
    }

    #endregion

    #region Helper Methods

    private static ConstantSymbols GetConstantSymbols(Compilation compilation, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (compilation.GetTypeByMetadataName(DiscriminatedUnion.AttributeName) is not { } unionAttribute)
        {
            throw new InvalidOperationException($"The symbol for {nameof(DiscriminatedUnionAttribute)} could not be loaded.");
        }

        var jsonDerivedTypeAttribute = compilation
            .GetTypeByMetadataName(JsonDerivedTypeAttribute);

        return new(unionAttribute)
        {
            JsonDerivedTypeAttribute = jsonDerivedTypeAttribute
        };
    }

    #endregion

    #region Model Creation

    private static UnionRenderModel? CreateUnionModel(
        UnionInfo unionInfo,
        Compilation compilation,
        UnionAttributeInfo projectAttributeDefaults,
        ConstantSymbols constantSymbols,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attributeSettings = unionInfo.DiscriminatedUnionAttributeInfo.Or(projectAttributeDefaults)
            .ToSettings();

        var context = new RenderContext(compilation, constantSymbols, attributeSettings);

        return CreateUnionModel(unionInfo, context, cancellationToken);
    }

    private static UnionRenderModel? CreateUnionModel(
        UnionInfo unionInfo,
        RenderContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var unionSymbol = unionInfo.Type.Resolve(context.Compilation);

        var memberContexts = unionInfo.MemberInfos
            .Select(CreateMemberContext)
            .ToImmutableArray();

        MemberRenderContext CreateMemberContext(MemberInfo memberInfo)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var memberContext = new MemberRenderContext(
                memberInfo.Name,
                memberInfo.Accessibility,
                memberInfo.Type.RenderType(context.Compilation, DisplayFormat.Qualified),
                memberInfo.HasDerivedTypeAttribute
            )
            {
                DeconstructInfo = memberInfo.DeconstructInfo
            };

            return memberContext;
        }

        return new(
            unionInfo.Name,
            unionInfo.Accessibility,
            memberContexts.Select(memberContext => memberContext.Accessibility)
                .DefaultIfEmpty(Accessibility.Public)
                .Min(),
            unionSymbol.ToDisplayString(DisplayFormat.Unqualified),
            unionInfo.Namespace,
            unionInfo.ObjectType,
            context.Settings.GeneratePrivateConstructor && !unionInfo.HasParameterlessConstructor
        )
        {
            MatchModel = CreateMatchModel(memberContexts, context, cancellationToken),
            PolymorphicAttributes = CreatePolymorphicAttributes(memberContexts, unionSymbol, context, cancellationToken)
                .ToImmutableArray()
        };
    }

    private static ImmutableArray<MatchRenderModel> CreateMatchModel(
        IEnumerable<MemberRenderContext> memberContexts,
        RenderContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var matchOn = context.Settings.MatchOn;

        if (context.Compilation is not CSharpCompilation { LanguageVersion: >= LanguageVersion.CSharp8 })
        {
            return [];
        }

        return matchOn switch
        {
            MatchUnionOn.Deconstruct => CreateDeconstructMatchModel(memberContexts, context, cancellationToken),
            MatchUnionOn.Type => CreateTypeMatchModel(memberContexts, cancellationToken),
            _ => []
        };
    }

    private static ImmutableArray<MatchRenderModel> CreateTypeMatchModel(
        IEnumerable<MemberRenderContext> memberContexts,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return [.. memberContexts.Select(CreateMatchModel)];

        MatchRenderModel CreateMatchModel(MemberRenderContext memberContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new(
                memberContext.Name,
                RenderParameter(memberContext),
                RenderArm(memberContext)
            );
        }

        string RenderParameter(MemberRenderContext memberContext)
        {
            var parameterName = $"on{memberContext.Name}";

            var funcType = $"{Func}<{memberContext.Type}, TResult>";

            return $"{funcType} {parameterName}";
        }

        string RenderArm(MemberRenderContext memberContext)
        {
            var typeName = memberContext.Name.ToCamelCase();

            var pattern = $"{memberContext.Type} {typeName} => on{memberContext.Name}({typeName})";

            return pattern;
        }
    }

    private static ImmutableArray<MatchRenderModel> CreateDeconstructMatchModel(
        IEnumerable<MemberRenderContext> memberContexts,
        RenderContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return [.. memberContexts.Select(CreateMatchModel)];

        MatchRenderModel CreateMatchModel(MemberRenderContext memberContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deconstructInfo = memberContext.DeconstructInfo
                ?? new([], memberContext.Accessibility);

            return new(
                memberContext.Name,
                RenderParameter(memberContext, deconstructInfo),
                RenderArm(memberContext, deconstructInfo)
            );
        }

        string RenderParameter(MemberRenderContext memberContext, DeconstructInfo deconstructInfo)
        {
            var renderedTypes = deconstructInfo.Parameters
                .Select(parameter => parameter.Type.Resolve(context.Compilation))
                .Select(parameterSymbol => parameterSymbol.Type.ToDisplayString(DisplayFormat.Qualified));

            var parameterName = $"on{memberContext.Name}";

            var funcType = $"{Func}<{string.Join(", ", [.. renderedTypes, "TResult"])}>";

            return $"{funcType} {parameterName}";
        }

        string RenderArm(MemberRenderContext memberContext, DeconstructInfo deconstructInfo)
        {
            var parameterNames = deconstructInfo.Parameters.Select(parameter => parameter.Name.ToCamelCase());

            var invocationParameters = string.Join(
                ", ",
                parameterNames
            );

            var patternParameters = string.Join(", ", parameterNames.Select(name => $"var {name}"));

            var pattern = invocationParameters switch
            {
                "" => memberContext.Type,
                _ => $"{memberContext.Type}({patternParameters})"
            };

            return $"{pattern} => on{memberContext.Name}({invocationParameters})";
        }
    }

    private static IEnumerable<string> CreatePolymorphicAttributes(
        IEnumerable<MemberRenderContext> memberContexts,
        INamedTypeSymbol unionSymbol,
        RenderContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (
            !context.Settings.GeneratePolymorphicSerialization
                || context.Symbols.JsonDerivedTypeAttribute is null 
                || unionSymbol.IsGenericType
        )
        {
            return [];
        }

        return memberContexts.Where(memberContext => !memberContext.HasDerivedTypeAttribute)
            .Select(memberContext => $"[{JsonDerivedTypeAttribute}(typeof({memberContext.Type}), \"{memberContext.Name}\")]");
    }

    #endregion

    #region Code Rendering

    private static class DisplayFormat
    {
        public static SymbolDisplayFormat Qualified { get; } =
            new(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included
            );

        public static SymbolDisplayFormat Unqualified { get; } =
            new(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            );
    }

    #endregion

    #region Data Models

    private readonly record struct UnionInfo(
        string Name,
        Accessibility Accessibility,
        SymbolId<INamedTypeSymbol> Type,
        string Namespace,
        ObjectType ObjectType,
        bool HasParameterlessConstructor,
        EquatableArray<MemberInfo> MemberInfos,
        UnionAttributeInfo DiscriminatedUnionAttributeInfo
    );

    private readonly record struct MemberInfo(
        string Name,
        Accessibility Accessibility,
        SymbolId<INamedTypeSymbol> Type,
        bool HasDerivedTypeAttribute
    )
    {
        public DeconstructInfo? DeconstructInfo { get; init; }
    }

    private readonly record struct DeconstructInfo(
        EquatableArray<ParameterInfo> Parameters,
        Accessibility Accessibility
    );

    private readonly record struct ParameterInfo(
        string Name,
        SymbolId<IParameterSymbol> Type
    );

    private readonly record struct RenderContext(
        Compilation Compilation,
        ConstantSymbols Symbols,
        UnionSettings Settings
    );

    private readonly record struct MemberRenderContext(
        string Name,
        Accessibility Accessibility,
        string Type,
        bool HasDerivedTypeAttribute
    )
    {
        public DeconstructInfo? DeconstructInfo { get; init; }
    }

    private readonly record struct UnionRenderModel(
        string Name,
        Accessibility Accessibility,
        Accessibility MinimumMemberAccessibility,
        string Type,
        string Namespace,
        ObjectType ObjectType,
        bool GeneratePrivateConstructor
    )
    {
        public EquatableArray<MatchRenderModel> MatchModel { get; init; } = [];

        public EquatableArray<string> PolymorphicAttributes { get; init; } = [];
    }

    private readonly record struct MatchRenderModel(
        string MemberName,
        string Parameter,
        string MatchArm
    );

    private readonly record struct ConstantSymbols(INamedTypeSymbol UnionAttribute)
    {
        public INamedTypeSymbol? JsonDerivedTypeAttribute { get; init; }
    }

    #endregion
}
