using FunctionJunction.Generator.Internal.Attributes;

namespace FunctionJunction.Generator.Internal.Models;

internal readonly record struct UnionSettings(
    MatchUnionOn MatchOn,
    bool GeneratePolymorphicSerialization,
    bool GeneratePrivateConstructor
);
