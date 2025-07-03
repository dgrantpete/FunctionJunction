using FunctionJunction.Generator;

namespace FunctionJunction.Tests.TestCompilations;

[DiscriminatedUnion(MatchOn = MatchUnionOn.Properties)]
public partial record Foo
{
    public record Bar : Foo;

    public record Baz : Foo;
}
