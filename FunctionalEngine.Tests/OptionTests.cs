using System.Drawing;
using static FunctionalEngine.Functions;

namespace FunctionalEngine.Tests;

public static class OptionTests
{
    private static readonly Option<int> someValue = 1;

    private static readonly Option<int> noneValue = default;

    private static readonly Option<string> someReference = "Foo";

    private static readonly Option<string> noneReference = default;

    [Fact]
    public static void FilterSomeTrueValue()
    {
        var filtered = someValue.Filter(_ => true);

        Assert.True(filtered.IsSome);
    }

    [Fact]
    public static void FilterSomeFalseValue()
    {
        var filtered = someValue.Filter(_ => false);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public static void FilterNoneTrueValue()
    {
        var filtered = noneValue.Filter(_ => true);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public static void FilterNoneFalseValue()
    {
        var filtered = noneValue.Filter(_ => false);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public static void FilterSomeTrueReference()
    {
        var filtered = someReference.Filter(_ => true);

        Assert.True(filtered.IsSome);
    }

    [Fact]
    public static void FilterSomeFalseReference()
    {
        var filtered = someReference.Filter(_ => false);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public static void FilterNoneTrueReference()
    {
        var filtered = noneReference.Filter(_ => true);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public static void FilterNoneFalseReference()
    {
        var filtered = noneReference.Filter(_ => false);

        Assert.True(filtered.IsNone);
    }
}
