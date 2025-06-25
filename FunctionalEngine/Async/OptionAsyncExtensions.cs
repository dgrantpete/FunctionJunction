using System.Collections.Immutable;
using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Async;

/// <summary>
/// Provides asynchronous extension methods for <see cref="Option{T}"/> and related async operations.
/// These methods handle async scenarios like sequencing async options, converting to async enumerables, and combining async option collections.
/// This class is marked as partial to accommodate generated async extension methods.
/// </summary>
public static partial class OptionAsyncExtensions
{
    /// <summary>
    /// Sequences an <see cref="Option{T}"/> containing a <see cref="Task{T}"/> into a <see cref="Task{T}"/> containing an <see cref="Option{T}"/>.
    /// This transforms <c>Option&lt;Task&lt;T&gt;&gt;</c> into <c>Task&lt;Option&lt;T&gt;&gt;</c>, allowing you to await the option's inner task.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns a completed <see cref="Task{T}"/> containing <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value inside the task and option. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> containing a task to sequence.</param>
    /// <returns>A <see cref="Task{T}"/> that completes with an <see cref="Option{T}"/> containing the task's result, or <c>None</c>.</returns>
    public static Task<Option<T>> Sequence<T>(this Option<Task<T>> option) where T : notnull =>
        option.Map(Identity);

    /// <summary>
    /// Asynchronously combines a sequence of <see cref="Option{T}"/> values into a single <see cref="Option{T}"/> containing a list of all values.
    /// If all <see cref="Option{T}"/> values are <c>Some</c>, returns <c>Some</c> containing all values. If any <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>.
    /// This provides "all-or-nothing" semantics for async option collections.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="options">The async sequence of options to combine.</param>
    /// <returns>A <see cref="Task{T}"/> containing either <c>Some</c> with all values, or <c>None</c> if any option was <c>None</c>.</returns>
    public static async Task<Option<ImmutableArray<T>>> All<T>(IAsyncEnumerable<Option<T>> options) where T : notnull
    {
        var values = ImmutableArray.CreateBuilder<T>();

        await foreach (var option in options)
        {
            if (option.TryUnwrap(out var value))
            {
                values.Add(value);
            }
            else
            {
                return default;
            }
        }

        return values.DrainToImmutable();
    }

    /// <summary>
    /// Asynchronously combines options from a collection of async option providers into a single option containing a list of all values.
    /// Each provider function is executed and awaited. If all options are <c>Some</c>, returns <c>Some</c> containing all values.
    /// If any option is <c>None</c>, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="optionProvidersAsync">The collection of async functions that provide options when executed.</param>
    /// <returns>A <see cref="Task{T}"/> containing either <c>Some</c> with all values, or <c>None</c> if any option was <c>None</c>.</returns>
    public static async Task<Option<ImmutableArray<T>>> All<T>(params IEnumerable<Func<Task<Option<T>>>> optionProvidersAsync) where T : notnull
    {
        var values = ImmutableArray.CreateBuilder<T>();

        foreach (var optionProviderAsync in optionProvidersAsync)
        {
            var option = await optionProviderAsync();

            if (option.TryUnwrap(out var value))
            {
                values.Add(value);
            }
            else
            {
                return default;
            }
        }

        return values.DrainToImmutable();
    }
     
    /// <summary>
    /// Asynchronously finds the first <c>Some</c> value from a sequence of <see cref="Option{T}"/> values.
    /// Returns the first <see cref="Option{T}"/> that contains a value, or <c>None</c> if all <see cref="Option{T}"/> values are <c>None</c>.
    /// This provides "first-success" semantics for async option collections.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="options">The async sequence of options to search.</param>
    /// <returns>A <see cref="Task{T}"/> containing the first <c>Some</c> value found, or <c>None</c> if none exist.</returns>
    public static async Task<Option<T>> Any<T>(IAsyncEnumerable<Option<T>> options) where T : notnull =>
        await options.FirstOrDefaultAsync(option => option.IsSome);

    /// <summary>
    /// Asynchronously finds the first <c>Some</c> value from a collection of async option providers.
    /// Each provider function is executed and awaited. Returns the first option that contains a value, or <c>None</c> if all options are <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="optionProvidersAsync">The collection of async functions that provide options when executed.</param>
    /// <returns>A <see cref="Task{T}"/> containing the first <c>Some</c> value found, or <c>None</c> if none exist.</returns>
    public static async Task<Option<T>> Any<T>(params IEnumerable<Func<Task<Option<T>>>> optionProvidersAsync) where T : notnull
    {
        foreach (var optionProviderAsync in optionProvidersAsync)
        {
            var option = await optionProviderAsync();

            if (option.TryUnwrap(out var value))
            {
                return value;
            }
        }

        return default;
    }
}
