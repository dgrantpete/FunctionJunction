using FunctionJunction.Generator;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using static FunctionJunction.Prelude;

namespace FunctionJunction;

/// <summary>
/// Represents the result of an operation that may succeed with a value of type <typeparamref name="TOk"/> or fail with an error of type <typeparamref name="TError"/>.
/// A <see cref="Result{TOk, TError}"/> is either <c>Ok</c> containing a successful value, or <c>Error</c> containing an error value.
/// This type is useful for explicit error handling without exceptions, making error cases visible in the type system.
/// </summary>
/// <typeparam name="TOk">The type of the successful value.</typeparam>
/// <typeparam name="TError">The type of the error value.</typeparam>
[DiscriminatedUnion]
[GenerateAsyncExtension(ExtensionClassName = "ResultAsyncExtensions", Namespace = "FunctionJunction.Async")]
public abstract partial record Result<TOk, TError>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="Result{TOk, TError}"/> represents a successful operation.
    /// </summary>
    public bool IsOk => this is Ok;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Result{TOk, TError}"/> represents a failed operation.
    /// </summary>
    public bool IsError => !IsOk;

    /// <summary>
    /// Represents a successful result containing a value of type <typeparamref name="TOk"/>.
    /// </summary>
    public record Ok : Result<TOk, TError>
    {
        /// <summary>
        /// Gets the successful value contained in this <c>Ok</c> result.
        /// </summary>
        public TOk Value { get; internal init; }

        internal Ok(TOk value)
        {
            Value = value;
        }

        /// <summary>
        /// Deconstructs this <c>Ok</c> result into its contained value.
        /// </summary>
        /// <param name="value">The successful value contained in this result.</param>
        public void Deconstruct(out TOk value)
        {
            value = Value;
        }
    }

    /// <summary>
    /// Represents a failed result containing an error value of type <typeparamref name="TError"/>.
    /// </summary>
    public record Error : Result<TOk, TError>
    {
        /// <summary>
        /// Gets the error value contained in this <c>Error</c> result.
        /// </summary>
        public TError Value { get; internal init; }

        internal Error(TError value)
        {
            Value = value;
        }
        
        /// <summary>
        /// Deconstructs this <c>Error</c> result into its contained error value.
        /// </summary>
        /// <param name="value">The error value contained in this result.</param>
        public void Deconstruct(out TError value)
        {
            value = Value;
        }
    }

    /// <summary>
    /// Implicitly converts a <typeparamref name="TOk"/> into an <c>Ok</c> value inside a <see cref="Result{TOk, TError}"/>.
    /// </summary>
    /// <param name="ok">The value being converted.</param>
    public static implicit operator Result<TOk, TError>(TOk ok) => 
        new Result<TOk, TError>.Ok(ok);

    /// <summary>
    /// Implicitly converts a <typeparamref name="TError"/> into an <c>Error</c> value inside a <see cref="Result{TOk, TError}"/>.
    /// </summary>
    /// <param name="error">The value being converted.</param>
    public static implicit operator Result<TOk, TError>(TError error) => 
        new Result<TOk, TError>.Error(error);

    /// <summary>
    /// Applies a function that returns a <see cref="Result{TOk, TError}"/> to the success value inside this <see cref="Result{TOk, TError}"/>, flattening the result.
    /// This is the monadic bind operation for <see cref="Result{TOk, TError}"/>. If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns the error without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the success value in the <see cref="Result{TOk, TError}"/> returned by the mapper.</typeparam>
    /// <param name="mapper">A function that takes the success value and returns a <c>Result&lt;TResult, TError&gt;</c>.</param>
    /// <returns>The <see cref="Result{TOk, TError}"/> returned by the mapper if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public Result<TResult, TError> FlatMap<TResult>(Func<TOk, Result<TResult, TError>> mapper) =>
        Match(
            mapper,
            error => new Result<TResult, TError>.Error(error)
        );

    /// <summary>
    /// Asynchronously applies a function that returns a <c>Task&lt;Result&lt;TResult, TError&gt;&gt;</c> to the success value inside this <see cref="Result{TOk, TError}"/>, flattening the result.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns a completed <see cref="Task"/> containing the error without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the success value in the <see cref="Result{TOk, TError}"/> returned by the async mapper.</typeparam>
    /// <param name="mapperAsync">An async function that takes the success value and returns a <c>Task&lt;Result&lt;TResult, TError&gt;&gt;</c>.</param>
    /// <returns>A <see cref="Task"/> containing the <see cref="Result{TOk, TError}"/> returned by the async mapper if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise a <see cref="Task"/> containing the original error.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TResult, TError>> FlatMap<TResult>(Func<TOk, Task<Result<TResult, TError>>> mapperAsync) =>
        Match(
            mapperAsync,
            error => Task.FromResult<Result<TResult, TError>>(new Result<TResult, TError>.Error(error))
        );

    /// <summary>
    /// Transforms the success value inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// This is the functor map operation for <see cref="Result{TOk, TError}"/>. If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns the error without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed success value.</typeparam>
    /// <param name="mapper">A function that transforms the success value to a new value of type <typeparamref name="TResult"/>.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the transformed success value if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public Result<TResult, TError> Map<TResult>(Func<TOk, TResult> mapper) =>
        FlatMap(ok => 
            NewOk(mapper(ok))
        );

    /// <summary>
    /// Asynchronously transforms the success value inside this <see cref="Result{TOk, TError}"/> using the provided async function.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns a completed <see cref="Task"/> containing the error without calling the mapper.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed success value.</typeparam>
    /// <param name="mapperAsync">An async function that transforms the success value to a new value of type <typeparamref name="TResult"/>.</param>
    /// <returns>A <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with the transformed success value if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise a <see cref="Task"/> containing the original error.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TResult, TError>> Map<TResult>(Func<TOk, Task<TResult>> mapperAsync) =>
        FlatMap(async ok => NewOk(await mapperAsync(ok)));

    /// <summary>
    /// Attempts to recover from an error by applying a function that may produce a new <see cref="Result{TOk, TError}"/>.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value unchanged. If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, applies the recoverer function to the error value.
    /// This allows for error handling and potential recovery from failed operations.
    /// </summary>
    /// <typeparam name="TResult">The type of the error value in the <see cref="Result{TOk, TError}"/> returned by the recoverer.</typeparam>
    /// <param name="recoverer">A function that takes the error value and attempts to produce a successful result or a new error.</param>
    /// <returns>The original success value if <c>Ok</c>, otherwise the <see cref="Result{TOk, TError}"/> returned by the recoverer function.</returns>
    [GenerateAsyncExtension]
    public Result<TOk, TResult> Recover<TResult>(Func<TError, Result<TOk, TResult>> recoverer) =>
        Match(
            ok => new Result<TOk, TResult>.Ok(ok),
            recoverer
        );

    /// <summary>
    /// Asynchronously attempts to recover from an error by applying an async function that may produce a new <see cref="Result{TOk, TError}"/>.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value unchanged. If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, applies the async recoverer function to the error value.
    /// This allows for async error handling and potential recovery from failed operations.
    /// </summary>
    /// <typeparam name="TResult">The type of the error value in the <see cref="Result{TOk, TError}"/> returned by the async recoverer.</typeparam>
    /// <param name="recovererAsync">An async function that takes the error value and attempts to produce a successful result or a new error.</param>
    /// <returns>A <see cref="Task"/> containing the original success value if <c>Ok</c>, otherwise a <see cref="Task"/> containing the <see cref="Result{TOk, TError}"/> returned by the async recoverer function.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TOk, TResult>> Recover<TResult>(Func<TError, Task<Result<TOk, TResult>>> recovererAsync) =>
        Match(
            ok => Task.FromResult<Result<TOk, TResult>>(new Result<TOk, TResult>.Ok(ok)),
            recovererAsync
        );

    /// <summary>
    /// Transforms the error value inside this <see cref="Result{TOk, TError}"/> using the provided function.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value unchanged. If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, applies the mapper to the error value.
    /// Useful for converting error types or adding additional context to errors.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed error value.</typeparam>
    /// <param name="mapper">A function that transforms the error value to a new value of type <typeparamref name="TResult"/>.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the original success value if <c>Ok</c>, otherwise a <see cref="Result{TOk, TError}"/> containing the transformed error value.</returns>
    [GenerateAsyncExtension]
    public Result<TOk, TResult> MapError<TResult>(Func<TError, TResult> mapper) =>
        Recover(error =>
            NewError(mapper(error))
        );

    /// <summary>
    /// Asynchronously transforms the error value inside this <see cref="Result{TOk, TError}"/> using the provided async function.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value unchanged. If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, applies the async mapper to the error value.
    /// Useful for async error transformation or adding context that requires async operations.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed error value.</typeparam>
    /// <param name="mapperAsync">An async function that transforms the error value to a new value of type <typeparamref name="TResult"/>.</param>
    /// <returns>A <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with the original success value if <c>Ok</c>, otherwise a <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with the transformed error value.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TOk, TResult>> MapError<TResult>(Func<TError, Task<TResult>> mapperAsync) =>
        Recover(async error =>
            NewError(await mapperAsync(error))
        );

    /// <summary>
    /// Validates the success value using a predicate function, converting the <see cref="Result{TOk, TError}"/> to an error if validation fails.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns the error unchanged. If this <see cref="Result{TOk, TError}"/> is <c>Ok</c> and the validator returns <see langword="true"/>, returns the original success value.
    /// If the validator returns <see langword="false"/>, converts the success value to an error using the error mapper.
    /// </summary>
    /// <param name="validator">A predicate function that tests the success value.</param>
    /// <param name="errorMapper">A function that converts the success value to an error when validation fails.</param>
    /// <returns>The original <see cref="Result{TOk, TError}"/> if validation passes or if already an error, otherwise a new error <see cref="Result{TOk, TError}"/>.</returns>
    [GenerateAsyncExtension]
    public Result<TOk, TError> Validate(Func<TOk, bool> validator, Func<TOk, TError> errorMapper) =>
        FlatMap(ok => validator(ok) switch
        {
            true => this,
            false => NewError(errorMapper(ok))
        });

    /// <summary>
    /// Asynchronously validates the success value using an async predicate function, converting the <see cref="Result{TOk, TError}"/> to an error if validation fails.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns the error unchanged. If this <see cref="Result{TOk, TError}"/> is <c>Ok</c> and the async validator returns <see langword="true"/>, returns the original success value.
    /// If the async validator returns <see langword="false"/>, converts the success value to an error using the async error mapper.
    /// </summary>
    /// <param name="validatorAsync">An async predicate function that tests the success value.</param>
    /// <param name="errorMapperAsync">An async function that converts the success value to an error when validation fails.</param>
    /// <returns>A <see cref="Task"/> containing the original <see cref="Result{TOk, TError}"/> if validation passes or if already an error, otherwise a <see cref="Task"/> containing a new error <see cref="Result{TOk, TError}"/>.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TOk, TError>> Validate(Func<TOk, Task<bool>> validatorAsync, Func<TOk, Task<TError>> errorMapperAsync) =>
        FlatMap(async ok => await validatorAsync(ok) switch
        {
            true => this,
            false => NewError(await errorMapperAsync(ok))
        });

    /// <summary>
    /// Combines this <see cref="Result{TOk, TError}"/> with another <see cref="Result{TOk, TError}"/> if both are successful, returning a tuple of both success values.
    /// If either <see cref="Result{TOk, TError}"/> is an error, returns the first error encountered. Useful for combining multiple operations that must all succeed.
    /// </summary>
    /// <typeparam name="TOther">The type of the success value in the other <see cref="Result{TOk, TError}"/>.</typeparam>
    /// <param name="otherProvider">A function that provides another <see cref="Result{TOk, TError}"/> to combine with this one.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing a tuple of both success values if both are <c>Ok</c>, otherwise the first error.</returns>
    [GenerateAsyncExtension]
    public Result<(TOk Left, TOther Right), TError> And<TOther>(Func<Result<TOther, TError>> otherProvider) =>
        FlatMap(ok => otherProvider().Map(otherOk => (ok, otherOk)));

    /// <summary>
    /// Asynchronously combines this <see cref="Result{TOk, TError}"/> with another <see cref="Result{TOk, TError}"/> if both are successful, returning a tuple of both success values.
    /// If either <see cref="Result{TOk, TError}"/> is an error, returns the first error encountered. Useful for combining multiple async operations that must all succeed.
    /// </summary>
    /// <typeparam name="TOther">The type of the success value in the other <see cref="Result{TOk, TError}"/>.</typeparam>
    /// <param name="otherProviderAsync">An async function that provides another <see cref="Result{TOk, TError}"/> to combine with this one.</param>
    /// <returns>A <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with a tuple of both success values if both are <c>Ok</c>, otherwise a <see cref="Task"/> containing the first error.</returns>
    [GenerateAsyncExtension]
    public Task<Result<(TOk Left, TOther Right), TError>> And<TOther>(Func<Task<Result<TOther, TError>>> otherProviderAsync) =>
        FlatMap(async ok => (await otherProviderAsync()).Map(otherOk => (ok, otherOk)));

    /// <summary>
    /// Returns this <see cref="Result{TOk, TError}"/> if it is successful, otherwise attempts to recover using another <see cref="Result{TOk, TError}"/> with a different error type.
    /// If both results are errors, returns an error containing a tuple of both error values. Useful for trying alternative operations when the first fails.
    /// </summary>
    /// <typeparam name="TOther">The type of the error value in the alternative <see cref="Result{TOk, TError}"/>.</typeparam>
    /// <param name="otherProvider">A function that provides an alternative <see cref="Result{TOk, TError}"/> when this <see cref="Result{TOk, TError}"/> is an error.</param>
    /// <returns>This <see cref="Result{TOk, TError}"/> if it is <c>Ok</c>, the alternative result if it is <c>Ok</c>, otherwise a <see cref="Result{TOk, TError}"/> containing a tuple of both errors.</returns>
    [GenerateAsyncExtension]
    public Result<TOk, (TError Left, TOther Right)> Or<TOther>(Func<Result<TOk, TOther>> otherProvider) =>
        Recover(error => otherProvider().MapError(otherError => (error, otherError)));

    /// <summary>
    /// Asynchronously returns this <see cref="Result{TOk, TError}"/> if it is successful, otherwise attempts to recover using another async <see cref="Result{TOk, TError}"/> with a different error type.
    /// If both results are errors, returns an error containing a tuple of both error values. Useful for trying alternative async operations when the first fails.
    /// </summary>
    /// <typeparam name="TOther">The type of the error value in the alternative <see cref="Result{TOk, TError}"/>.</typeparam>
    /// <param name="otherProviderAsync">An async function that provides an alternative <see cref="Result{TOk, TError}"/> when this <see cref="Result{TOk, TError}"/> is an error.</param>
    /// <returns>A <see cref="Task"/> containing this <see cref="Result{TOk, TError}"/> if it is <c>Ok</c>, the alternative result if it is <c>Ok</c>, otherwise a <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with a tuple of both errors.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TOk, (TError Left, TOther Right)>> Or<TOther>(Func<Task<Result<TOk, TOther>>> otherProviderAsync) =>
        Recover(async error => (await otherProviderAsync()).MapError(otherError => (error, otherError)));

    /// <summary>
    /// Swaps the success and error types of this <see cref="Result{TOk, TError}"/>.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns an <c>Error</c> containing the success value.
    /// If this <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns an <c>Ok</c> containing the error value.
    /// Useful when you need to invert the semantics of success and failure.
    /// </summary>
    /// <returns>A <c>Result&lt;TError, TOk&gt;</c> with the success and error types swapped.</returns>
    [GenerateAsyncExtension]
    public Result<TError, TOk> Swap() =>
        Match<Result<TError, TOk>>(
            value => new Result<TError, TOk>.Error(value),
            value => new Result<TError, TOk>.Ok(value)
        );

    /// <summary>
    /// Performs a side effect on the success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, then returns the original <see cref="Result{TOk, TError}"/> unchanged.
    /// Useful for logging, debugging, or performing side effects without modifying the <see cref="Result{TOk, TError}"/>.
    /// </summary>
    /// <param name="tapper">An action to perform on the success value if it exists.</param>
    /// <returns>The original <see cref="Result{TOk, TError}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public Result<TOk, TError> Tap(Action<TOk> tapper) =>
        Map(ok =>
        {
            tapper(ok);
            return ok;
        });

    /// <summary>
    /// Asynchronously performs a side effect on the success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, then returns the original <see cref="Result{TOk, TError}"/> unchanged.
    /// Useful for async logging, debugging, or performing async side effects without modifying the <see cref="Result{TOk, TError}"/>.
    /// </summary>
    /// <param name="tapperAsync">An async action to perform on the success value if it exists.</param>
    /// <returns>A <see cref="Task"/> containing the original <see cref="Result{TOk, TError}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TOk, TError>> Tap(Func<TOk, Task> tapperAsync) =>
        Map(async ok =>
        {
            await tapperAsync(ok);
            return ok;
        });

    /// <summary>
    /// Performs a side effect on the error value if the <see cref="Result{TOk, TError}"/> is <c>Error</c>, then returns the original <see cref="Result{TOk, TError}"/> unchanged.
    /// Useful for logging, debugging, or performing side effects on errors without modifying the <see cref="Result{TOk, TError}"/>.
    /// </summary>
    /// <param name="tapper">An action to perform on the error value if it exists.</param>
    /// <returns>The original <see cref="Result{TOk, TError}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public Result<TOk, TError> TapError(Action<TError> tapper) =>
        MapError(error =>
        {
            tapper(error);
            return error;
        });

    /// <summary>
    /// Asynchronously performs a side effect on the error value if the <see cref="Result{TOk, TError}"/> is <c>Error</c>, then returns the original <see cref="Result{TOk, TError}"/> unchanged.
    /// Useful for async logging, debugging, or performing async side effects on errors without modifying the <see cref="Result{TOk, TError}"/>.
    /// </summary>
    /// <param name="tapperAsync">An async action to perform on the error value if it exists.</param>
    /// <returns>A <see cref="Task"/> containing the original <see cref="Result{TOk, TError}"/> unchanged.</returns>
    [GenerateAsyncExtension]
    public Task<Result<TOk, TError>> TapError(Func<TError, Task> tapperAsync) =>
        MapError(async error =>
        {
            await tapperAsync(error);
            return error;
        });

    /// <summary>
    /// Extracts the success value from the <see cref="Result{TOk, TError}"/> if it contains one, otherwise transforms the error using the default provider function.
    /// This is a safe way to get a value from a <see cref="Result{TOk, TError}"/> without risking exceptions.
    /// </summary>
    /// <param name="defaultProvider">A function that transforms the error value to a success value when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</param>
    /// <returns>The success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the result of calling the default provider with the error.</returns>
    [GenerateAsyncExtension]
    public TOk UnwrapOr(Func<TError, TOk> defaultProvider) =>
        Match(
            Identity,
            defaultProvider
        );

    /// <summary>
    /// Asynchronously extracts the success value from the <see cref="Result{TOk, TError}"/> if it contains one, otherwise transforms the error using the async default provider function.
    /// This is a safe way to get a value from a <see cref="Result{TOk, TError}"/> without risking exceptions.
    /// </summary>
    /// <param name="defaultProviderAsync">An async function that transforms the error value to a success value when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</param>
    /// <returns>A <see cref="Task"/> containing the success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise a <see cref="Task"/> containing the result of calling the async default provider with the error.</returns>
    [GenerateAsyncExtension]
    public Task<TOk> UnwrapOr(Func<TError, Task<TOk>> defaultProviderAsync) =>
        Match(
            Task.FromResult,
            defaultProviderAsync
        );

    /// <summary>
    /// Extracts the success value from the <see cref="Result{TOk, TError}"/> if it contains one, otherwise throws an exception created from the error using the exception provider function.
    /// Useful when you want to treat an error as an exceptional case with a custom exception.
    /// </summary>
    /// <typeparam name="TException">The type of exception to throw. Must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="exceptionProvider">A function that creates an exception from the error value when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</param>
    /// <returns>The success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>.</returns>
    /// <exception cref="Exception">Throws the exception created by <paramref name="exceptionProvider"/> when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</exception>
    [GenerateAsyncExtension]
    public TOk UnwrapOrThrow<TException>(Func<TError, TException> exceptionProvider) where TException : Exception =>
        UnwrapOr(TOk (error) => throw exceptionProvider(error));

    /// <summary>
    /// Asynchronously extracts the success value from the <see cref="Result{TOk, TError}"/> if it contains one, otherwise throws an exception created from the error using the async exception provider function.
    /// Useful when you want to treat an error as an exceptional case with a custom exception that may need to be created asynchronously.
    /// </summary>
    /// <typeparam name="TException">The type of exception to throw. Must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="exceptionProviderAsync">An async function that creates an exception from the error value when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</param>
    /// <returns>A <see cref="Task"/> containing the success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>.</returns>
    /// <exception cref="Exception">Throws the exception created by <paramref name="exceptionProviderAsync"/> when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</exception>
    [GenerateAsyncExtension]
    public Task<TOk> UnwrapOrThrow<TException>(Func<TError, Task<TException>> exceptionProviderAsync) where TException : Exception =>
        UnwrapOr(async error => throw await exceptionProviderAsync(error));

    /// <summary>
    /// Safely casts the success value to the specified type if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>.
    /// The generic constraints ensure that the cast is type-safe. If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns the error unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type to cast the success value to. Must be non-null and the current success type must be assignable to it.</typeparam>
    /// <returns>A <c>Result&lt;TResult, TError&gt;</c> containing the cast success value if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the original error.</returns>
    public Result<TResult, TError> Cast<TResult>() where TResult : notnull, TOk =>
        Map(ok => (TResult)ok!);

    /// <summary>
    /// Safely casts the error value to the specified type if the <see cref="Result{TOk, TError}"/> is <c>Error</c>.
    /// The generic constraints ensure that the cast is type-safe. If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type to cast the error value to. Must be non-null and the current error type must be assignable to it.</typeparam>
    /// <returns>A <c>Result&lt;TOk, TResult&gt;</c> containing the original success value if this <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the cast error value.</returns>
    public Result<TOk, TResult> CastError<TResult>() where TResult : notnull, TError =>
        MapError(error => (TResult)error!);

    /// <summary>
    /// Converts the <see cref="Result{TOk, TError}"/> to an <see cref="IEnumerable{T}"/> of success values.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns an enumerable containing the single success value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns an empty enumerable.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the success value if <c>Ok</c>, or empty if <c>Error</c>.</returns>
    public IEnumerable<TOk> ToEnumerable()
    {
        if (this is Ok(var ok))
        {
            yield return ok;
        }
    }

    /// <summary>
    /// Converts the <see cref="Result{TOk, TError}"/> to an <see cref="IEnumerable{T}"/> of error values.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns an enumerable containing the single error value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns an empty enumerable.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the error value if <c>Error</c>, or empty if <c>Ok</c>.</returns>
    public IEnumerable<TError> ToErrorEnumerable()
    {
        if (this is Error(var error))
        {
            yield return error;
        }
    }

    private static Result<TResult, TError> NewOk<TResult>(TResult value) =>
        new Result<TResult, TError>.Ok(value);

    private static Result<TOk, TResult> NewError<TResult>(TResult value) =>
        new Result<TOk, TResult>.Error(value);
}

/// <summary>
/// Provides static methods for creating and working with <see cref="Result{TOk, TError}"/> instances.
/// </summary>
[GenerateAsyncExtension(ExtensionClassName = "ResultAsyncExtensions", Namespace = "FunctionJunction.Async")]
public static class Result
{
    /// <summary>
    /// Provides helper methods for creating <see cref="Result{TOk, TError}"/> instances when one of the generic types is partially specified.
    /// Useful for functional composition where you need to specify one type parameter while leaving the other to be inferred.
    /// </summary>
    /// <typeparam name="TPartial">The partially specified type parameter.</typeparam>
    public static class ApplyType<TPartial>
    {
        /// <summary>
        /// Creates an error <see cref="Result{TOk, TError}"/> with the specified error value and the success type set to <typeparamref name="TPartial"/>.
        /// </summary>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="error">The error value to wrap in the <see cref="Result{TOk, TError}"/>.</param>
        /// <returns>A <c>Result&lt;TPartial, TError&gt;</c> containing the error value.</returns>
        public static Result<TPartial, TError> Error<TError>(TError error) =>
            new Result<TPartial, TError>.Error(error);

        /// <summary>
        /// Creates a successful <see cref="Result{TOk, TError}"/> with the specified success value and the error type set to <typeparamref name="TPartial"/>.
        /// </summary>
        /// <typeparam name="TOk">The type of the success value.</typeparam>
        /// <param name="ok">The success value to wrap in the <see cref="Result{TOk, TError}"/>.</param>
        /// <returns>A <c>Result&lt;TOk, TPartial&gt;</c> containing the success value.</returns>
        public static Result<TOk, TPartial> Ok<TOk>(TOk ok) =>
            new Result<TOk, TPartial>.Ok(ok);
    }

    /// <summary>
    /// Creates a successful <see cref="Result{TOk, TError}"/> containing the specified value.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="ok">The success value to wrap in the <see cref="Result{TOk, TError}"/>.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the success value.</returns>
    public static Result<TOk, TError> Ok<TOk, TError>(TOk ok) => new Result<TOk, TError>.Ok(ok);

    /// <summary>
    /// Creates an error <see cref="Result{TOk, TError}"/> containing the specified error value.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="error">The error value to wrap in the <see cref="Result{TOk, TError}"/>.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing the error value.</returns>
    public static Result<TOk, TError> Error<TOk, TError>(TError error) => new Result<TOk, TError>.Error(error);

    /// <summary>
    /// Converts a <see cref="Result{TOk, TError}"/> to an <see cref="Option{T}"/> of the success value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns <c>Some</c> containing the success value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns <c>None</c>.
    /// Useful for when you want to ignore the error and just work with optional success values.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value. Must be non-null.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to convert.</param>
    /// <returns>An <see cref="Option{T}"/> containing the success value if <c>Ok</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TOk> ToOption<TOk, TError>(this Result<TOk, TError> result) where TOk : notnull =>
        result.Match(
            Option.Some,
            _ => default
        );

    /// <summary>
    /// Converts a <see cref="Result{TOk, TError}"/> to an <see cref="Option{T}"/> of the error value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns <c>Some</c> containing the error value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns <c>None</c>.
    /// Useful for when you want to ignore the success and just work with optional error values.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be non-null.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to convert.</param>
    /// <returns>An <see cref="Option{T}"/> containing the error value if <c>Error</c>, otherwise <c>None</c>.</returns>
    [GenerateAsyncExtension]
    public static Option<TError> ToErrorOption<TOk, TError>(this Result<TOk, TError> result) where TError : notnull =>
        result.Match(
            _ => default,
            Option.Some
        );

    /// <summary>
    /// Validates the success value using a function that returns an optional error.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns the error unchanged. If the <see cref="Result{TOk, TError}"/> is <c>Ok</c> and the validator returns <c>None</c>, returns the original success value.
    /// If the validator returns <c>Some</c> containing an error, converts the <see cref="Result{TOk, TError}"/> to an error.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be non-null.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to validate.</param>
    /// <param name="validator">A function that validates the success value and returns an optional error.</param>
    /// <returns>The original <see cref="Result{TOk, TError}"/> if validation passes or if already an error, otherwise a new error <see cref="Result{TOk, TError}"/>.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TError> Validate<TOk, TError>(this Result<TOk, TError> result, Func<TOk, Option<TError>> validator)
        where TError : notnull
    =>
        result.FlatMap(ok =>
            validator(ok).ToErrorResult(() => ok)
        );

    /// <summary>
    /// Attempts to recover from an error using a function that returns an optional success value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value unchanged. If the <see cref="Result{TOk, TError}"/> is <c>Error</c> and the recoverer returns <c>Some</c>, converts to a success.
    /// If the recoverer returns <c>None</c>, keeps the original error.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value. Must be non-null.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to attempt recovery on.</param>
    /// <param name="recoverer">A function that attempts to recover from the error value and returns an optional success value.</param>
    /// <returns>The original success if <c>Ok</c>, a new success if recovery succeeds, otherwise the original error.</returns>
    [GenerateAsyncExtension]
    public static Result<TOk, TError> Recover<TOk, TError>(this Result<TOk, TError> result, Func<TError, Option<TOk>> recoverer)
        where TOk : notnull
    =>
        result.Recover(error =>
            recoverer(error).ToResult(() => error)
        );

    /// <summary>
    /// Converts a <see cref="Result{TOk, TError}"/> to a nullable reference type.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns <see langword="null"/>.
    /// Useful for interoperating with APIs that expect nullable reference types.
    /// </summary>
    /// <typeparam name="TOk">The reference type to extract from the <see cref="Result{TOk, TError}"/>.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to convert.</param>
    /// <returns>The success value if <c>Ok</c>, otherwise <see langword="null"/>.</returns>
    [GenerateAsyncExtension]
    public static TOk? UnwrapNullable<TOk, TError>(this Result<TOk, TError> result) where TOk : class =>
        result.Match(
            value => value,
            TOk? (_) => null
        );

    /// <summary>
    /// Converts a <see cref="Result{TOk, TError}"/> to a nullable value type.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, returns the success value.
    /// If the <see cref="Result{TOk, TError}"/> is <c>Error</c>, returns <see langword="null"/>.
    /// Useful for interoperating with APIs that expect nullable value types.
    /// </summary>
    /// <typeparam name="TOk">The value type to extract from the <see cref="Result{TOk, TError}"/>.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to convert.</param>
    /// <returns>The success value if <c>Ok</c>, otherwise <see langword="null"/>.</returns>
    [GenerateAsyncExtension]
    public static TOk? UnwrapNullableValue<TOk, TError>(this Result<TOk, TError> result) where TOk : struct =>
        result.Match(
            value => value,
            TOk? (_) => null
        );

    /// <summary>
    /// Extracts the success value from the <see cref="Result{TOk, TError}"/> if it contains one, otherwise throws an <see cref="InvalidOperationException"/> with a default message.
    /// This is a convenience method for when you want to treat an error as an exceptional case with a standard error message that includes the error details.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to extract the value from.</param>
    /// <returns>The success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</exception>
    [GenerateAsyncExtension]
    public static TOk UnwrapOrThrow<TOk, TError>(this Result<TOk, TError> result) =>
        result.UnwrapOrThrow(error =>
            new InvalidOperationException($"Could not unwrap 'Result<{typeof(TOk).Name}, {typeof(TError).Name}>' because it contains an error: {error}")
        );

    /// <summary>
    /// Extracts the success value from the <see cref="Result{TOk, TError}"/> if it contains one, otherwise throws the error value directly as an exception.
    /// Useful when the error type is already an exception and you want to throw it directly.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TException">The exception type that serves as the error value. Must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to extract the value from.</param>
    /// <returns>The success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>.</returns>
    /// <exception cref="Exception">Throws the error value directly when the <see cref="Result{TOk, TError}"/> is <c>Error</c>.</exception>
    [GenerateAsyncExtension]
    public static TOk UnwrapOrThrowException<TOk, TException>(this Result<TOk, TException> result) where TException : Exception =>
        result.UnwrapOrThrow(Identity);

    /// <summary>
    /// Attempts to extract the success value from a <see cref="Result{TOk, TError}"/> using the try pattern.
    /// This provides a safe way to check for and extract success values without exceptions, similar to <c>TryParse</c> methods.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to attempt to unwrap.</param>
    /// <param name="ok">When this method returns, contains the success value if the <see cref="Result{TOk, TError}"/> is <c>Ok</c>, otherwise the default value.</param>
    /// <returns><see langword="true"/> if the <see cref="Result{TOk, TError}"/> contains a success value; otherwise, <see langword="false"/>.</returns>
    public static bool TryUnwrap<TOk, TError>(this Result<TOk, TError> result, [NotNullWhen(true)] out TOk? ok)
    {
        var isOk = false;
        var maybeOk = default(TOk?);

        result.Tap(ok =>
        {
            isOk = true;
            maybeOk = ok;
        });

        ok = maybeOk;

        return isOk;
    }

    /// <summary>
    /// Attempts to extract the error value from a <see cref="Result{TOk, TError}"/> using the try pattern.
    /// This provides a safe way to check for and extract error values without exceptions, complementing <see cref="TryUnwrap{TOk, TError}"/>.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The <see cref="Result{TOk, TError}"/> to attempt to extract the error from.</param>
    /// <param name="error">When this method returns, contains the error value if the <see cref="Result{TOk, TError}"/> is <c>Error</c>, otherwise the default value.</param>
    /// <returns><see langword="true"/> if the <see cref="Result{TOk, TError}"/> contains an error value; otherwise, <see langword="false"/>.</returns>
    public static bool TryUnwrapError<TOk, TError>(this Result<TOk, TError> result, [NotNullWhen(true)] out TError? error)
    {
        var isError = false;
        var maybeError = default(TError?);

        result.TapError(error =>
        {
            isError = true;
            maybeError = error;
        });

        error = maybeError;

        return isError;
    }

    /// <summary>
    /// Combines a sequence of <see cref="Result{TOk, TError}"/> values into a single <see cref="Result{TOk, TError}"/> containing a list of all success values.
    /// If all results are successful, returns <c>Ok</c> containing an <see cref="ImmutableArray{T}"/> of all success values in order.
    /// If any result is an error, returns the first encountered error and stops processing remaining results.
    /// This operation is "all-or-nothing" - either all succeed or the operation fails.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="results">The sequence of <see cref="Result{TOk, TError}"/> values to combine.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing either all success values as a list, or the first error encountered.</returns>
    public static Result<ImmutableArray<TOk>, TError> All<TOk, TError>(IEnumerable<Result<TOk, TError>> results)
    {
        var okValues = ImmutableArray.CreateBuilder<TOk>();

        foreach (var result in results)
        {
            if (result.TryUnwrapError(out var error))
            {
                return error;
            }

            result.Tap(okValues.Add);
        }

        return okValues.DrainToImmutable();
    }

    /// <summary>
    /// Combines a sequence of <see cref="Result{TOk, TError}"/> providers into a single <see cref="Result{TOk, TError}"/> containing a list of all success values.
    /// Each provider function is called lazily only if the previous results were successful.
    /// If all providers produce successful results, returns <c>Ok</c> containing an <see cref="ImmutableArray{T}"/> of all success values in order.
    /// If any provider produces an error, returns that error and stops calling remaining providers.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="resultProviders">The sequence of functions that provide <see cref="Result{TOk, TError}"/> values.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing either all success values as a list, or the first error encountered.</returns>
    public static Result<ImmutableArray<TOk>, TError> All<TOk, TError>(params IEnumerable<Func<Result<TOk, TError>>> resultProviders) =>
        All(resultProviders.Select(resultProvider => resultProvider()));

    /// <summary>
    /// Attempts to find the first successful result from a sequence of <see cref="Result{TOk, TError}"/> values.
    /// If any result is successful, returns the first success value found.
    /// If all results are errors, returns an <c>Error</c> containing an <see cref="ImmutableArray{T}"/> of all error values in order.
    /// This operation succeeds as soon as any individual result succeeds, providing "first-success" semantics.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="results">The sequence of <see cref="Result{TOk, TError}"/> values to evaluate.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing either the first success value, or all error values as a list.</returns>
    public static Result<TOk, ImmutableArray<TError>> Any<TOk, TError>(IEnumerable<Result<TOk, TError>> results)
    {
        var errors = ImmutableArray.CreateBuilder<TError>();

        foreach (var result in results)
        {
            if (result.TryUnwrap(out var okValue))
            {
                return okValue;
            }

            result.TapError(errors.Add);
        }

        return errors.DrainToImmutable();
    }

    /// <summary>
    /// Attempts to find the first successful result from a sequence of <see cref="Result{TOk, TError}"/> providers.
    /// Each provider function is called lazily until one produces a successful result.
    /// If any provider produces a successful result, returns that success value and stops calling remaining providers.
    /// If all providers produce errors, returns an <c>Error</c> containing an <see cref="ImmutableArray{T}"/> of all error values in order.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="resultProviders">The sequence of functions that provide <see cref="Result{TOk, TError}"/> values.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing either the first success value, or all error values as a list.</returns>
    public static Result<TOk, ImmutableArray<TError>> Any<TOk, TError>(params IEnumerable<Func<Result<TOk, TError>>> resultProviders)
    {
        var errorValues = ImmutableArray.CreateBuilder<TError>();

        foreach (var resultProvider in resultProviders)
        {
            var result = resultProvider();

            if (result.TryUnwrap(out var okValue))
            {
                return okValue;
            }

            result.TapError(errorValues.Add);
        }

        return errorValues.DrainToImmutable();
    }

    /// <summary>
    /// Asynchronously combines a sequence of <see cref="Result{TOk, TError}"/> values into a single result containing a list of all success values.
    /// If all results are successful, returns <c>Ok</c> containing all success values. If any result is an error, returns the first error encountered.
    /// This provides "all-or-nothing" semantics for async result collections.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="results">The async sequence of results to combine.</param>
    /// <returns>A <see cref="Task{T}"/> containing either <c>Ok</c> with all success values, or the first error encountered.</returns>
    public static async Task<Result<ImmutableArray<TOk>, TError>> All<TOk, TError>(IAsyncEnumerable<Result<TOk, TError>> results)
    {
        var okValues = ImmutableArray.CreateBuilder<TOk>();

        await foreach (var result in results)
        {
            if (result.TryUnwrapError(out var error))
            {
                return error;
            }

            result.Tap(okValues.Add);
        }

        return okValues.DrainToImmutable();
    }

    /// <summary>
    /// Asynchronously combines results from a collection of async result providers into a single result containing a list of all success values.
    /// Each provider function is executed and awaited. If all results are successful, returns <c>Ok</c> containing all success values.
    /// If any result is an error, returns the first error encountered.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="resultProvidersAsync">The collection of async functions that provide results when executed.</param>
    /// <returns>A <see cref="Task{T}"/> containing either <c>Ok</c> with all success values, or the first error encountered.</returns>
    public static async Task<Result<ImmutableArray<TOk>, TError>> All<TOk, TError>(params IEnumerable<Func<Task<Result<TOk, TError>>>> resultProvidersAsync)
    {
        var okValues = ImmutableArray.CreateBuilder<TOk>();

        foreach (var resultProviderAsync in resultProvidersAsync)
        {
            var result = await resultProviderAsync();

            if (result.TryUnwrapError(out var error))
            {
                return error;
            }

            result.Tap(okValues.Add);
        }

        return okValues.DrainToImmutable();
    }

    /// <summary>
    /// Asynchronously finds the first successful result from a sequence of <see cref="Result{TOk, TError}"/> values.
    /// If any result is successful, returns the first success value found. If all results are errors, returns an error containing all error values.
    /// This provides "first-success" semantics for async result collections.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="results">The async sequence of results to search.</param>
    /// <returns>A <see cref="Task{T}"/> containing either the first success value, or all error values if none succeed.</returns>
    public static async Task<Result<TOk, ImmutableArray<TError>>> Any<TOk, TError>(IAsyncEnumerable<Result<TOk, TError>> results)
    {
        var errors = ImmutableArray.CreateBuilder<TError>();

        await foreach (var result in results)
        {
            if (result.TryUnwrap(out var okValue))
            {
                return okValue;
            }

            result.TapError(errors.Add);
        }

        return errors.DrainToImmutable();
    }

    /// <summary>
    /// Asynchronously finds the first successful result from a collection of async result providers.
    /// Each provider function is executed and awaited. If any result is successful, returns the first success value found.
    /// If all results are errors, returns an error containing all error values.
    /// </summary>
    /// <typeparam name="TOk">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error values.</typeparam>
    /// <param name="resultProvidersAsync">The collection of async functions that provide results when executed.</param>
    /// <returns>A <see cref="Task{T}"/> containing either the first success value, or all error values if none succeed.</returns>
    public static async Task<Result<TOk, ImmutableArray<TError>>> Any<TOk, TError>(params IEnumerable<Func<Task<Result<TOk, TError>>>> resultProvidersAsync)
    {
        var errors = ImmutableArray.CreateBuilder<TError>();

        foreach (var resultProviderAsync in resultProvidersAsync)
        {
            var result = await resultProviderAsync();

            if (result.TryUnwrap(out var ok))
            {
                return ok;
            }

            result.TapError(errors.Add);
        }

        return errors.DrainToImmutable();
    }
}
