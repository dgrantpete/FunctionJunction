using System.ComponentModel;
using static FunctionJunction.Prelude;

namespace FunctionJunction.Async;

/// <summary>
/// Provides asynchronous extension methods for <see cref="Option{T}"/> and related async operations.
/// These methods handle async scenarios like sequencing async options, converting to async enumerables, and combining async option collections.
/// This class is marked as partial to accommodate generated async extension methods.
/// </summary>
public static partial class OptionAsyncExtensions
{
    /// <summary>
    /// Sequences an <see cref="Option{T}"/> containing a <see cref="ValueTask{T}"/> into a <see cref="ValueTask{T}"/> containing an <see cref="Option{T}"/>.
    /// This transforms <c>Option&lt;ValueTask&lt;T&gt;&gt;</c> into <c>ValueTask&lt;Option&lt;T&gt;&gt;</c>, allowing you to await the option's inner task.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns a completed <see cref="ValueTask{T}"/> containing <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value inside the task and option. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> containing a task to sequence.</param>
    /// <returns>A <see cref="ValueTask{T}"/> that completes with an <see cref="Option{T}"/> containing the task's result, or <c>None</c>.</returns>
    public static ValueTask<Option<T>> Sequence<T>(this Option<ValueTask<T>> option) where T : notnull =>
        option.Map(Identity);

    /// <summary>
    /// Converts a <see cref="ValueTask{T}"/> containing an <see cref="Option{T}"/> into an <see cref="IAsyncEnumerable{T}"/>.
    /// If the task's <see cref="Option{T}"/> is <c>Some</c>, yields the single value. If <c>None</c>, yields nothing.
    /// Useful for integrating async options into async enumerable processing pipelines.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="optionTask">The task containing a result to convert.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the success value if present, or nothing if an error.</returns>
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ValueTask<Option<T>> optionTask) where T : notnull
    {
        var option = await optionTask.ConfigureAwait(false);

        if (option.TryUnwrap(out var value))
        {
            yield return value;
        }
    }
}

/// <summary>
/// Asynchronous extension methods specific to <see cref="Option{T}"/> which contain a value type.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class ValueOptionAsyncExtensions
{

}
