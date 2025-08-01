using FunctionJunction.Extensions;

namespace FunctionJunction.Tests.Examples;

public class OptionExamples
{
    [Fact]
    public static void Instantiate()
    {
        // begin-snippet: OptionInstantiation
        // Explicitly created
        var explicitSome = Option.Some("Foo");
        var explicitNone = Option.None<string>();

        // Implicitly created
        Option<int> implicitSome = 10;
        Option<int> implicitNone = default;

        Assert.Equal(Option.Some(10), implicitSome);
        Assert.Equal(Option.None<int>(), implicitNone);

        // From nullable value types
        double? definitelyForSureNotNull = null;
        var valueOption = Option.FromNullable(definitelyForSureNotNull);

        Assert.Equal(default, valueOption);

        // Nullable reference types can be converted implicitly
        string? nullableReference = "Not null";
        Option<string> referenceOption = nullableReference;

        Assert.Equal("Not null", nullableReference);

        // From boolean values
        var parsedOption = int.TryParse("123", out var parseResult)
            .ToOption(() => parseResult);

        Assert.Equal(123, parsedOption);
        // end-snippet
    }

    [Fact]
    public static void Transform()
    {
        // begin-snippet: OptionTransformation
        var someValue = Option.Some(42);
        var noneValue = Option.None<int>();

        // Map - transform the value if present
        var doubled = someValue.Map(x => x * 2);
        var doubledNone = noneValue.Map(x => x * 2);

        Assert.Equal(84, doubled);
        Assert.Equal(doubledNone, default);

        // FlatMap - transform to Option and flatten
        Option<int> TryDiv(int dividend, int divisor) => divisor switch
        {
            0 => default,
            _ => Option.Some(dividend / divisor)
        };

        var successfulDivision = someValue.FlatMap(x => TryDiv(x, 1));
        var failedDivision = someValue.FlatMap(x => TryDiv(x, 0));

        Assert.Equal(42, successfulDivision);
        Assert.Equal(default, failedDivision);

        // Filter - keep value only if condition is met
        var evenOnly = someValue.Filter(x => x % 2 is 0); // Some(42)
        var oddOnly = someValue.Filter(x => x % 2 is 1); // None

        Assert.Equal(42, evenOnly);
        Assert.Equal(default, oddOnly);

        // Or - provide alternative if None
        var withDefault = noneValue.Or(() => Option.Some(100));

        Assert.Equal(100, withDefault);

        // And - combine two Options
        var combined = someValue.And(() => Option.Some("hello"));

        Assert.Equal((42, "hello"), combined);
        // end-snippet
    }
}
