using FunctionJunction.Generator.Internal.Attributes;

namespace FunctionJunction.Generator.Internal;

internal static class DiscriminatedUnionDefaults
{
    public const string TemplateName = "DiscriminatedUnion.sbn";

    public const string AttributeName = $"FunctionJunction.Generator.{nameof(DiscriminatedUnionAttribute)}";

    public static DiscriminatedUnionAttribute Instance { get; } = new();
}
