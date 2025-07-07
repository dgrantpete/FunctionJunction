using FunctionJunction.Generator.Internal.Attributes;

namespace FunctionJunction.Generator.Internal.Helpers;

internal static class DiscriminatedUnion
{
    public const string TemplateName = "DiscriminatedUnion.sbn";

    public const string AttributeName = $"FunctionJunction.Generator.{nameof(DiscriminatedUnionAttribute)}";

    public static DiscriminatedUnionAttribute DefaultInstance { get; } = new();
}
