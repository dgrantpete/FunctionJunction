using FunctionalEngine.Generator.Internal.Attributes;

namespace FunctionalEngine.Generator.Internal;

internal static class DiscriminatedUnionDefaults
{
    public const string TemplateName = "DiscriminatedUnion.sbn";

    public const string AttributeName = $"FunctionalEngine.Generator.{nameof(DiscriminatedUnionAttribute)}";

    public static DiscriminatedUnionAttribute Instance { get; } = new();
}
