using FunctionalEngine.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace FunctionalEngine.Generator.Generators;

partial class AsyncExtensionMethodGenerator
{
    private static IEnumerable<MethodInfo> GetMethodInfosForClass(
        INamedTypeSymbol classSymbol, 
        ImmutableArray<GeneratorAttributeSyntaxContext> methodContexts,
        CancellationToken cancellationToken = default
    )
    {
        if (
            methodContexts is not [{ SemanticModel.Compilation: var compilation }, ..]
                || compilation.GetTypeByMetadataName(GenerateAsyncExtensionDefaults.AttributeName) is not { } attributeSymbol
                || compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1") is not { } taskSymbol
                || GetClassInfo(classSymbol, GetUsingStatements(methodContexts, cancellationToken), attributeSymbol) is not { } classInfo
        )
        {
            return [];
        }

        return methodContexts.Select(methodContext => GetMethodInfo(methodContext, classInfo, taskSymbol, cancellationToken))
            .OfType<MethodInfo>();
    }

    private static ClassInfo? GetClassInfo(INamedTypeSymbol classSymbol, IEnumerable<string> usings, INamedTypeSymbol attributeSymbol)
    {
        if (GetAccessibility(classSymbol.DeclaredAccessibility) is not { } accessibility)
        {
            return null;
        }

        var attribute = classSymbol.GetAttributes()
            .SingleOrDefault(attribute =>
                SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol)
            );

        var classInfo = new ClassInfo(
            classSymbol.Name,
            GetFormattedType(classSymbol),
            classSymbol.ContainingNamespace.ToDisplayString(),
            usings.ToImmutableArray(),
            classSymbol.TypeParameters
                .Select(GetGenericInfo)
                .ToImmutableArray(),
            accessibility,
            GetAttributeInfo(attribute)
        );

        return classInfo;
    }

    private static string GetFormattedType(ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(format: new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        ));

    private static IEnumerable<string> GetUsingStatements(IEnumerable<GeneratorAttributeSyntaxContext> methodContexts, CancellationToken cancellationToken) =>
        methodContexts.Select(methodContext => methodContext.TargetNode.SyntaxTree)
            .Distinct()
            .SelectMany(syntaxTree => syntaxTree
                .GetCompilationUnitRoot(cancellationToken)
                .Usings
            )
            .Select(@using => @using.ToString())
            .Distinct();

    private static MethodInfo? GetMethodInfo(
        GeneratorAttributeSyntaxContext context, 
        ClassInfo classInfo, 
        INamedTypeSymbol taskSymbol, 
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TargetSymbol is not IMethodSymbol methodSymbol
            || GetMethodType(methodSymbol) is not { } methodType
            || GetAccessibility(methodSymbol.DeclaredAccessibility) is not { } accessibility
            || context.Attributes is not [var attribute, ..]
        )
        {
            return null;
        }

        var attributeInfo = GetAttributeInfo(attribute);

        var parameterInfos = methodSymbol.Parameters.Select(GetParameterInfo)
            .ToImmutableArray();

        var generics = methodSymbol.TypeParameters
            .Select(GetGenericInfo)
            .ToImmutableArray();

        return new(
            methodSymbol.Name,
            classInfo,
            parameterInfos,
            generics,
            GetReturnType(methodSymbol.ReturnType, taskSymbol),
            methodType,
            accessibility,
            attributeInfo,
            methodSymbol.GetDocumentationCommentId()!
        );
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

    private static ReturnTypeInfo GetReturnType(ITypeSymbol returnTypeSymbol, INamedTypeSymbol taskSymbol)
    {
        if (
            returnTypeSymbol is INamedTypeSymbol namedTypeSymbol
                && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, taskSymbol)
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

    private static Accessibility? GetAccessibility(Microsoft.CodeAnalysis.Accessibility accessibility) => accessibility switch
    {
        Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
        Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
        _ => null
    };

    private sealed record ClassInfo(
        string Name,
        string Type,
        string Namespace,
        EquatableArray<string> Usings,
        EquatableArray<GenericInfo> Generics,
        Accessibility Accessibility,
        AttributeInfo AttributeInfo
    );

    private readonly record struct MethodInfo(
        string Name,
        ClassInfo ContainingClass,
        EquatableArray<ParameterInfo> Parameters,
        EquatableArray<GenericInfo> Generics,
        ReturnTypeInfo ReturnType,
        MethodType Type,
        Accessibility Accessibility,
        AttributeInfo AttributeInfo,
        string DocumentationReference
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
                ExtensionClassName ?? GenerateAsyncExtensionDefaults.ExtensionClassName,
                ExtensionMethodName ?? GenerateAsyncExtensionDefaults.ExtensionMethodName,
                Namespace ?? GenerateAsyncExtensionDefaults.Namespace
            );

        public readonly AttributeInfo Or(AttributeInfo? other) =>
            new()
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
}
