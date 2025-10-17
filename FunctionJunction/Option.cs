using FunctionJunction.Generator;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static FunctionJunction.Prelude;

namespace FunctionJunction;

/// <summary>
/// Represents an optional value that may or may not exist.
/// An <see cref="Option{T}"/> is either <c>Some(value)</c> containing a value of type <typeparamref name="T"/>, or <c>None</c> indicating no value.
/// This type is useful for avoiding <see langword="null"/> reference exceptions and making the possibility of missing values explicit.
/// </summary>
/// <typeparam name="T">The type of the value that may be contained. Must be non-null.</typeparam>
[GenerateAsyncExtension(ExtensionClassName = "OptionAsyncExtensions", Namespace = "FunctionJunction.Async")]
public readonly record struct Option<T> where T : notnull
{
    private readonly T internalValue;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Option{T}"/> contains a value.
    /// </summary>
    public bool IsSome { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Option{T}"/> does not contain a value.
    /// </summary>
    public bool IsNone => !IsSome;

    internal Option(T value)
    {
        IsSome = true;
        internalValue = value;
    }

    /// <summary>
    /// Implicitly converts a nullable <typeparamref name="T"/> into an <see cref="Option{T}"/>, where the value is <c>Some</c> if it isn't <see langword="null"></see>.
    /// </summary>
    /// <param name="value">The value being converted.</param>
    public static implicit operator Option<T>(T? value) => value switch
    {
        not null => new Option<T>(value),
        null => default
    };

    /// <summary>
    /// Performs pattern matching on the <see cref="Option{T}"/>, executing one of two provided functions based on whether the <see cref="Option{T}"/> contains a value.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by either function.</typeparam>
    /// <param name="onSome">The function to execute if the <see cref="Option{T}"/> contains a value. Receives the contained value as a parameter.</param>
    /// <param name="onNone">The function to execute if the <see cref="Option{T}"/> does not contain a value.</param>
    /// <returns>The result of executing either <paramref name="onSome"/> or <paramref name="onNone"/>.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSome,
        Func<TResult> onNone
    ) =>
        IsSome switch
        {
            true => onSome(internalValue),
            false => onNone()
        };

    /// <summary>
    /// Performs pattern matching on the <see cref="Option{T}"/>, executing one of two provided actions based on whether the <see cref="Option{T}"/> contains a value.
    /// </summary>
    /// <param name="onSome">The action to execute if the <see cref="Option{T}"/> contains a value. Receives the contained value as a parameter.</param>
    /// <param name="onNone">The action to execute if the <see cref="Option{T}"/> does not contain a value.</param>
    public void Match(Action<T> onSome, Action onNone)
    {
        if (IsSome)
        {
            onSome(internalValue);
            return;
        }
        
        onNone();
    }

    /// <summary>
    /// Applies a function that returns an <see cref="Option{T}"/> to the value inside this <see cref="Option{T}"/>, flattening the result.
    /// This is the monadic bind operation for <see cref="Option{T}"/>. If this <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c> without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the <see cref="Option{T}"/> returned by the mapper. Must be non-null.</typeparam>
    /// <param name="mapper">A function that takes the contained value and returns an <c>Option&lt;TResult&gt;</c>.</param>
    /// <returns>The <see cref="Option{T}"/> returned by the mapper if this <see cref="Option{T}"/> is <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public Option<TResult> FlatMap<TResult>(Func<T, Option<TResult>> mapper) where TResult : notnull =>
        Match(
            mapper,
            () => default
        );

    /// <summary>
    /// Asynchronously applies a function that returns a <c>ValueTask&lt;Option&lt;TResult&gt;&gt;</c> to the value inside this <see cref="Option{T}"/>, flattening the result.
    /// If this <see cref="Option{T}"/> is <c>None</c>, returns a completed <see cref="ValueTask"/> containing <c>None</c> without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the <see cref="Option{T}"/> returned by the async mapper. Must be non-null.</typeparam>
    /// <param name="mapperAsync">An async function that takes the contained value and returns a <c>ValueTask&lt;Option&lt;TResult&gt;&gt;</c>.</param>
    /// <returns>A <see cref="ValueTask"/> containing the <see cref="Option{T}"/> returned by the async mapper if this <see cref="Option{T}"/> is <c>Some</c>, otherwise a <see cref="ValueTask"/> containing <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<TResult>> FlatMap<TResult>(Func<T, ValueTask<Option<TResult>>> mapperAsync) where TResult : notnull =>
        Match(
            mapperAsync,
            () => new ValueTask<Option<TResult>>(default(Option<TResult>))
        );

    /// <summary>
    /// Transforms the value inside this <see cref="Option{T}"/> using the provided function.
    /// This is the functor map operation for <see cref="Option{T}"/>. If this <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c> without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed value. Must be non-null.</typeparam>
    /// <param name="mapper">A function that transforms the contained value to a new value of type <typeparamref name="TResult"/>.</param>
    /// <returns>An <see cref="Option{T}"/> containing the transformed value if this <see cref="Option{T}"/> is <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public Option<TResult> Map<TResult>(Func<T, TResult> mapper) where TResult : notnull =>
        FlatMap(value => new Option<TResult>(mapper(value)));

    /// <summary>
    /// Asynchronously transforms the value inside this <see cref="Option{T}"/> using the provided async function.
    /// If this <see cref="Option{T}"/> is <c>None</c>, returns a completed <see cref="ValueTask"/> containing <c>None</c> without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed value. Must be non-null.</typeparam>
    /// <param name="mapperAsync">An async function that transforms the contained value to a new value of type <typeparamref name="TResult"/>.</param>
    /// <returns>A <see cref="ValueTask"/> containing an <see cref="Option{T}"/> with the transformed value if this <see cref="Option{T}"/> is <c>Some</c>, otherwise a <see cref="ValueTask"/> containing <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<TResult>> Map<TResult>(Func<T, ValueTask<TResult>> mapperAsync) where TResult : notnull =>
        FlatMap(async value => new Option<TResult>(await mapperAsync(value).ConfigureAwait(false)));

    /// <summary>
    /// Filters the <see cref="Option{T}"/> based on a predicate. If the <see cref="Option{T}"/> is <c>Some</c> and the predicate returns <see langword="true"/>, returns the original <see cref="Option{T}"/>.
    /// If the <see cref="Option{T}"/> is <c>Some</c> but the predicate returns <see langword="false"/>, or if the <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>.
    /// </summary>
    /// <param name="filter">A predicate function that tests the contained value.</param>
    /// <returns>The original <see cref="Option{T}"/> if it contains a value that satisfies the predicate, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public Option<T> Filter(Func<T, bool> filter) =>
        FlatMap(value => filter(value) switch
        {
            true => new Option<T>(value),
            false => default
        });

    /// <summary>
    /// Asynchronously filters the <see cref="Option{T}"/> based on an async predicate. If the <see cref="Option{T}"/> is <c>Some</c> and the predicate returns <see langword="true"/>, returns the original <see cref="Option{T}"/>.
    /// If the <see cref="Option{T}"/> is <c>Some</c> but the predicate returns <see langword="false"/>, or if the <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>.
    /// </summary>
    /// <param name="filterAsync">An async predicate function that tests the contained value.</param>
    /// <returns>A <see cref="ValueTask"/> containing the original <see cref="Option{T}"/> if it contains a value that satisfies the predicate, otherwise a <see cref="ValueTask"/> containing <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<T>> Filter(Func<T, ValueTask<bool>> filterAsync) =>
        FlatMap(async value => await filterAsync(value).ConfigureAwait(false) switch
        {
            true => new Option<T>(value),
            false => default
        });

    /// <summary>
    /// Returns this <see cref="Option{T}"/> if it contains a value, otherwise returns the <see cref="Option{T}"/> provided by the alternative function.
    /// This operation allows for chaining multiple <see cref="Option{T}"/> sources, returning the first one that contains a value.
    /// </summary>
    /// <param name="otherProvider">A function that provides an alternative <see cref="Option{T}"/> when this <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>This <see cref="Option{T}"/> if it is <c>Some</c>, otherwise the <see cref="Option{T}"/> returned by the alternative provider.</returns>
    [GenerateAsyncExtension]
    public Option<T> Or(Func<Option<T>> otherProvider) =>
        Match(
            value => new(value),
            otherProvider
        );

    /// <summary>
    /// Asynchronously returns this <see cref="Option{T}"/> if it contains a value, otherwise returns the <see cref="Option{T}"/> provided by the async alternative function.
    /// This operation allows for chaining multiple async <see cref="Option{T}"/> sources, returning the first one that contains a value.
    /// </summary>
    /// <param name="otherProviderAsync">An async function that provides an alternative <see cref="Option{T}"/> when this <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>A <see cref="ValueTask"/> containing this <see cref="Option{T}"/> if it is <c>Some</c>, otherwise a <see cref="ValueTask"/> containing the <see cref="Option{T}"/> returned by the async alternative provider.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<T>> Or(Func<ValueTask<Option<T>>> otherProviderAsync) =>
        Match(
            value => new ValueTask<Option<T>>(new Option<T>(value)),
            otherProviderAsync
        );

    /// <summary>
    /// Combines this <see cref="Option{T}"/> with another <see cref="Option{T}"/> if both contain values, returning a tuple of both values.
    /// If either <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>. Useful for combining multiple optional values.
    /// </summary>
    /// <typeparam name="TOther">The type of the value in the other <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="otherProvider">A function that provides another <see cref="Option{T}"/> to combine with this one.</param>
    /// <returns>An <see cref="Option{T}"/> containing a tuple of both values if both are <c>Some</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public Option<(T Left, TOther Right)> And<TOther>(Func<Option<TOther>> otherProvider) where TOther : notnull =>
        FlatMap(value => otherProvider().Map(otherValue => (value, otherValue)));

    /// <summary>
    /// Asynchronously combines this <see cref="Option{T}"/> with another <see cref="Option{T}"/> if both contain values, returning a tuple of both values.
    /// If either <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>. Useful for combining multiple async optional values.
    /// </summary>
    /// <typeparam name="TOther">The type of the value in the other <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="otherProviderAsync">An async function that provides another <see cref="Option{T}"/> to combine with this one.</param>
    /// <returns>A <see cref="ValueTask"/> containing an <see cref="Option{T}"/> with a tuple of both values if both are <c>Some</c>, otherwise a <see cref="ValueTask"/> containing <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<(T Left, TOther Right)>> And<TOther>(Func<ValueTask<Option<TOther>>> otherProviderAsync) where TOther : notnull =>
        FlatMap(async value =>
            (await otherProviderAsync().ConfigureAwait(false))
                .Map(otherValue => (value, otherValue))
        );

    /// <summary>
    /// Performs a side effect on the contained value if the <see cref="Option{T}"/> is <c>Some</c>, then returns the original <see cref="Option{T}"/> unchanged.
    /// Useful for logging, debugging, or performing side effects without modifying the <see cref="Option{T}"/>.
    /// </summary>
    /// <param name="tapper">An action to perform on the contained value if it exists.</param>
    /// <returns>The original <see cref="Option{T}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public Option<T> Tap(Action<T> tapper) =>
        Map(value =>
        {
            tapper(value);
            return value;
        });

    /// <summary>
    /// Asynchronously performs a side effect on the contained value if the <see cref="Option{T}"/> is <c>Some</c>, then returns the original <see cref="Option{T}"/> unchanged.
    /// Useful for async logging, debugging, or performing async side effects without modifying the <see cref="Option{T}"/>.
    /// </summary>
    /// <param name="tapperAsync">An async action to perform on the contained value if it exists.</param>
    /// <returns>A <see cref="ValueTask"/> containing the original <see cref="Option{T}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<T>> Tap(Func<T, ValueTask> tapperAsync) =>
        Map(async value =>
        {
            await tapperAsync(value).ConfigureAwait(false);
            return value;
        });

    /// <summary>
    /// Performs a side effect if the <see cref="Option{T}"/> is <c>None</c>, then returns the original <see cref="Option{T}"/> unchanged.
    /// Useful for logging, debugging, or performing side effects when no value is present.
    /// </summary>
    /// <param name="tapper">An action to perform when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>The original <see cref="Option{T}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public Option<T> TapNone(Action tapper) =>
        Or(() =>
        {
            tapper();
            return Option.None<T>();
        });

    /// <summary>
    /// Asynchronously performs a side effect if the <see cref="Option{T}"/> is <c>None</c>, then returns the original <see cref="Option{T}"/> unchanged.
    /// Useful for async logging, debugging, or performing async side effects when no value is present.
    /// </summary>
    /// <param name="tapperAsync">An async action to perform when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>A <see cref="ValueTask"/> containing the original <see cref="Option{T}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public ValueTask<Option<T>> TapNone(Func<ValueTask> tapperAsync) =>
        Or(async () =>
        {
            await tapperAsync().ConfigureAwait(false);
            return default;
        });

    /// <summary>
    /// Extracts the value from the <see cref="Option{T}"/> if it contains one, otherwise returns the result of the default provider function.
    /// This is a safe way to get a value from an <see cref="Option{T}"/> without risking exceptions.
    /// </summary>
    /// <param name="defaultProvider">A function that provides a default value when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>The contained value if the <see cref="Option{T}"/> is <c>Some</c>, otherwise the result of calling the default provider.</returns>
    [GenerateAsyncExtension]
    public T UnwrapOr(Func<T> defaultProvider) =>
        Match(
            Identity,
            defaultProvider
        );

    /// <summary>
    /// Asynchronously extracts the value from the <see cref="Option{T}"/> if it contains one, otherwise returns the result of the async default provider function.
    /// This is a safe way to get a value from an <see cref="Option{T}"/> without risking exceptions.
    /// </summary>
    /// <param name="defaultProviderAsync">An async function that provides a default value when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>A <see cref="ValueTask"/> containing the contained value if the <see cref="Option{T}"/> is <c>Some</c>, otherwise a <see cref="ValueTask"/> containing the result of calling the async default provider.</returns>
    [GenerateAsyncExtension]
    public ValueTask<T> UnwrapOr(Func<ValueTask<T>> defaultProviderAsync) =>
        Match(
            value => new ValueTask<T>(value),
            defaultProviderAsync
        );

    /// <summary>
    /// Extracts the value from the <see cref="Option{T}"/> if it contains one, otherwise throws an exception provided by the exception provider function.
    /// Useful when you want to treat a <c>None</c> value as an exceptional case with a custom exception.
    /// </summary>
    /// <typeparam name="TException">The type of exception to throw. Must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="exceptionProvider">A function that provides the exception to throw when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>The contained value if the <see cref="Option{T}"/> is <c>Some</c>.</returns>
    /// <exception cref="Exception">Throws the exception provided by <paramref name="exceptionProvider"/> when the <see cref="Option{T}"/> is <c>None</c>.</exception>
    [GenerateAsyncExtension]
    public T UnwrapOrThrow<TException>(Func<TException> exceptionProvider) where TException : Exception =>
        UnwrapOr(T () => throw exceptionProvider());

    /// <summary>
    /// Asynchronously extracts the value from the <see cref="Option{T}"/> if it contains one, otherwise throws an exception provided by the async exception provider function.
    /// Useful when you want to treat a <c>None</c> value as an exceptional case with a custom exception that may need to be created asynchronously.
    /// </summary>
    /// <typeparam name="TException">The type of exception to throw. Must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="exceptionProvider">An async function that provides the exception to throw when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>A <see cref="ValueTask"/> containing the contained value if the <see cref="Option{T}"/> is <c>Some</c>.</returns>
    /// <exception cref="Exception">Throws the exception provided by <paramref name="exceptionProvider"/> when the <see cref="Option{T}"/> is <c>None</c>.</exception>
    [GenerateAsyncExtension]
    public ValueTask<T> UnwrapOrThrow<TException>(Func<ValueTask<TException>> exceptionProvider) where TException : Exception =>
        UnwrapOr(async () => throw await exceptionProvider().ConfigureAwait(false));

    /// <summary>
    /// Extracts the value from the <see cref="Option{T}"/> if it contains one, otherwise throws an <see cref="InvalidOperationException"/> with a default message.
    /// This is a convenience method for when you want to treat a <c>None</c> value as an exceptional case with a standard error message.
    /// </summary>
    /// <returns>The contained value if the <see cref="Option{T}"/> is <c>Some</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="Option{T}"/> is <c>None</c>.</exception>
    [GenerateAsyncExtension]
    public T UnwrapOrThrow() =>
        UnwrapOrThrow(() =>
            new InvalidOperationException($"Could not unwrap 'Option<{typeof(T).Name}>' because it doesn't contain a maybeValue")
        );

    /// <summary>
    /// Filters the <see cref="Option{T}"/> to only contain values that are of the specified type.
    /// If the contained value is of type <typeparamref name="TResult"/>, returns an <see cref="Option{T}"/> containing that value.
    /// Otherwise, returns <c>None</c>. Useful for safe type filtering without exceptions.
    /// </summary>
    /// <typeparam name="TResult">The type to filter for. Must be non-null.</typeparam>
    /// <returns>An <c>Option&lt;TResult&gt;</c> containing the value if it is of type <typeparamref name="TResult"/>, otherwise <c>None</c>.</returns>
    public Option<TResult> OfType<TResult>() where TResult : notnull =>
        FlatMap(value => value switch
        {
            TResult castValue => new Option<TResult>(castValue),
            _ => default
        });

    /// <summary>
    /// Converts the <see cref="Option{T}"/> to an <see cref="IEnumerable{T}"/>.
    /// If the <see cref="Option{T}"/> is <c>Some</c>, returns an enumerable containing the single value.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns an empty enumerable.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the value if <c>Some</c>, or empty if <c>None</c>.</returns>
    public IEnumerable<T> ToEnumerable()
    {
        if (IsSome)
        {
            yield return internalValue;
        }
    }
}

/// <summary>
/// Provides static methods for creating and working with <see cref="Option{T}"/> instances.
/// </summary>
[GenerateAsyncExtension(ExtensionClassName = "OptionAsyncExtensions", Namespace = "FunctionJunction.Async")]
public static class Option
{
    /// <summary>
    /// Creates an <see cref="Option{T}"/> that does not contain a value (<c>None</c>).
    /// </summary>
    /// <typeparam name="T">The type that the <see cref="Option{T}"/> would contain if it had a value. Must be non-null.</typeparam>
    /// <returns>An <see cref="Option{T}"/> representing the absence of a value.</returns>
    public static Option<T> None<T>() where T : notnull => default;

    /// <summary>
    /// Creates an <see cref="Option{T}"/> that contains the specified value (<c>Some</c>).
    /// </summary>
    /// <typeparam name="T">The type of the value to contain. Must be non-null.</typeparam>
    /// <param name="value">The value to wrap in an <see cref="Option{T}"/>.</param>
    /// <returns>An <see cref="Option{T}"/> containing the specified value.</returns>
    public static Option<T> Some<T>(T value) where T : notnull => new(value);

    /// <summary>
    /// Deconstructs an <see cref="Option{T}"/> into a nullable reference type <typeparamref name="T"/>. Enables pattern matching directly on <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value in the <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to be deconstructed.</param>
    /// <param name="value">The nullable value being returned.</param>
    /// <remarks>
    /// <code>
    /// Option&lt;string&gt; referenceOption = ...;
    /// 
    /// if (referenceOption is Option&lt;string&gt;({ } notNullString))
    /// {
    ///     ...
    /// }
    /// </code>
    /// </remarks>
    public static void Deconstruct<T>(this Option<T> option, out T? value) where T : class
    {
        value = null;

        if (option.TryUnwrap(out var maybeValue))
        {
            value = maybeValue;
        }
    }

    /// <summary>
    /// Deconstructs an <see cref="Option{T}"/> into a nullable reference type <see cref="Nullable{T}"/>. Enables pattern matching directly on <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value in the <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to be deconstructed.</param>
    /// <param name="value">The nullable value being returned.</param>
    /// <remarks>
    /// <code>
    /// Option&lt;int&gt; valueOption = ...;
    /// 
    /// if (valueOption is Option&lt;int&gt;({ } notNullInt))
    /// {
    ///     ...
    /// }
    /// </code>
    /// </remarks>
    public static void Deconstruct<T>(this Option<T> option, out T? value) where T : struct
    {
        value = null;

        if (option.TryUnwrap(out var maybeValue))
        {
            value = maybeValue;
        }
    }

    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a <see cref="Result{T, TError}"/>.
    /// If the <see cref="Option{T}"/> is <c>Some</c>, returns an <c>Ok</c> result with the contained value.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns an <c>Error</c> result with the error provided by the error provider function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <typeparam name="TError">The type of the error to use when the <see cref="Option{T}"/> is <c>None</c>.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to convert.</param>
    /// <param name="errorProvider">A function that provides the error value when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>A <see cref="Result{T, TError}"/> representing the <see cref="Option{T}"/> as either an <c>Ok</c> or <c>Error</c>.</returns>
    [GenerateAsyncExtension]
    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, Func<TError> errorProvider)
        where T : notnull
    =>
        option.Match(
            Result.ApplyType<TError>.Ok,
            Compose(errorProvider, Result.ApplyType<T>.Error)
        );

    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a <see cref="Result{TOk, T}"/>, treating the <see cref="Option{T}"/> value as an error case.
    /// If the <see cref="Option{T}"/> is <c>Some</c>, returns an <c>Error</c> result with the contained value.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns an <c>Ok</c> result with the value provided by the ok provider function.
    /// Useful when the presence of a value indicates an error condition.
    /// </summary>
    /// <typeparam name="TOk">The type of the ok value to use when the <see cref="Option{T}"/> is <c>None</c>.</typeparam>
    /// <typeparam name="T">The type of the value in the <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to convert.</param>
    /// <param name="okProvider">A function that provides the ok value when the <see cref="Option{T}"/> is <c>None</c>.</param>
    /// <returns>A <see cref="Result{TOk, T}"/> representing the <see cref="Option{T}"/> with inverted semantics.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, T> ToErrorResult<TOk, T>(this Option<T> option, Func<TOk> okProvider)
        where T : notnull
    =>
        option.Match(
            Result.ApplyType<TOk>.Error,
            Compose(okProvider, Result.ApplyType<T>.Ok)
        );

    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a nullable reference type.
    /// If the <see cref="Option{T}"/> is <c>Some</c>, returns the contained value.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns <see langword="null"/>.
    /// Useful for interoperating with APIs that expect nullable reference types.
    /// </summary>
    /// <typeparam name="T">The reference type contained in the <see cref="Option{T}"/>.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to convert.</param>
    /// <returns>The contained value if <c>Some</c>, otherwise <see langword="null"/>.</returns>
    [GenerateAsyncExtension]
    public static T? UnwrapNullable<T>(this Option<T> option) where T : class =>
        option.Match(
            T? (value) => value,
            () => null
        );

    /// <summary>
    /// Attempts to extract the value from an <see cref="Option{T}"/> using the try pattern.
    /// This provides a safe way to check for and extract values without exceptions, similar to <c>TryParse</c> methods.
    /// </summary>
    /// <typeparam name="T">The type of the value in the <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to attempt to unwrap.</param>
    /// <param name="value">When this method returns, contains the value if the <see cref="Option{T}"/> is <c>Some</c>, otherwise the default value.</param>
    /// <returns><see langword="true"/> if the <see cref="Option{T}"/> contains a value; otherwise, <see langword="false"/>.</returns>
    public static bool TryUnwrap<T>(this Option<T> option, [NotNullWhen(true)] out T? value) where T : notnull
    {
        var isSome = false;
        var maybeValue = default(T?);

        option.Tap(value =>
        {
            isSome = true;
            maybeValue = value;
        });

        value = maybeValue;

        return isSome;
    }

    /// <summary>
    /// Creates an <see cref="Option{T}"/> from a nullable reference type.
    /// If the value is not <see langword="null"/>, returns <c>Some</c> containing the value.
    /// If the value is <see langword="null"/>, returns <c>None</c>.
    /// Useful for converting nullable reference types to the safer <see cref="Option{T}"/> type.
    /// </summary>
    /// <typeparam name="T">The reference type to wrap in an <see cref="Option{T}"/>.</typeparam>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value if not <see langword="null"/>, otherwise <c>None</c>.</returns>
    public static Option<T> FromNullable<T>(T? value) where T : class => value switch
    {
        not null => new(value),
        null => default
    };

    /// <summary>
    /// Asynchronously creates an <see cref="Option{T}"/> from a <see cref="ValueTask"/> containing a nullable reference type.
    /// If the awaited value is not <see langword="null"/>, returns <c>Some</c> containing the value.
    /// If the awaited value is <see langword="null"/>, returns <c>None</c>.
    /// Useful for converting async operations that return nullable reference types to the safer <see cref="Option{T}"/> type.
    /// </summary>
    /// <typeparam name="T">The reference type to wrap in an <see cref="Option{T}"/>.</typeparam>
    /// <param name="valueTask">The task containing the nullable value to convert.</param>
    /// <returns>A <see cref="ValueTask"/> containing an <see cref="Option{T}"/> with the value if not <see langword="null"/>, otherwise <c>None</c>.</returns>
    public static async ValueTask<Option<T>> AwaitFromNullable<T>(ValueTask<T?> valueTask) where T : class => await valueTask.ConfigureAwait(false) switch
    {
        { } someValue => new(someValue),
        null => default
    };

    /// <summary>
    /// Creates an <see cref="Option{T}"/> from a nullable value type.
    /// If the value has a value, returns <c>Some</c> containing the value.
    /// If the value is <see langword="null"/>, returns <c>None</c>.
    /// Useful for converting nullable value types to the safer <see cref="Option{T}"/> type.
    /// </summary>
    /// <typeparam name="T">The value type to wrap in an <see cref="Option{T}"/>.</typeparam>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value if it has a value, otherwise <c>None</c>.</returns>
    public static Option<T> FromNullable<T>(T? value) where T : struct => value switch
    {
        { } someValue => new(someValue),
        null => default
    };

    /// <summary>
    /// Asynchronously creates an <see cref="Option{T}"/> from a <see cref="ValueTask"/> containing a nullable value type.
    /// If the awaited value has a value, returns <c>Some</c> containing the value.
    /// If the awaited value is <see langword="null"/>, returns <c>None</c>.
    /// Useful for converting async operations that return nullable value types to the safer <see cref="Option{T}"/> type.
    /// </summary>
    /// <typeparam name="T">The value type to wrap in an <see cref="Option{T}"/>.</typeparam>
    /// <param name="valueTask">The task containing the nullable value to convert.</param>
    /// <returns>A <see cref="ValueTask"/> containing an <see cref="Option{T}"/> with the value if it has a value, otherwise <c>None</c>.</returns>
    public static async ValueTask<Option<T>> AwaitFromNullable<T>(ValueTask<T?> valueTask) where T : struct => await valueTask.ConfigureAwait(false) switch
    {
        { } someValue => new(someValue),
        null => default
    };

    /// <summary>
    /// Flattens a nested <see cref="Option{T}"/> by removing one level of nesting.
    /// If the outer <see cref="Option{T}"/> is <c>Some</c> containing an inner <see cref="Option{T}"/>, returns the inner <see cref="Option{T}"/>.
    /// If the outer <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>.
    /// Useful for simplifying nested <see cref="Option{T}"/> structures.
    /// </summary>
    /// <typeparam name="T">The type of the value in the inner <see cref="Option{T}"/>. Must be non-null.</typeparam>
    /// <param name="option">The nested <see cref="Option{T}"/> to flatten.</param>
    /// <returns>The flattened <see cref="Option{T}"/>.</returns>
    [GenerateAsyncExtension]
    public static Option<T> Flatten<T>(this Option<Option<T>> option) where T : notnull =>
        option.FlatMap(Identity);

    /// <summary>
    /// Combines multiple <see cref="Option{T}"/> values into a single <see cref="Option{T}"/> containing a list of all values.
    /// If all <see cref="Option{T}"/> values are <c>Some</c>, returns <c>Some</c> containing a list of all values in order.
    /// If any <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>.
    /// Useful for operations that require all optional values to be present.
    /// </summary>
    /// <typeparam name="T">The type of the values in the <see cref="Option{T}"/> collection. Must be non-null.</typeparam>
    /// <param name="options">The collection of <see cref="Option{T}"/> values to combine.</param>
    /// <returns>An <see cref="Option{T}"/> containing a list of all values if all are <c>Some</c>, otherwise <c>None</c>.</returns>
    public static Option<ImmutableArray<T>> All<T>(IEnumerable<Option<T>> options) where T : notnull
    {
        var values = ImmutableArray.CreateBuilder<T>();

        foreach (var option in options)
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

        return values.MoveToImmutable();
    }

    /// <summary>
    /// Combines multiple <see cref="Option{T}"/> provider functions into a single <see cref="Option{T}"/> containing a list of all values.
    /// Each provider function is called to get its <see cref="Option{T}"/>, then all results are combined.
    /// If all providers return <c>Some</c>, returns <c>Some</c> containing a list of all values in order.
    /// If any provider returns <c>None</c>, returns <c>None</c>.
    /// Useful for lazy evaluation of multiple optional computations.
    /// </summary>
    /// <typeparam name="T">The type of the values in the <see cref="Option{T}"/> collection. Must be non-null.</typeparam>
    /// <param name="optionProviders">The collection of functions that provide <see cref="Option{T}"/> values to combine.</param>
    /// <returns>An <see cref="Option{T}"/> containing a list of all values if all providers return <c>Some</c>, otherwise <c>None</c>.</returns>
    public static Option<ImmutableArray<T>> All<T>(params IEnumerable<Func<Option<T>>> optionProviders) where T : notnull =>
        All(optionProviders.Select(Invoke));

    /// <summary>
    /// Returns the first <see cref="Option{T}"/> that contains a value from a collection of <see cref="Option{T}"/> values.
    /// If no <see cref="Option{T}"/> contains a value, returns <c>None</c>.
    /// Useful for finding the first successful result from multiple optional operations.
    /// </summary>
    /// <typeparam name="T">The type of the values in the <see cref="Option{T}"/> collection. Must be non-null.</typeparam>
    /// <param name="options">The collection of <see cref="Option{T}"/> values to search through.</param>
    /// <returns>The first <see cref="Option{T}"/> that is <c>Some</c>, or <c>None</c> if all are <c>None</c>.</returns>
    public static Option<T> Any<T>(IEnumerable<Option<T>> options) where T : notnull =>
        options.FirstOrDefault(option => option.IsSome);

    /// <summary>
    /// Returns the first <see cref="Option{T}"/> that contains a value from a collection of <see cref="Option{T}"/> provider functions.
    /// Each provider function is called in sequence until one returns <c>Some</c>, or all have been tried.
    /// If no provider returns a value, returns <c>None</c>.
    /// Useful for lazy evaluation when trying multiple optional operations in sequence.
    /// </summary>
    /// <typeparam name="T">The type of the values in the <see cref="Option{T}"/> collection. Must be non-null.</typeparam>
    /// <param name="optionProviders">The collection of functions that provide <see cref="Option{T}"/> values to search through.</param>
    /// <returns>The first <see cref="Option{T}"/> that is <c>Some</c>, or <c>None</c> if all providers return <c>None</c>.</returns>
    public static Option<T> Any<T>(params IEnumerable<Func<Option<T>>> optionProviders) where T : notnull =>
        Any(optionProviders.Select(Invoke));

    /// <summary>
    /// Asynchronously combines a sequence of <see cref="Option{T}"/> values into a single <see cref="Option{T}"/> containing a list of all values.
    /// If all <see cref="Option{T}"/> values are <c>Some</c>, returns <c>Some</c> containing all values. If any <see cref="Option{T}"/> is <c>None</c>, returns <c>None</c>.
    /// This provides "all-or-nothing" semantics for async option collections.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="options">The async sequence of options to combine.</param>
    /// <returns>A <see cref="ValueTask{T}"/> containing either <c>Some</c> with all values, or <c>None</c> if any option was <c>None</c>.</returns>
    public static async ValueTask<Option<ImmutableArray<T>>> All<T>(IAsyncEnumerable<Option<T>> options) where T : notnull
    {
        var values = ImmutableArray.CreateBuilder<T>();

        await foreach (var option in options.ConfigureAwait(false))
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

        return values.MoveToImmutable();
    }

    /// <summary>
    /// Asynchronously combines options from a collection of async option providers into a single option containing a list of all values.
    /// Each provider function is executed and awaited. If all options are <c>Some</c>, returns <c>Some</c> containing all values.
    /// If any option is <c>None</c>, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="optionProvidersAsync">The collection of async functions that provide options when executed.</param>
    /// <returns>A <see cref="ValueTask{T}"/> containing either <c>Some</c> with all values, or <c>None</c> if any option was <c>None</c>.</returns>
    public static async ValueTask<Option<ImmutableArray<T>>> All<T>(params IEnumerable<Func<ValueTask<Option<T>>>> optionProvidersAsync) where T : notnull
    {
        var values = ImmutableArray.CreateBuilder<T>();

        foreach (var optionProviderAsync in optionProvidersAsync)
        {
            var option = await optionProviderAsync().ConfigureAwait(false);

            if (option.TryUnwrap(out var value))
            {
                values.Add(value);
            }
            else
            {
                return default;
            }
        }

        return values.MoveToImmutable();
    }

    /// <summary>
    /// Asynchronously finds the first <c>Some</c> value from a sequence of <see cref="Option{T}"/> values.
    /// Returns the first <see cref="Option{T}"/> that contains a value, or <c>None</c> if all <see cref="Option{T}"/> values are <c>None</c>.
    /// This provides "first-success" semantics for async option collections.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="options">The async sequence of options to search.</param>
    /// <returns>A <see cref="ValueTask{T}"/> containing the first <c>Some</c> value found, or <c>None</c> if none exist.</returns>
    public static async ValueTask<Option<T>> Any<T>(IAsyncEnumerable<Option<T>> options) where T : notnull
    {
        await foreach (var option in options.ConfigureAwait(false))
        {
            if (option.IsSome)
            {
                return option;
            }
        }

        return default;
    }

    /// <summary>
    /// Asynchronously finds the first <c>Some</c> value from a collection of async option providers.
    /// Each provider function is executed and awaited. Returns the first option that contains a value, or <c>None</c> if all options are <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options. Must be non-null.</typeparam>
    /// <param name="optionProvidersAsync">The collection of async functions that provide options when executed.</param>
    /// <returns>A <see cref="ValueTask{T}"/> containing the first <c>Some</c> value found, or <c>None</c> if none exist.</returns>
    public static async ValueTask<Option<T>> Any<T>(params IEnumerable<Func<ValueTask<Option<T>>>> optionProvidersAsync) where T : notnull
    {
        foreach (var optionProviderAsync in optionProvidersAsync)
        {
            var option = await optionProviderAsync().ConfigureAwait(false);

            if (option.IsSome)
            {
                return option;
            }
        }

        return default;
    }
}

/// <summary>
/// <para>A static class which houses extension methods for <see cref="Option{T}"/> where the contained value is a <see langword="struct"/>.</para>
/// <para>This exists separately from <see cref="Option"/> so the same method name (i.e. <c>UnwrapNullable</c>) 
/// can have implementations for both value types and reference types without conflicts.</para>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[GenerateAsyncExtension(ExtensionClassName = "ValueOptionAsyncExtensions", Namespace = "FunctionJunction.Async")]
public static class ValueOption
{
    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a nullable value type.
    /// If the <see cref="Option{T}"/> is <c>Some</c>, returns the contained value.
    /// If the <see cref="Option{T}"/> is <c>None</c>, returns <see langword="null"/>.
    /// Useful for interoperating with APIs that expect nullable value types.
    /// </summary>
    /// <typeparam name="T">The value type contained in the <see cref="Option{T}"/>.</typeparam>
    /// <param name="option">The <see cref="Option{T}"/> to convert.</param>
    /// <returns>The contained value if <c>Some</c>, otherwise <see langword="null"/>.</returns>
    [GenerateAsyncExtension]
    public static T? UnwrapNullable<T>(this Option<T> option) where T : struct =>
        option.Match<T?>(
            value => value,
            () => null
        );
}
