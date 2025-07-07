using FunctionJunction.Generator.Internal.Attributes;

namespace FunctionJunction.Generator.Internal.Helpers;

internal static class GenerateAsyncExtension
{
    public const string TemplateName = "AsyncExtensionMethod.sbn";

    public const string AttributeName = $"FunctionJunction.Generator.{nameof(GenerateAsyncExtensionAttribute)}";

    public static GenerateAsyncExtensionAttribute DefaultInstance { get; } = new();
}
