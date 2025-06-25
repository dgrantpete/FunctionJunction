namespace FunctionJunction.Tests;

internal static class TestHelpers
{
    public static TError UnwrapErrorOr<TOk, TError>(this Result<TOk, TError> result, Func<TOk, TError> defaultProvider) =>
        result.Match(
            defaultProvider,
            error => error
        );
}
