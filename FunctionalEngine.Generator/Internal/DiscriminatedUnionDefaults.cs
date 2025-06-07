namespace FunctionalEngine.Generator.Internal;

internal static class DiscriminatedUnionDefaults
{
    public const string TemplateName = "DiscriminatedUnion.sbn";

    public const string AttributeName = $"{nameof(FunctionalEngine)}.{nameof(DiscriminatedUnionAttribute)}";

    public const bool GenerateMatch = true;

    public const bool GeneratePolymorphicSerialization = true;

    public const bool GeneratePrivateConstructor = true;
}
