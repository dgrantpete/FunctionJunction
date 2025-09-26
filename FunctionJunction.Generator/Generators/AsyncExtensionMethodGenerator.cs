using FunctionJunction.Generator.Internal.Attributes;
using FunctionJunction.Generator.Internal.Helpers;
using FunctionJunction.Generator.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using Scriban.Runtime;
using System.Collections.Immutable;
using Accessibility = FunctionJunction.Generator.Internal.Models.Accessibility;

namespace FunctionJunction.Generator.Generators;

[Generator("C#")]
internal class AsyncExtensionMethodGenerator : IIncrementalGenerator
{
    #region Template and Initialization

    private static readonly Lazy<Template> template = new(static () =>
    {
        var assembly = typeof(AsyncExtensionMethodGenerator).Assembly;

        var resourceName = assembly.GetManifestResourceNames()
            .Single(resource => resource.EndsWith(GenerateAsyncExtension.TemplateName));

        using var templateStream = assembly.GetManifestResourceStream(resourceName);

        using var reader = new StreamReader(templateStream);

        var templateText = reader.ReadToEnd();

        return Template.Parse(templateText);
    });

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodInfoProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateAsyncExtension.AttributeName,
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
                GetMethodInfo
            )
            .WhereNotNull();

        var renderModelProvider = methodInfoProvider.Collect()
            .SelectMany((value, cancellationToken) =>
                value.GroupBy(
                    methodInfo => CreateClassGroup(methodInfo, cancellationToken),
                    (methodInfo) => new MethodRenderContext(
                        CreateMethodModel(methodInfo, cancellationToken),
                        methodInfo.FilePath,
                        methodInfo.Namespace
                    ),
                    static (classGroup, methodContexts) =>
                    (
                        classGroup,
                        methodContexts.ToEquatableArray()
                    )
                )
            )
            .Combine(context.CompilationProvider)
            .Select(static (value, cancellationToken) =>
            {
                var ((classGroup, methodContexts), compilation) = value;

                return CreateClassModel(classGroup, methodContexts, compilation, cancellationToken);
            });

        context.RegisterSourceOutput(
            renderModelProvider,
            static (context, renderModel) =>
            {
                var generatedCode = GenerateCode(renderModel);

                context.AddSource($"{renderModel.Namespace}.{renderModel.ExtensionClassName}.g.cs", generatedCode);
            }
        );
    }

    #endregion

    #region Data Extraction

    private static MethodInfo? GetMethodInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TargetSymbol is not IMethodSymbol methodSymbol
                || methodSymbol.GetAccessibility() is not { } accessibility
                || GetMethodType(methodSymbol) is not { } methodType
                || methodSymbol.GetDocumentationCommentId() is not { } documentationReference
                || GetConstantSymbols(context.SemanticModel.Compilation) is not { } constantSymbols
                || GetClassInfo(methodSymbol.ContainingType, constantSymbols, cancellationToken) is not (var classInfo, var classAttributeInfo)
                || context.Attributes is not [var attribute]
        )
        {
            return null;
        }

        var methodAttributeInfo = GetAttributeInfo(attribute);

        var parameters = methodSymbol.Parameters.Select(GetParameterInfo)
            .ToImmutableArray();

        var generics = methodSymbol.TypeParameters
            .Select(GetGenericInfo)
            .ToImmutableArray();

        var returnType = GetReturnType(methodSymbol.ReturnType, constantSymbols, cancellationToken);

        return new(
            methodSymbol.Name,
            accessibility,
            methodSymbol.ContainingNamespace.ToDisplayString(),
            methodType,
            parameters,
            generics,
            returnType,
            methodAttributeInfo.Or(classAttributeInfo)
                .ToSettings(),
            documentationReference,
            context.TargetNode.SyntaxTree.FilePath,
            classInfo
        );
    }

    private static (ClassInfo ClassInfo, AttributeInfo AttributeInfo)? GetClassInfo(INamedTypeSymbol classSymbol, ConstantSymbols constantSymbols, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (classSymbol.GetAccessibility() is not { } accessibility)
        {
            return null;
        }

        var attribute = classSymbol.GetAttributes()
            .SingleOrDefault(attribute =>
                SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, constantSymbols.GeneratorAttribute)
            );

        var attributeInfo = GetAttributeInfo(attribute);

        var classInfo = new ClassInfo(
            classSymbol.Name,
            accessibility,
            GetFormattedType(classSymbol),
            classSymbol.TypeParameters
                .Select(GetGenericInfo)
                .ToImmutableArray()
            );

        return (classInfo, attributeInfo);
    }

    #endregion

    #region Helper Methods

    private static ConstantSymbols? GetConstantSymbols(Compilation compilation)
    {
        if (
            compilation.GetTypeByMetadataName(GenerateAsyncExtension.AttributeName) is not { } attributeSymbol
                || compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1") is not { } taskSymbol
                || compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") is not { } valueTaskSymbol
        )
        {
            return null;
        }

        return new(taskSymbol, valueTaskSymbol, attributeSymbol);
    }

    private static AttributeInfo GetAttributeInfo(AttributeData? attribute)
    {
        if (attribute is null)
        {
            return default;
        }

        var extensionClassName = (string?)attribute.NamedArguments
            .Select((string Name, TypedConstant Constant)? (argument) => (argument.Key, argument.Value))
            .SingleOrDefault(argument => argument?.Name is nameof(GenerateAsyncExtensionAttribute.ExtensionClassName))
            ?.Constant
            .Value;

        var extensionMethodName = (string?)attribute.NamedArguments
            .Select((string Name, TypedConstant Constant)? (argument) => (argument.Key, argument.Value))
            .SingleOrDefault(argument => argument?.Name is nameof(GenerateAsyncExtensionAttribute.ExtensionMethodName))
            ?.Constant
            .Value;

        var @namespace = (string?)attribute.NamedArguments
            .Select((string Name, TypedConstant Constant)? (argument) => (argument.Key, argument.Value))
            .SingleOrDefault(argument => argument?.Name is nameof(GenerateAsyncExtensionAttribute.Namespace))
            ?.Constant
            .Value;

        return new()
        {
            ExtensionClassName = extensionClassName,
            ExtensionMethodName = extensionMethodName,
            Namespace = @namespace
        };
    }

    private static string GetFormattedType(ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(format: new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        ));

    private static MethodType? GetMethodType(IMethodSymbol methodSymbol) => methodSymbol switch
    {
        { IsExtensionMethod: true } => MethodType.Extension,
        { IsStatic: false, MethodKind: MethodKind.Ordinary } => MethodType.Instance,
        _ => null
    };

    private static GenericInfo GetGenericInfo(ITypeParameterSymbol typeParameterSymbol) =>
        new(
            typeParameterSymbol.Name,
            GetParameterConstraints(typeParameterSymbol)
        );

    private static string? GetParameterConstraints(ITypeParameterSymbol typeParameterSymbol)
    {
        return GetConstraintComponents()
            .Aggregate(
                default(string?),
                (previousConstraints, constraint) => previousConstraints switch
                {
                    { } => $"{previousConstraints}, {constraint}",
                    null => $"where {typeParameterSymbol.Name} : {constraint}"
                }
            );

        IEnumerable<string> GetConstraintComponents()
        {
            if (typeParameterSymbol.HasReferenceTypeConstraint)
            {
                yield return "class";
            }

            if (typeParameterSymbol.HasValueTypeConstraint)
            {
                yield return "struct";
            }

            if (typeParameterSymbol.HasNotNullConstraint)
            {
                yield return "notnull";
            }

            if (typeParameterSymbol.HasUnmanagedTypeConstraint)
            {
                yield return "unmanaged";
            }

            foreach (var constraint in typeParameterSymbol.ConstraintTypes)
            {
                yield return constraint.ToDisplayString();
            }

            if (typeParameterSymbol.HasConstructorConstraint)
            {
                yield return "new()";
            }
        }
    }

    private static ParameterInfo GetParameterInfo(IParameterSymbol parameterSymbol) =>
        new(
            parameterSymbol.Name,
            GetFormattedType(parameterSymbol.Type)
        );

    private static ReturnTypeInfo GetReturnType(ITypeSymbol returnTypeSymbol, ConstantSymbols constantSymbols, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (
            returnTypeSymbol is INamedTypeSymbol namedTypeSymbol
                && (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, constantSymbols.Task)
                    || SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, constantSymbols.ValueTask)
                )
                && namedTypeSymbol.TypeArguments is [var innerTypeSymbol]
        )
        {
            return new(
                SyncType: GetFormattedType(innerTypeSymbol),
                ReturnsTask: true
            );
        }

        return new(
            SyncType: GetFormattedType(returnTypeSymbol),
            ReturnsTask: false
        );
    }

    #endregion

    #region Model Creation

    private static ClassGroup CreateClassGroup(MethodInfo methodInfo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classInfo = methodInfo.ClassInfo;
        var attributeSettings = methodInfo.AttributeSettings;

        var extensionClassName = FormatTemplatedName(classInfo.Name, attributeSettings.ExtensionClassName);
        var @namespace = FormatTemplatedName(methodInfo.Namespace, attributeSettings.Namespace);

        return new(
            extensionClassName,
            @namespace,
            classInfo.Accessibility
        );
    }

    private static MethodRenderModel CreateMethodModel(MethodInfo methodInfo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classInfo = methodInfo.ClassInfo;

        var generics = classInfo.Generics
            .Concat(methodInfo.Generics)
            .ToImmutableArray();

        var (extensionParameter, parameters) = CreateParameterModels(methodInfo);

        var name = FormatTemplatedName(methodInfo.Name, methodInfo.AttributeSettings.ExtensionMethodName);

        return new(
            name,
            methodInfo.Name,
            methodInfo.Accessibility,
            generics,
            extensionParameter,
            parameters,
            methodInfo.ReturnType.SyncType,
            methodInfo.ReturnType.ReturnsTask,
            methodInfo.DocumentationReference
        );
    }

    private static (ParameterRenderModel ExtensionParameter, ImmutableArray<ParameterRenderModel> Parameters) CreateParameterModels(MethodInfo methodInfo)
    {
        if (methodInfo.Type is MethodType.Extension)
        {
            var parameters = methodInfo.Parameters
                .Skip(1)
                .Select(CreateParameterModel);

            return (
                CreateParameterModel(methodInfo.Parameters[0]),
                [.. parameters]
            );
        }

        var extensionParameter = new ParameterRenderModel(
            methodInfo.ClassInfo.Name.ToCamelCase(),
            methodInfo.ClassInfo.Type
        );

        return (extensionParameter, [.. methodInfo.Parameters.Select(CreateParameterModel)]);
    }

    private static ParameterRenderModel CreateParameterModel(ParameterInfo parameterInfo) =>
        new(
            parameterInfo.Name,
            parameterInfo.Type
        );

    private static ClassRenderModel CreateClassModel(
        ClassGroup classGroup,
        EquatableArray<MethodRenderContext> methodContexts,
        Compilation compilation,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var includedFilePaths = methodContexts.Select(methodContext => methodContext.FilePath)
            .ToImmutableHashSet();

        var usingsForOriginalNamespaces = methodContexts.Select(methodContext => methodContext.OriginalNamespace)
            .Select(@namespace => $"using {@namespace};");

        var usings = compilation.SyntaxTrees
            .Where(syntaxTree => includedFilePaths.Contains(syntaxTree.FilePath))
            .SelectMany(syntaxTree =>
                syntaxTree.GetCompilationUnitRoot(cancellationToken)
                    .Usings
            )
            .Select(@using => @using.ToString())
            .Concat(usingsForOriginalNamespaces)
            .Where(@using => @using != classGroup.Namespace)
            .Distinct();

        return new(
            classGroup.ExtensionClassName,
            classGroup.Namespace,
            usings.ToImmutableArray(),
            classGroup.Accessibility,
            methodContexts.Select(methodContext => methodContext.Model)
                .ToEquatableArray()
        );
    }

    #endregion

    #region Code Rendering

    private static readonly string[] templateSplitStrings = ["{0}"];

    private static string GenerateCode(ClassRenderModel renderModel)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanHelpers));
        scriptObject.Import(renderModel);

        var templateContext = new TemplateContext();
        templateContext.PushGlobal(scriptObject);

        return template.Value.Render(templateContext);
    }

    private static string FormatTemplatedName(string originalName, string nameTemplate)
    {
        var templateParts = nameTemplate.Split(templateSplitStrings, StringSplitOptions.None)
            .AsSpan();

        if (templateParts is [var firstPart, ..] && originalName.StartsWith(firstPart))
        {
            templateParts[0] = string.Empty;
        }

        if (templateParts is [.., var lastPart] && originalName.EndsWith(lastPart))
        {
            templateParts[^1] = string.Empty;
        }

        return string.Join(originalName, templateParts.ToArray());
    }

    private static class ScribanHelpers
    {
        public static string ToCamelCase(string text) => text.ToCamelCase();

        public static string RenderGenerics(IEnumerable<GenericInfo> generics) =>
            $"<{string.Join(", ", generics.Select(generic => generic.Name))}>";
    }

    #endregion

    #region Data Models

    private readonly record struct ConstantSymbols(
        INamedTypeSymbol Task,
        INamedTypeSymbol ValueTask,
        INamedTypeSymbol GeneratorAttribute
    );

    private sealed record MethodInfo(
        string Name,
        Accessibility Accessibility,
        string Namespace,
        MethodType Type,
        EquatableArray<ParameterInfo> Parameters,
        EquatableArray<GenericInfo> Generics,
        ReturnTypeInfo ReturnType,
        AttributeSettings AttributeSettings,
        string DocumentationReference,
        string FilePath,
        ClassInfo ClassInfo
    );

    private sealed record ClassInfo(
        string Name,
        Accessibility Accessibility,
        string Type,
        EquatableArray<GenericInfo> Generics
    );

    private readonly record struct ParameterInfo(
        string Name,
        string Type
    );

    private readonly record struct GenericInfo(
        string Name,
        string? Constraint
    );

    private readonly record struct ReturnTypeInfo(
        string SyncType,
        bool ReturnsTask
    );

    private readonly record struct AttributeInfo
    {
        public string? ExtensionClassName { get; init; }

        public string? ExtensionMethodName { get; init; }

        public string? Namespace { get; init; }

        public readonly AttributeSettings ToSettings() =>
            new(
                ExtensionClassName ?? GenerateAsyncExtension.DefaultInstance.ExtensionClassName,
                ExtensionMethodName ?? GenerateAsyncExtension.DefaultInstance.ExtensionMethodName,
                Namespace ?? GenerateAsyncExtension.DefaultInstance.Namespace
            );

        public readonly AttributeInfo Or(AttributeInfo? other) =>
            new AttributeInfo
            {
                ExtensionClassName = ExtensionClassName ?? other?.ExtensionClassName, 
                ExtensionMethodName = ExtensionMethodName ?? other?.ExtensionMethodName, 
                Namespace = Namespace ?? other?.Namespace
            };
    }

    private readonly record struct AttributeSettings(
        string ExtensionClassName,
        string ExtensionMethodName,
        string Namespace
    );

    private readonly record struct ClassGroup(
        string ExtensionClassName,
        string Namespace,
        Accessibility Accessibility
    );

    private readonly record struct ClassRenderModel(
        string ExtensionClassName,
        string Namespace,
        EquatableArray<string> Usings,
        Accessibility Accessibility,
        EquatableArray<MethodRenderModel> Methods
    );

    private readonly record struct MethodRenderContext(
        MethodRenderModel Model,
        string FilePath,
        string OriginalNamespace
    );

    private readonly record struct MethodRenderModel(
        string Name,
        string OriginalName,
        Accessibility Accessibility,
        EquatableArray<GenericInfo> Generics,
        ParameterRenderModel ExtensionParameter,
        EquatableArray<ParameterRenderModel> Parameters,
        string ReturnType,
        bool NeedsExtraAwait,
        string DocumentationReference
    );

    private readonly record struct ParameterRenderModel(
        string Name,
        string Type
    );

    private enum MethodType
    {
        Instance,
        Extension
    }

    #endregion
}
