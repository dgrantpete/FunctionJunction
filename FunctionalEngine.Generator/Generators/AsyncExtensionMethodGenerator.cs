using FunctionalEngine.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace FunctionalEngine.Generator.Generators;

[Generator("C#")]
internal class AsyncExtensionMethodGenerator : IIncrementalGenerator
{
    private static readonly Lazy<Template> template = new(static () =>
    {
        var assembly = typeof(AsyncExtensionMethodGenerator).Assembly;

        var resourceName = assembly.GetManifestResourceNames()
            .Single(resource => resource.EndsWith(GenerateAsyncExtensionDefaults.TemplateName));

        using var templateStream = assembly.GetManifestResourceStream(resourceName);

        using var reader = new StreamReader(templateStream);

        var templateText = reader.ReadToEnd();

        return Template.Parse(templateText);
    });

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {   
        var methodInfoProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    GenerateAsyncExtensionDefaults.AttributeName,
                    static (node, _) => node is MethodDeclarationSyntax,
                    GetMethodInfo
                )
                .SelectMany<MethodInfo?, MethodInfo>(static (maybeMethodInfo, _) => maybeMethodInfo switch
                {
                    null => [],
                    { } methodInfo => [methodInfo]
                });

        var renderModelProvider = methodInfoProvider.Collect()
            .SelectMany(static (methodInfo, _) =>
                methodInfo.GroupBy(
                    methodInfo => (
                        ExtensionClassName: string.Format(
                            methodInfo.AttributeSettings.ExtensionClassName, 
                            methodInfo.ContainingClass.Name
                        ),
                        methodInfo.ContainingClass.Namespace,
                        methodInfo.Accessibility
                    ),
                    (classInfo, methodInfos) => 
                        CreateClassModel(classInfo.ExtensionClassName, classInfo.Namespace, classInfo.Accessibility, methodInfos)
                )
            );

        context.RegisterSourceOutput(
            renderModelProvider,
            static (context, renderModel) =>
            {
                var generatedCode = GenerateCode(renderModel);

                context.AddSource($"{renderModel.ExtensionClassName}.g.cs", generatedCode);
            }
        );
    }

    private static string GenerateCode(ClassRenderModel renderModel)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanHelpers));
        scriptObject.Import(renderModel);

        var templateContext = new TemplateContext();
        templateContext.PushGlobal(scriptObject);

        return template.Value.Render(templateContext);
    }

    private static MethodInfo? GetMethodInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol
            || GetMethodType(methodSymbol) is not { } methodType
            || GetClassInfo(methodSymbol.ContainingType) is not { } classInfo
            || GetAccessibility(methodSymbol.DeclaredAccessibility) is not { } accessibility
        )
        {
            return null;
        }

        var attributeSettings = GetAttributeSettings(context.Attributes[0]);

        var parameterInfos = methodSymbol.Parameters.Select(GetParameterInfo)
            .ToImmutableArray();

        var generics = methodSymbol.TypeParameters
            .Select(GetGenericInfo)
            .ToImmutableArray();

        return new(
            methodSymbol.Name,
            classInfo,
            parameterInfos,
            GetTypeInfo(methodSymbol.ReturnType),
            generics,
            methodType,
            accessibility,
            attributeSettings
        );
    }

    private static AttributeSettings GetAttributeSettings(AttributeData attribute)
    {
        var extensionClassName = (string?)attribute.NamedArguments
            .Select(argument => ((string Name, TypedConstant Constant)?)(argument.Key, argument.Value))
            .SingleOrDefault(argument => argument?.Name is nameof(GenerateAsyncExtensionAttribute.ExtensionClassName))
            ?.Constant
            .Value;

        return new(
            extensionClassName ?? GenerateAsyncExtensionDefaults.ExtensionClassName
        );
    }

    private static MethodType? GetMethodType(IMethodSymbol methodSymbol) => methodSymbol switch
    {
        { IsExtensionMethod: true } => MethodType.Extension,
        { IsStatic: false, MethodKind: MethodKind.Ordinary } => MethodType.Instance,
        _ => null
    };

    private static ClassInfo? GetClassInfo(INamedTypeSymbol typeSymbol)
    {
        if (GetAccessibility(typeSymbol.DeclaredAccessibility) is not { } accessibility)
        {
            return null;
        }

        return new(
            typeSymbol.Name,
            typeSymbol.ToDisplayString(),
            typeSymbol.ContainingNamespace.Name,
            typeSymbol.TypeParameters
                .Select(GetGenericInfo)
                .ToImmutableArray(),
            accessibility
        );
    }

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
            GetTypeInfo(parameterSymbol.Type)
        );

    private static TypeInfo GetTypeInfo(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arraySymbol)
        {
            var name = $"{{0}}[{new string(',', arraySymbol.Rank - 1)}]";

            return new(
                name, 
                ImmutableArray.Create(GetTypeInfo(arraySymbol.ElementType))
            );
        }

        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableSymbol)
        {
            var innerType = GetTypeInfo(nullableSymbol.TypeArguments[0]);

            return new("{0}?", ImmutableArray.Create(innerType));
        }

        if (typeSymbol is INamedTypeSymbol { IsGenericType: true, IsTupleType: false } genericSymbol)
        {
            var typeArguments = genericSymbol.TypeArguments.Select(GetTypeInfo).ToImmutableArray();

            var displayName = genericSymbol.ToDisplayString(format: new(genericsOptions: SymbolDisplayGenericsOptions.None));

            var name = $"{displayName}<{{0}}>";

            return new TypeInfo(name, typeArguments);
        }

        return new(
            typeSymbol.ToDisplayString(),
            ImmutableArray.Create<TypeInfo>()
        );
    }

    private static Accessibility? GetAccessibility(Microsoft.CodeAnalysis.Accessibility accessibility) => accessibility switch
    {
        Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
        Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
        _ => null
    };

    private static string RenderType(TypeInfo type)
    {
        var renderedParameters = string.Join(
            ", ",
            type.TypeParameters
                .Select(RenderType)
        );

        return string.Format(type.Name, renderedParameters);
    }

    private static ClassRenderModel CreateClassModel(
        string extensionClassName, 
        string @namespace, 
        Accessibility accessibility, 
        IEnumerable<MethodInfo> methodInfos
    ) =>
        new(
            extensionClassName,
            @namespace,
            accessibility,
            methodInfos.Select(CreateMethodModel)
                .ToImmutableArray()
        );

    private static MethodRenderModel CreateMethodModel(MethodInfo methodInfo)
    {
        var needsExtraAwait = methodInfo.ReturnType.Name.EndsWith("Task<{0}>");

        var returnType = needsExtraAwait switch
        {
            true => methodInfo.ReturnType.TypeParameters[0],
            false => methodInfo.ReturnType
        };

        var renderedReturnType = RenderType(returnType);

        var generics = methodInfo.ContainingClass.Generics
            .Concat(methodInfo.Generics)
            .ToImmutableArray();

        var (extensionParameter, parameters) = CreateParameterModels(methodInfo);

        var name = methodInfo.Name.EndsWith("Async") switch
        {
            true => methodInfo.Name,
            false => $"{methodInfo.Name}Async"
        };

        return new(
            name,
            methodInfo.Name,
            methodInfo.Accessibility,
            generics,
            extensionParameter,
            parameters,
            renderedReturnType,
            needsExtraAwait
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
            ToCamelCase(methodInfo.ContainingClass.Name),
            methodInfo.ContainingClass.Type
        );

        return (extensionParameter, [.. methodInfo.Parameters.Select(CreateParameterModel)]);
    }

    private static ParameterRenderModel CreateParameterModel(ParameterInfo parameterInfo) =>
        new(
            parameterInfo.Name,
            RenderType(parameterInfo.Type)
        );

    private static string ToCamelCase(string pascalCaseString) =>
        char.ToLowerInvariant(pascalCaseString[0]) + pascalCaseString[1..];

    private readonly record struct AttributeSettings(
        string ExtensionClassName
    );

    private readonly record struct ClassRenderModel(
        string ExtensionClassName,
        string Namespace,
        Accessibility Accessibility,
        EquatableArray<MethodRenderModel> Methods
    );

    private readonly record struct MethodRenderModel(
        string Name,
        string OriginalName,
        Accessibility Accessibility,
        EquatableArray<GenericInfo> Generics,
        ParameterRenderModel ExtensionParameter,
        EquatableArray<ParameterRenderModel> Parameters,
        string ReturnType,
        bool NeedsExtraAwait
    );

    private readonly record struct ParameterRenderModel(
        string Name,
        string Type
    );

    private readonly record struct MethodInfo(
        string Name,
        ClassInfo ContainingClass,
        EquatableArray<ParameterInfo> Parameters,
        TypeInfo ReturnType,
        EquatableArray<GenericInfo> Generics,
        MethodType Type,
        Accessibility Accessibility,
        AttributeSettings AttributeSettings
    );

    private readonly record struct ClassInfo(
        string Name,
        string Type,
        string Namespace,
        EquatableArray<GenericInfo> Generics,
        Accessibility Accessibility
    );

    private readonly record struct ParameterInfo(
        string Name,
        TypeInfo Type
    );

    private record TypeInfo(
        string Name,
        EquatableArray<TypeInfo> TypeParameters
    );

    private readonly record struct GenericInfo(
        string Name,
        string? Constraint
    );

    private enum Accessibility
    {
        Public,
        Internal
    }

    private enum MethodType
    {
        Instance,
        Extension
    }

    private static class ScribanHelpers
    {
        public static string ToCamelCase(string text) => AsyncExtensionMethodGenerator.ToCamelCase(text);

        public static string RenderGenerics(IEnumerable<GenericInfo> generics) =>
            $"<{string.Join(", ", generics.Select(generic => generic.Name))}>";
    }
}
