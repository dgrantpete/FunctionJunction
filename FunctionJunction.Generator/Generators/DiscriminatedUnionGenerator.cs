using FunctionJunction.Generator.Internal;
using FunctionJunction.Generator.Internal.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Collections.Immutable;
using System.Text;
using Accessibility = FunctionJunction.Generator.Internal.Accessibility;

namespace FunctionJunction.Generator.Generators;

[Generator("C#")]
internal class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    #region Template and Initialization

    private const string JsonDerivedTypeAttribute = "System.Text.Json.Serialization.JsonDerivedTypeAttribute";

    private const string Func = "System.Func";

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
        var projectAttributeDefaultsProvider = context.AnalyzerConfigOptionsProvider
            .Select((options, cancellationToken) => 
                GetDefaultAttributeInfo(options.GlobalOptions, cancellationToken)
            );

        var constantSymbolsProvider = context.CompilationProvider
            .Select(GetConstantSymbols);

        var unionInfoProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                DiscriminatedUnionDefaults.AttributeName,
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

        if (
            context.TargetSymbol is not INamedTypeSymbol unionSymbol
                || GetObjectType(unionSymbol) is not { } objectType
                || context.Attributes is not [var attribute]
                || unionSymbol.GetAccessibility() is not { } accessibility
        )
        {
            return null;
        }

        var attributeInfo = GetAttributeInfo(attribute, cancellationToken);

        var memberInfos = unionSymbol.GetTypeMembers()
            .Where(memberSymbol => SymbolEqualityComparer.Default.Equals(memberSymbol.ContainingType, unionSymbol))
            .Select(GetMemberInfo)
            .OfType<MemberInfo>()
            .ToImmutableArray();

        MemberInfo? GetMemberInfo(INamedTypeSymbol memberSymbol)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (memberSymbol.GetAccessibility() is not { } accessibility)
            {
                return null;
            }

            return new(
                memberSymbol.Name,
                accessibility,
                SymbolId.Create(memberSymbol),
                memberSymbol.GetAttributes()
                    .Select(attribute => attribute.AttributeClass)
                    .Contains(constantSymbols.JsonDerivedTypeAttribute, SymbolEqualityComparer.Default)
            )
            {
                DeconstructInfo = GetDeconstructInfo(memberSymbol)
            };
        }

        return new(
            unionSymbol.Name,
            accessibility,
            SymbolId.Create(unionSymbol),
            unionSymbol.ContainingNamespace.ToDisplayString(),
            objectType,
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

        if (maybeDeconstructSymbol is not { } deconstructSymbol || memberSymbol.GetAccessibility() is not { } accessibility)
        {
            return null;
        }

        var parameters = deconstructSymbol.Parameters
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

        if (compilation.GetTypeByMetadataName(DiscriminatedUnionDefaults.AttributeName) is not INamedTypeSymbol unionAttribute)
        {
            var types = compilation.GetTypesByMetadataName(DiscriminatedUnionDefaults.AttributeName);

            throw new InvalidOperationException($"The symbol for {nameof(DiscriminatedUnionAttribute)} could not be loaded.");
        }

        var jsonDerivedTypeAttribute = compilation
            .GetTypeByMetadataName(JsonDerivedTypeAttribute);

        return new(unionAttribute)
        {
            JsonDerivedTypeAttribute = jsonDerivedTypeAttribute
        };
    }

    private static AttributeInfo GetAttributeInfo(AttributeData attribute, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var unionArguments = attribute.NamedArguments
            .ToImmutableDictionary(
                argument => argument.Key, 
                TypedConstant? (argument) => argument.Value
            );

        return new()
        {
            MatchOn = (MatchUnionOn?)(unionArguments.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.MatchOn))
                ?.Value as int?),
            GeneratePolymorphicSerialization = unionArguments.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization))
                ?.Value as bool?,
            GeneratePrivateConstructor = unionArguments.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor))
                ?.Value as bool?
        };
    }

    private static ObjectType? GetObjectType(ITypeSymbol typeSymbol) => typeSymbol switch
    {
        { IsRecord: false, TypeKind: TypeKind.Class } => ObjectType.Class,
        { IsRecord: true, TypeKind: TypeKind.Class } => ObjectType.Record,
        _ => null
    };

    private static AttributeInfo GetDefaultAttributeInfo(AnalyzerConfigOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        options.TryGetValue(
            $"build_property.FunctionJunction_Defaults_{nameof(DiscriminatedUnionAttribute.MatchOn)}", 
            out string? match
        );

        options.TryGetValue(
            $"build_property.FunctionJunction_Defaults_{nameof(DiscriminatedUnionAttribute.GeneratePolymorphicSerialization)}", 
            out string? polymorphicSerialization
        );

        options.TryGetValue(
            $"build_property.FunctionJunction_Defaults_{nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor)}", 
            out string? privateConstructor
        );

        return new()
        {
            MatchOn = TryParseEnum<MatchUnionOn>(match),
            GeneratePolymorphicSerialization = TryParseBool(polymorphicSerialization),
            GeneratePrivateConstructor = TryParseBool(privateConstructor)
        };

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

    #endregion

    #region Model Creation

    private static UnionRenderModel? CreateUnionModel(
        UnionInfo unionInfo,
        Compilation compilation,
        AttributeInfo projectAttributeDefaults,
        ConstantSymbols constantSymbols,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attributeSettings = unionInfo.AttributeInfo.Or(projectAttributeDefaults)
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
            unionInfo.Type.RenderType(context.Compilation, DisplayFormat.Unqualified),
            unionInfo.Namespace,
            unionInfo.ObjectType,
            context.Settings.GeneratePrivateConstructor
        )
        {
            MatchModel = CreateMatchModel(memberContexts, context, cancellationToken),
            PolymorphicAttributes = CreatePolymorphicAttributes(memberContexts, context, cancellationToken)
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

        if (matchOn is MatchUnionOn.Properties)
        {
            return CreatePropertiesMatchModel(memberContexts, context, cancellationToken);
        }

        if (matchOn is MatchUnionOn.Type)
        {
            return CreateTypeMatchModel(memberContexts, cancellationToken);
        }

        return [];
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

    private static ImmutableArray<MatchRenderModel> CreatePropertiesMatchModel(
        IEnumerable<MemberRenderContext> memberContexts,
        RenderContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return memberContexts.SelectAll(CreateMatchModel) ?? [];

        MatchRenderModel? CreateMatchModel(MemberRenderContext memberContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (memberContext.DeconstructInfo is not { } deconstructInfo)
            {
                return null;
            }

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

            var funcType = $"{Func}<{string.Join(", ", renderedTypes)}, TResult>";

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
        RenderContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (
            !context.Settings.GeneratePolymorphicSerialization 
                || context.Symbols.JsonDerivedTypeAttribute is not { }derivedTypeAttributeSymbol
        )
        {
            return [];
        }

        return memberContexts.Where(memberContext => !memberContext.HasDerivedTypeAttribute)
            .Select(memberContext => $"[{JsonDerivedTypeAttribute}(typeof({memberContext.Type}))]");
    }

    #endregion

    #region Code Rendering

    private static class DisplayFormat
    {
        public static SymbolDisplayFormat Qualified { get; } = 
            new(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
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
        EquatableArray<MemberInfo> MemberInfos,
        AttributeInfo AttributeInfo
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
        AttributeSettings Settings
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

    private readonly record struct AttributeSettings(
        MatchUnionOn MatchOn,
        bool GeneratePolymorphicSerialization,
        bool GeneratePrivateConstructor
    );

    private readonly record struct AttributeInfo
    {
        public MatchUnionOn? MatchOn { get; init; }

        public bool? GeneratePolymorphicSerialization { get; init; }

        public bool? GeneratePrivateConstructor { get; init; }

        public AttributeInfo Or(AttributeInfo other) =>
            this with
            {
                MatchOn = MatchOn ?? other.MatchOn,
                GeneratePolymorphicSerialization = GeneratePolymorphicSerialization ?? other.GeneratePolymorphicSerialization,
                GeneratePrivateConstructor = GeneratePrivateConstructor ?? other.GeneratePrivateConstructor
            };

        public AttributeSettings ToSettings() =>
            new(
                MatchOn ?? DiscriminatedUnionDefaults.Instance.MatchOn,
                GeneratePolymorphicSerialization ?? DiscriminatedUnionDefaults.Instance.GeneratePolymorphicSerialization,
                GeneratePrivateConstructor ?? DiscriminatedUnionDefaults.Instance.GeneratePrivateConstructor
            );
    }

    private readonly record struct SymbolId<TSymbol>(string Id, SymbolIdType Type) where TSymbol : class, ISymbol
    {
        public string? ForeignId { get; init; }

        public TSymbol Resolve(Compilation compilation)
        {
            if (
                ForeignId is { } foreignId 
                    && typeof(TSymbol) == typeof(IParameterSymbol)
                    && GetSymbolInternal<IMethodSymbol>(Id, Type, compilation) is { } methodSymbol
            )
            {
                return methodSymbol.Parameters.FirstOrDefault(parameter => parameter.Name == foreignId) as TSymbol
                    ?? throw new InvalidOperationException("Symbol could not be resolved");
            }

            return GetSymbolInternal<TSymbol>(Id, Type, compilation)
                ?? throw new InvalidOperationException("Symbol could not be resolved");
        }

        public string RenderType(Compilation compilation, SymbolDisplayFormat? format = null) =>
            Resolve(compilation).ToDisplayString(format);

        private static TInternalSymbol? GetSymbolInternal<TInternalSymbol>(
            string id,
            SymbolIdType type,
            Compilation compilation
        )
            where TInternalSymbol : class, ISymbol
        =>
            type switch
            {
                SymbolIdType.Declaration => DocumentationCommentId.GetFirstSymbolForDeclarationId(id, compilation) as TInternalSymbol,
                _ => DocumentationCommentId.GetFirstSymbolForReferenceId(id, compilation) as TInternalSymbol
            };
    }

    private static class SymbolId
    {
        public static SymbolId<TSymbol> Create<TSymbol>(TSymbol symbol) where TSymbol : class, ISymbol
        {
            var symbolId = DocumentationCommentId.CreateReferenceId(symbol);

            if (
                symbolId is "" 
                    && symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol methodSymbol } parameterSymbol
                    && DocumentationCommentId.CreateDeclarationId(methodSymbol) is { } methodSymbolId
            )
            {
                return new(methodSymbolId, SymbolIdType.Declaration)
                {
                    ForeignId = parameterSymbol.Name
                };
            }

            return new(symbolId, SymbolIdType.Reference);
        }
    }

    private enum ObjectType
    {
        Class,
        Record
    }

    private enum SymbolIdType
    {
        Reference,
        Declaration
    }

    #endregion
}
