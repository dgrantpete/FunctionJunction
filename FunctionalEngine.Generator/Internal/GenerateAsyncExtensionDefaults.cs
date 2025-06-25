using FunctionalEngine.Generator.Internal.Attributes;

namespace FunctionalEngine.Generator.Internal;

internal static class GenerateAsyncExtensionDefaults
{
    public const string TemplateName = "AsyncExtensionMethod.sbn";
    
    public const string AttributeName = $"FunctionalEngine.Generator.{nameof(GenerateAsyncExtensionAttribute)}";

    public static GenerateAsyncExtensionAttribute Instance { get; } = new();
}
