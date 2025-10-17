namespace FunctionJunction.Tests;

public class VoidMatchTests
{
    [Fact]
    public void Option_VoidMatch_Some_CallsCorrectAction()
    {
        // Arrange
        var option = Option.Some(42);
        var wasCalled = false;
        var value = 0;

        // Act
        option.Match(
            onSome: v =>
            {
                wasCalled = true;
                value = v;
            },
            onNone: () =>
            {
                Assert.Fail("Should not call onNone");
            }
        );

        // Assert
        Assert.True(wasCalled);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Option_VoidMatch_None_CallsCorrectAction()
    {
        // Arrange
        var option = Option.None<int>();
        var wasCalled = false;

        // Act
        option.Match(
            onSome: v =>
            {
                Assert.Fail("Should not call onSome");
            },
            onNone: () =>
            {
                wasCalled = true;
            }
        );

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public void Result_VoidMatch_Ok_CallsCorrectAction()
    {
        // Arrange
        Result<int, string> result = 42;
        var wasCalled = false;
        var value = 0;

        // Act
        result.Match(
            onOk: v =>
            {
                wasCalled = true;
                value = v;
            },
            onError: e =>
            {
                Assert.Fail("Should not call onError");
            }
        );

        // Assert
        Assert.True(wasCalled);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Result_VoidMatch_Error_CallsCorrectAction()
    {
        // Arrange
        Result<int, string> result = "error occurred";
        var wasCalled = false;
        var errorValue = string.Empty;

        // Act
        result.Match(
            onOk: v =>
            {
                Assert.Fail("Should not call onOk");
            },
            onError: e =>
            {
                wasCalled = true;
                errorValue = e;
            }
        );

        // Assert
        Assert.True(wasCalled);
        Assert.Equal("error occurred", errorValue);
    }
}
