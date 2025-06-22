namespace FunctionalEngine.Generator.Internal;

internal static class DiscriminatedUnionDefaults
{
    public const string TemplateName = "DiscriminatedUnion.sbn";

    public static string AttributeName { get; } = typeof(DiscriminatedUnionAttribute).FullName;

    public static DiscriminatedUnionAttribute Instance { get; } = new();
}
