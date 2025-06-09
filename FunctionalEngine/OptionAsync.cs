using static FunctionalEngine.Functions;

namespace FunctionalEngine;

public static class OptionAsync
{
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Option<T>> optionTask,
        Func<T, TResult> onSome,
        Func<TResult> onNone
    ) where T : notnull =>
        (await optionTask).Match(onSome, onNone);

    public static async Task<Option<TResult>> FlatMapAsync<T, TResult>(this Task<Option<T>> optionTask, Func<T, Option<TResult>> mapper)
        where T : notnull
        where TResult : notnull
    =>
        (await optionTask).FlatMap(mapper);

    public static async Task<Option<TResult>> FlatMapAsync<T, TResult>(this Task<Option<T>> optionTask, Func<T, Task<Option<TResult>>> mapperAsync)
        where T : notnull
        where TResult : notnull
    =>
        await (await optionTask).FlatMapAsync(mapperAsync);

    public static async Task<Option<TResult>> MapAsync<T, TResult>(this Task<Option<T>> optionTask, Func<T, TResult> mapper)
        where T : notnull
        where TResult : notnull
    =>
        (await optionTask).Map(mapper);

    public static async Task<Option<TResult>> MapAsync<T, TResult>(this Task<Option<T>> optionTask, Func<T, Task<TResult>> mapperAsync)
        where T : notnull
        where TResult : notnull
    =>
        await (await optionTask).MapAsync(mapperAsync);

    public static async Task<Option<T>> FilterAsync<T>(this Task<Option<T>> optionTask, Func<T, bool> filter) where T : notnull =>
        (await optionTask).Filter(filter);

    public static async Task<Option<T>> FilterAsync<T>(this Task<Option<T>> optionTask, Func<T, Task<bool>> filterAsync) where T : notnull =>
        await (await optionTask).FilterAsync(filterAsync);

    public static async Task<Option<T>> OrAsync<T>(this Task<Option<T>> optionTask, Func<Option<T>> alternateProvider) where T : notnull =>
        (await optionTask).Or(alternateProvider);

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

    public static async Task<Option<T>> FlattenAsync<T>(this Task<Option<Option<T>>> optionTask) where T : notnull =>
        (await optionTask).Flatten();
}
