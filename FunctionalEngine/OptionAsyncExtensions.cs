using static FunctionalEngine.Functions;

namespace FunctionalEngine;

public static partial class OptionAsyncExtensions
{
    public static Task<Option<T>> SequenceAsync<T>(this Option<Task<T>> option) where T : notnull =>
        option.MapAsync(Identity);

    public static async Task<Option<T>> SomeAsync<T>(Task<T> valueTask) where T : notnull =>
        Option.Some(await valueTask);

    public static async Task<T?> UnwrapNullableAsync<T>(this Task<Option<T>> optionTask) where T : class =>
        (await optionTask).UnwrapNullable();

    public static async Task<T?> UnwrapNullableValueAsync<T>(this Task<Option<T>> optionTask) where T : struct =>
        (await optionTask).UnwrapNullableValue();

    public static async Task<Option<T>> FromNullableAsync<T>(Task<T?> valueTask) where T : class =>
        Option.FromNullable(await valueTask);

    public static async Task<Option<T>> FromNullableAsync<T>(Task<T?> valueTask) where T : struct =>
        Option.FromNullable(await valueTask);
}
