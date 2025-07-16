using FunctionJunction.Extensions;

namespace FunctionJunction.Tests.Examples;

public class OptionExamples
{
    [Fact]
    public static void Instantiate()
    {
        // begin-snippet: OptionInstantiation
        // Implicitly created
        Option<int> implicitSome = 10;
        Option<int> implicitNone = default;

        // Explicitly created
        var explicitSome = Option.Some("Foo");
        var explicitNone = Option.None<string>();

        // From nullable value types
        double? definitelyForSureNotNull = null;
        var valueOption = Option.FromNullable(definitelyForSureNotNull);

        // Nullable reference types can be converted implicitly
        string? nullableReference = "Not null";
        Option<string> referenceOption = nullableReference;
        // end-snippet
    }

    [Fact]
    public static void Transform()
    {
        // begin-snippet: OptionTransformation
        var a = Iterator.Generate(Console.ReadLine)
            .Select(Option.FromNullable);
        // end-snippet
    }
}
