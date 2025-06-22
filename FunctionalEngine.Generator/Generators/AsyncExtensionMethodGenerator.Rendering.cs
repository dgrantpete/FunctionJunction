using FunctionalEngine.Generator.Internal;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace FunctionalEngine.Generator.Generators;

partial class AsyncExtensionMethodGenerator
{
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

    private static ImmutableArray<ClassRenderModel> CreateClassModels(
        ImmutableArray<UngroupedMethodData> methodDatas,
        CancellationToken cancellationToken = default
    ) =>
        [.. methodDatas.GroupBy(
            methodData => (
                methodData.Namespace,
                methodData.Accessibility,
                methodData.ExtensionClassName
            ),
            (groupedData, methodDatas) =>
                CreateClassModel(
                    groupedData.ExtensionClassName,
                    groupedData.Namespace,
                    methodDatas.SelectMany(methodData => methodData.Usings)
                        .Distinct(),
                    groupedData.Accessibility,
                    methodDatas.Select(methodData => methodData.RenderModel),
                    cancellationToken
                )
            )];

    private static ClassRenderModel CreateClassModel(
        string extensionClassName,
        string @namespace,
        IEnumerable<string> usings,
        Accessibility accessibility,
        IEnumerable<MethodRenderModel> methodModels,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return new(
            extensionClassName,
            @namespace,
            usings.ToImmutableArray(),
            accessibility,
            methodModels.ToImmutableArray()
        );
    }

    private static UngroupedMethodData CreateMethodModel(MethodInfo methodInfo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classInfo = methodInfo.ContainingClass;

        var attributeSettings = methodInfo.AttributeInfo
            .Or(classInfo.AttributeInfo)
            .ToSettings();

        var generics = classInfo.Generics
            .Concat(methodInfo.Generics)
            .ToImmutableArray();

        var (extensionParameter, parameters) = CreateParameterModels(methodInfo);

        var name = FormatTemplatedName(methodInfo.Name, attributeSettings.ExtensionMethodName);

        var methodModel = new MethodRenderModel(
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

        return new(
            methodModel, 
            FormatTemplatedName(classInfo.Name, attributeSettings.ExtensionClassName),
            FormatTemplatedName(classInfo.Namespace, attributeSettings.Namespace),
            classInfo.Usings,
            classInfo.Accessibility
        );
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
            parameterInfo.Type
        );

    private static string ToCamelCase(string pascalCaseString) =>
        char.ToLowerInvariant(pascalCaseString[0]) + pascalCaseString[1..];

    private readonly record struct ClassRenderModel(
        string ExtensionClassName,
        string Namespace,
        EquatableArray<string> Usings,
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
        bool NeedsExtraAwait,
        string DocumentationReference
    );

    private readonly record struct UngroupedMethodData(
        MethodRenderModel RenderModel,
        string ExtensionClassName,
        string Namespace,
        EquatableArray<string> Usings,
        Accessibility Accessibility
    );

    private readonly record struct ParameterRenderModel(
        string Name,
        string Type
    );

    private static class ScribanHelpers
    {
        public static string ToCamelCase(string text) => AsyncExtensionMethodGenerator.ToCamelCase(text);

        public static string RenderGenerics(IEnumerable<GenericInfo> generics) =>
            $"<{string.Join(", ", generics.Select(generic => generic.Name))}>";
    }
}
