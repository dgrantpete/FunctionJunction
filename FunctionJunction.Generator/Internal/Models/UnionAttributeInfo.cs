using FunctionJunction.Generator.Internal.Attributes;
using FunctionJunction.Generator.Internal.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;

namespace FunctionJunction.Generator.Internal.Models;

internal readonly record struct UnionAttributeInfo
{
    public MatchUnionOn? MatchOn { get; init; }

    public JsonPolymorphism? JsonPolymorphism { get; init; }

    public bool? GeneratePrivateConstructor { get; init; }

    public static UnionAttributeInfo FromAttributeData(AttributeData attribute, CancellationToken cancellationToken = default)
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
            JsonPolymorphism = (JsonPolymorphism?)(unionArguments.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.JsonPolymorphism))
                ?.Value as int?),
            GeneratePrivateConstructor = unionArguments.GetValueOrDefault(nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor))
                ?.Value as bool?
        };
    }

    public static UnionAttributeInfo GetDefaults(
        AnalyzerConfigOptions options, 
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string PropertyPrefix = "build_property.FunctionJunction_Defaults_";

        options.TryGetValue(
            PropertyPrefix + nameof(DiscriminatedUnionAttribute.MatchOn),
            out var match
        );

        options.TryGetValue(
            PropertyPrefix + nameof(DiscriminatedUnionAttribute.JsonPolymorphism),
            out var jsonPolymorphism
        );

        options.TryGetValue(
            PropertyPrefix + nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor),
            out var privateConstructor
        );

        return new()
        {
            MatchOn = TryParseEnum<MatchUnionOn>(match),
            JsonPolymorphism = TryParseEnum<JsonPolymorphism>(jsonPolymorphism),
            GeneratePrivateConstructor = TryParseBool(privateConstructor)
        };

        static bool? TryParseBool(string? text)
        {
            if (text is null or "")
            {
                return null;
            }

            if (!bool.TryParse(text, out var value))
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

            if (!Enum.TryParse<T>(text, out var value))
            {
                return null;
            }

            return value;
        }
    }

    public IEnumerable<SyntaxNode> GenerateAttributeArguments(SyntaxGenerator generator)
    {
        if (MatchOn is { } matchOn)
        {
            yield return generator.AttributeArgument(
                nameof(DiscriminatedUnionAttribute.MatchOn),
                generator.MemberAccessExpression(
                    generator.IdentifierName(nameof(MatchUnionOn)),
                    matchOn.ToString()
                )
            );
        }

        if (JsonPolymorphism is { } jsonPolymorphism)
        {
            yield return generator.AttributeArgument(
                nameof(DiscriminatedUnionAttribute.JsonPolymorphism),
                generator.MemberAccessExpression(
                    generator.IdentifierName(nameof(JsonPolymorphism)),
                    jsonPolymorphism.ToString()
                )
            );
        }

        if (GeneratePrivateConstructor is { } privateConstructor)
        {
            yield return generator.AttributeArgument(
                nameof(DiscriminatedUnionAttribute.GeneratePrivateConstructor),
                generator.LiteralExpression(privateConstructor)
            );
        }
    }

    public UnionAttributeInfo Or(UnionAttributeInfo other) =>
        this with
        {
            MatchOn = MatchOn ?? other.MatchOn,
            JsonPolymorphism = JsonPolymorphism ?? other.JsonPolymorphism,
            GeneratePrivateConstructor = GeneratePrivateConstructor ?? other.GeneratePrivateConstructor
        };

    public UnionSettings ToSettings() =>
        new(
            MatchOn ?? DiscriminatedUnion.DefaultInstance.MatchOn,
            JsonPolymorphism ?? DiscriminatedUnion.DefaultInstance.JsonPolymorphism,
            GeneratePrivateConstructor ?? DiscriminatedUnion.DefaultInstance.GeneratePrivateConstructor
        );
}
