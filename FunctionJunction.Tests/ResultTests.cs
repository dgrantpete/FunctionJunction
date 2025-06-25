using static FunctionJunction.Result;

namespace FunctionJunction.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_CreatesSuccessResult()
    {
        var result = Ok<int, string>(42);

        Assert.True(result.IsOk);
        Assert.False(result.IsError);
    }

    [Fact]
    public void Error_CreatesErrorResult()
    {
        var result = Error<int, string>("failure");

        Assert.False(result.IsOk);
        Assert.True(result.IsError);
    }

    [Fact]
    public void ImplicitConversion_FromOkValue()
    {
        Result<int, string> result = 42;

        Assert.True(result.IsOk);
        Assert.Equal(42, result.UnwrapOr(_ => 0));
    }

    [Fact]
    public void ImplicitConversion_FromErrorValue()
    {
        Result<int, string> result = "error";

        Assert.True(result.IsError);
        Assert.Equal("error", result.UnwrapErrorOr(_ => ""));
    }

    [Fact]
    public void Match_WithOk_CallsOkBranch()
    {
        var result = Ok<int, string>(10);

        var matched = result.Match(
            ok => ok * 2,
            error => 0
        );

        Assert.Equal(20, matched);
    }

    [Fact]
    public void Match_WithError_CallsErrorBranch()
    {
        var result = Error<int, string>("failed");

        var matched = result.Match(
            ok => "success",
            error => error.ToUpper()
        );

        Assert.Equal("FAILED", matched);
    }

    [Fact]
    public void Map_TransformsOkValue()
    {
        var result = Ok<int, string>(5);

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(10, mapped.UnwrapOr(_ => 0));
    }

    [Fact]
    public void Map_PropagatesError()
    {
        var result = Error<int, string>("error");

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsError);
        Assert.Equal("error", mapped.UnwrapErrorOr(_ => ""));
    }

    [Fact]
    public void FlatMap_ChainsOkResults()
    {
        var result = Ok<int, string>(10);

        var chained = result.FlatMap(x =>
            x > 5 ? Ok<int, string>(x * 2) : Error<int, string>("too small")
        );

        Assert.Equal(20, chained.UnwrapOr(_ => 0));
    }

    [Fact]
    public void FlatMap_ShortCircuitsOnError()
    {
        var result = Error<int, string>("initial error");

        var chained = result.FlatMap(x => Ok<int, string>(x * 2));

        Assert.True(chained.IsError);
        Assert.Equal("initial error", chained.UnwrapErrorOr(_ => ""));
    }

    [Fact]
    public void MapError_TransformsError()
    {
        var result = Error<int, string>("error");

        var mapped = result.MapError(e => e.Length);

        Assert.True(mapped.IsError);
        Assert.Equal(5, mapped.UnwrapErrorOr(_ => 0));
    }

    [Fact]
    public void MapError_PropagatesOk()
    {
        var result = Ok<int, string>(42);

        var mapped = result.MapError(e => e.Length);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.UnwrapOr(_ => 0));
    }

    [Fact]
    public void Recover_HandlesError()
    {
        var result = Error<int, string>("error");

        var recovered = result.Recover(e => Ok<int, int>(e.Length));

        Assert.True(recovered.IsOk);
        Assert.Equal(5, recovered.UnwrapOr(_ => 0));
    }

    [Fact]
    public void Validate_AddsValidation()
    {
        var result = Ok<int, string>(10);

        var validated = result.Validate(
            x => x > 20,
            x => $"{x} is too small"
        );

        Assert.True(validated.IsError);
        Assert.Equal("10 is too small", validated.UnwrapErrorOr(_ => ""));
    }

    [Fact]
    public void And_CombinesTwoOkResults()
    {
        var result1 = Ok<int, string>(1);

        var combined = result1.And(() => Ok<string, string>("test"));

        Assert.True(combined.IsOk);
        var (left, right) = combined.UnwrapOr(_ => (0, ""));
        Assert.Equal(1, left);
        Assert.Equal("test", right);
    }

    [Fact]
    public void And_PropagatesFirstError()
    {
        var result1 = Error<int, string>("first error");

        var combined = result1.And(() => Ok<string, string>("test"));

        Assert.True(combined.IsError);
        Assert.Equal("first error", combined.UnwrapErrorOr(_ => ""));
    }

    [Fact]
    public void Or_ReturnsFirstOk()
    {
        var result1 = Ok<int, string>(42);

        var combined = result1.Or(() => Ok<int, string>(99));

        Assert.True(combined.IsOk);
        Assert.Equal(42, combined.UnwrapOr(_ => 0));
    }

    [Fact]
    public void Or_CombinesErrors()
    {
        var result1 = Error<int, string>("error1");

        var combined = result1.Or(() => Error<int, int>(404));

        Assert.True(combined.IsError);
        var (left, right) = combined.UnwrapErrorOr(_ => ("", 0));
        Assert.Equal("error1", left);
        Assert.Equal(404, right);
    }

    [Fact]
    public void Swap_SwapsOkToError()
    {
        var result = Ok<int, string>(42);

        var swapped = result.Swap();

        Assert.True(swapped.IsError);
        Assert.Equal(42, swapped.UnwrapErrorOr(_ => 0));
    }

    [Fact]
    public void Swap_SwapsErrorToOk()
    {
        var result = Error<int, string>("error");

        var swapped = result.Swap();

        Assert.True(swapped.IsOk);
        Assert.Equal("error", swapped.UnwrapOr(_ => ""));
    }

    [Fact]
    public void UnwrapOr_ReturnsValue_WhenOk()
    {
        var result = Ok<int, string>(42);

        var value = result.UnwrapOr(err => 0);

        Assert.Equal(42, value);
    }

    [Fact]
    public void UnwrapOr_ReturnsDefault_WhenError()
    {
        var result = Error<int, string>("error");

        var value = result.UnwrapOr(err => err.Length);

        Assert.Equal(5, value);
    }

    [Fact]
    public void UnwrapOrThrow_ReturnsValue_WhenOk()
    {
        var result = Ok<string, string>("value");

        var value = result.UnwrapOrThrow();

        Assert.Equal("value", value);
    }

    [Fact]
    public void UnwrapOrThrow_ThrowsException_WhenError()
    {
        var result = Error<string, string>("error message");

        var ex = Assert.Throws<InvalidOperationException>(() => result.UnwrapOrThrow());
        Assert.Contains("error message", ex.Message);
    }

    [Fact]
    public void UnwrapOrThrowException_ThrowsErrorDirectly()
    {
        var result = Error<int, ArgumentException>(new ArgumentException("bad arg"));

        var ex = Assert.Throws<ArgumentException>(() => result.UnwrapOrThrowException());
        Assert.Equal("bad arg", ex.Message);
    }

    [Fact]
    public void ToEnumerable_YieldsValue_WhenOk()
    {
        var result = Ok<int, string>(10);

        var list = result.ToEnumerable().ToList();

        Assert.Single(list);
        Assert.Equal(10, list[0]);
    }

    [Fact]
    public void ToEnumerable_YieldsNothing_WhenError()
    {
        var result = Error<int, string>("error");

        var list = result.ToEnumerable().ToList();

        Assert.Empty(list);
    }

    [Fact]
    public void ToErrorEnumerable_YieldsError_WhenError()
    {
        var result = Error<int, string>("error");

        var list = result.ToErrorEnumerable().ToList();

        Assert.Single(list);
        Assert.Equal("error", list[0]);
    }

    [Fact]
    public void TryUnwrap_ReturnsTrue_WhenOk()
    {
        var result = Ok<int, string>(42);

        var success = result.TryUnwrap(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryUnwrapError_ReturnsTrue_WhenError()
    {
        var result = Error<int, string>("error");

        var success = result.TryUnwrapError(out var error);

        Assert.True(success);
        Assert.Equal("error", error);
    }

    [Fact]
    public void ToOption_ConvertOkToSome()
    {
        var result = Ok<int, string>(42);

        var option = result.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.UnwrapOr(() => 0));
    }

    [Fact]
    public void ToOption_ConvertErrorToNone()
    {
        var result = Error<int, string>("error");

        var option = result.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void PatternMatching_WithOkRecord()
    {
        var result = Ok<int, string>(42);

        var isFortyTwo = result switch
        {
            Result<int, string>.Ok(42) => true,
            _ => false
        };

        Assert.True(isFortyTwo);
    }

    [Fact]
    public void PatternMatching_WithErrorRecord()
    {
        var result = Error<int, string>("failure");

        var message = result switch
        {
            Result<int, string>.Error(var err) => err,
            _ => "no error"
        };

        Assert.Equal("failure", message);
    }
}