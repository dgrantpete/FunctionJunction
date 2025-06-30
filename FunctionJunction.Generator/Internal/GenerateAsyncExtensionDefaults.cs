using FunctionJunction.Generator.Internal.Attributes;

namespace FunctionJunction.Generator.Internal;

internal static class GenerateAsyncExtensionDefaults
{
    public const string TemplateName = "AsyncExtensionMethod.sbn";

    public const string AttributeName = $"FunctionJunction.Generator.{nameof(GenerateAsyncExtensionAttribute)}";

    public static GenerateAsyncExtensionAttribute Instance { get; } = new();
}
