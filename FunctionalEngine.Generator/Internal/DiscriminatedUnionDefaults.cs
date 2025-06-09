namespace FunctionalEngine.Generator.Internal;

internal static class DiscriminatedUnionDefaults
{
    public const string TemplateName = "DiscriminatedUnion.sbn";

    public static string AttributeName { get; } = typeof(DiscriminatedUnionAttribute).FullName;

    public static MatchUnionOn MatchOn { get; } = MatchUnionOn.Type;

    public const bool GeneratePolymorphicSerialization = true;

    public const bool GeneratePrivateConstructor = true;
}
