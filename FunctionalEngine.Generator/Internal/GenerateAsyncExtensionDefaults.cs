namespace FunctionalEngine.Generator.Internal;

internal static class GenerateAsyncExtensionDefaults
{
    public const string TemplateName = "AsyncExtensionMethod.sbn";

    public static string AttributeName { get; } = typeof(GenerateAsyncExtensionAttribute).FullName;

    public const string ExtensionClassName = "{0}ExtensionsAsync";

    public const string ExtensionMethodName = "Await{0}";

    public const string Namespace = "{0}";
}
