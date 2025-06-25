using static FunctionalEngine.Option;

namespace FunctionalEngine.Tests;

public class OptionTests
{
    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        var option = Some(42);

        Assert.True(option.IsSome);
        Assert.False(option.IsNone);
    }

    [Fact]
    public void None_CreatesEmptyOption()
    {
        var option = None<int>();
        
        Assert.False(option.IsSome);
        Assert.True(option.IsNone);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSome()
    {
        Option<string> option = "test";

        Assert.True(option.IsSome);
    }

    [Fact]
    public void ImplicitConversion_FromNull_CreatesNone()
    {
        Option<string> option = null;

        Assert.True(option.IsNone);
    }

    [Fact]
    public void Match_WithSome_CallsOnSome()
    {
        var option = Some(10);

        var result = option.Match(
            onSome: x => x * 2,
            onNone: () => 0
        );

        Assert.Equal(20, result);
    }

    [Fact]
    public void Match_WithNone_CallsOnNone()
    {
        var option = None<int>();

        var result = option.Match(
            onSome: x => x * 2,
            onNone: () => -1
        );

        Assert.Equal(-1, result);
    }

    [Fact]
    public void Map_TransformsValue_WhenSome()
    {
        var option = Some(5);

        var result = option.Map(x => x.ToString());

        Assert.True(result.IsSome);
        Assert.Equal("5", result.UnwrapOr(() => string.Empty));
    }

    [Fact]
    public void Map_ReturnsNone_WhenNone()
    {
        var option = None<int>();

        var result = option.Map(x => x.ToString());

        Assert.True(result.IsNone);
    }

    [Fact]
    public void FlatMap_ChainsOptions()
    {
        var option = Some(10);

        var result = option.FlatMap(x => x > 5 ? Some(x * 2) : None<int>());

        Assert.Equal(20, result.UnwrapOr(() => 0));
    }

    [Fact]
    public void Filter_KeepsValue_WhenPredicateTrue()
    {
        var option = Some(10);

        var result = option.Filter(x => x > 5);

        Assert.True(result.IsSome);
    }

    [Fact]
    public void Filter_RemovesValue_WhenPredicateFalse()
    {
        var option = Some(3);

        var result = option.Filter(x => x > 5);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Or_ReturnsOriginal_WhenSome()
    {
        var option = Some(1);

        var result = option.Or(() => Some(2));

        Assert.Equal(1, result.UnwrapOr(() => 0));
    }

    [Fact]
    public void Or_ReturnsAlternative_WhenNone()
    {
        var option = None<int>();

        var result = option.Or(() => Some(2));

        Assert.Equal(2, result.UnwrapOr(() => 0));
    }

    [Fact]
    public void And_CombinesBothValues_WhenBothSome()
    {
        var option1 = Some(1);

        var result = option1.And(() => Some("test"));

        Assert.True(result.IsSome);
        var (left, right) = result.UnwrapOr(() => (0, string.Empty));
        Assert.Equal(1, left);
        Assert.Equal("test", right);
    }

    [Fact]
    public void UnwrapOr_ReturnsValue_WhenSome()
    {
        var option = Some(42);

        var result = option.UnwrapOr(() => 0);

        Assert.Equal(42, result);
    }

    [Fact]
    public void UnwrapOr_ReturnsDefault_WhenNone()
    {
        var option = None<int>();

        var result = option.UnwrapOr(() => 99);

        Assert.Equal(99, result);
    }

    [Fact]
    public void UnwrapOrThrow_ReturnsValue_WhenSome()
    {
        var option = Some("value");

        var result = option.UnwrapOrThrow();

        Assert.Equal("value", result);
    }

    [Fact]
    public void UnwrapOrThrow_ThrowsException_WhenNone()
    {
        var option = None<string>();

        Assert.Throws<InvalidOperationException>(() => option.UnwrapOrThrow());
    }

    [Fact]
    public void ToEnumerable_YieldsValue_WhenSome()
    {
        var option = Some(10);

        var list = option.ToEnumerable().ToList();

        Assert.Single(list);
        Assert.Equal(10, list[0]);
    }

    [Fact]
    public void ToEnumerable_YieldsNothing_WhenNone()
    {
        var option = None<int>();

        var list = option.ToEnumerable().ToList();

        Assert.Empty(list);
    }

    [Fact]
    public void TryUnwrap_ReturnsTrue_WhenSome()
    {
        var option = Some(42);

        var success = option.TryUnwrap(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryUnwrap_ReturnsFalse_WhenNone()
    {
        var option = None<int>();

        var success = option.TryUnwrap(out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void Flatten_RemovesNestedOption()
    {
        var nested = Some(Some(42));

        var result = nested.Flatten();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.UnwrapOr(() => 0));
    }

    [Fact]
    public void ToResult_ConvertsToOk_WhenSome()
    {
        var option = Some(10);

        var result = option.ToResult(() => "error");

        Assert.True(result.IsOk);
        Assert.Equal(10, result.UnwrapOr(_ => 0));
    }

    [Fact]
    public void ToResult_ConvertsToError_WhenNone()
    {
        var option = None<int>();

        var result = option.ToResult(() => "error");

        Assert.True(result.IsError);
        Assert.Equal("error", result.ToErrorOption().UnwrapOr(() => string.Empty));
    }
}