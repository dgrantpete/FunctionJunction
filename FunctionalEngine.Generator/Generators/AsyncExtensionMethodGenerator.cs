using FunctionalEngine.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
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
    private static readonly Lazy<Template> template = new(() =>
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
                    (node, _) => node is MethodDeclarationSyntax,
                    GetMethodInfo
                )
                .SelectMany<MethodInfo?, MethodInfo>((maybeMethodInfo, _) => maybeMethodInfo switch
                {
                    null => [],
                    { } methodInfo => [methodInfo]
                });

        var renderModelProvider = methodInfoProvider.Collect()
            .SelectMany((methodInfo, _) =>
                methodInfo.GroupBy(
                    methodInfo => methodInfo.ContainingClass,
                    CreateRenderModel
                )
            );

        context.RegisterSourceOutput(
            renderModelProvider,
            (context, renderModel) =>
            {
                var generatedCode = template.Value.Render(renderModel);

                context.AddSource($"{renderModel.Name}AsyncExtensions.g.cs", generatedCode);
            }
        );
    }

    private static MethodInfo? GetMethodInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var classInfo = GetClassInfo(methodSymbol.ContainingType);

        var parameterInfos = methodSymbol.Parameters.Select(GetParameterInfo)
            .ToImmutableArray();

        var generics = methodSymbol.TypeParameters
            .Select(GetGenericInfo)
            .ToImmutableArray();

        return new(
            methodSymbol.Name,
            GetClassInfo(methodSymbol.ContainingType),
            parameterInfos,
            GetTypeInfo(methodSymbol.ReturnType),
            generics,
            GetAccessibility(methodSymbol.DeclaredAccessibility)
        );
    }

    private static ClassInfo GetClassInfo(INamedTypeSymbol typeSymbol) =>
        new(
            typeSymbol.Name,
            typeSymbol.ContainingNamespace.Name,
            typeSymbol.TypeParameters
                .Select(GetGenericInfo)
                .ToImmutableArray(),
            GetAccessibility(typeSymbol.DeclaredAccessibility)
        );

    private static GenericInfo GetGenericInfo(ITypeParameterSymbol typeParameterSymbol) =>
        new(
            typeParameterSymbol.Name,
            typeParameterSymbol switch
            {
                { HasReferenceTypeConstraint: true } => "class",
                { HasValueTypeConstraint: true } => "struct",
                { HasNotNullConstraint: true } => "notnull",
                { ConstraintTypes: [var constraint] } => constraint.ToDisplayString(),
                _ => null
            }
        );

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

            var name = $"{genericSymbol.ToDisplayString(
                new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.None)
            )}<{{0}}>";

            return new TypeInfo(name, typeArguments);
        }

        return new(
            typeSymbol.ToDisplayString(),
            ImmutableArray.Create<TypeInfo>()
        );
    }

    private static Accessibility GetAccessibility(Microsoft.CodeAnalysis.Accessibility accessibility) => accessibility switch
    {
        Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
        Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
        _ => throw new InvalidOperationException("Accessibility must be 'Internal' or 'Public'")
    };

    private static string RenderType(TypeInfo type)
    {
        if (type.TypeParameters is [])
        {
            return type.Name;
        }

        var renderedParameters = string.Join(
            ", ",
            type.TypeParameters
                .Select(RenderType)
        );

        return string.Format(type.Name, renderedParameters);
    }

    private static ClassRenderModel CreateRenderModel(ClassInfo classInfo, IEnumerable<MethodInfo> methodInfos) =>
        new(
            classInfo.Name,
            classInfo.Namespace,
            classInfo.Accessibility,
            classInfo.Generics,
            methodInfos.Select(CreateMethodRenderModel)
                .ToImmutableArray()
        );

    private static MethodRenderModel CreateMethodRenderModel(MethodInfo methodInfo)
    {
        var needsExtraAwait = methodInfo.ReturnType.Name.EndsWith("Task<{0}>");

        var returnType = needsExtraAwait switch
        {
            true => methodInfo.ReturnType,
            false => new TypeInfo(
                "Task<{0}>",
                ImmutableArray.Create(methodInfo.ReturnType)
            )
        };

        var renderedReturnType = RenderType(returnType);

        var parameters = methodInfo.Parameters
            .Select(parameterInfo =>
                new ParameterRenderModel(
                    parameterInfo.Name,
                    RenderType(parameterInfo.Type)
                )
            )
            .ToImmutableArray();

        var generics = methodInfo.ContainingClass.Generics
            .Concat(methodInfo.Generics)
            .ToImmutableArray();

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
            parameters,
            renderedReturnType,
            needsExtraAwait
        );
    }

    private readonly record struct ClassRenderModel(
        string Name,
        string Namespace,
        Accessibility Accessibility,
        EquatableArray<GenericInfo> Generics,
        EquatableArray<MethodRenderModel> Methods
    );

    private readonly record struct MethodRenderModel(
        string Name,
        string OriginalName,
        Accessibility Accessibility,
        EquatableArray<GenericInfo> Generics,
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
        Accessibility Accessibility
    );

    private readonly record struct ClassInfo(
        string Name,
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
}
