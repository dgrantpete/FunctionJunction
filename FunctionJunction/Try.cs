namespace FunctionJunction;

/// <summary>
/// Provides methods for executing functions that may throw exceptions of type <typeparamref name="TException"/> and converting them to <see cref="Result{TOk, TError}"/> values.
/// This allows for explicit exception handling without using try-catch blocks, making error cases visible in the type system.
/// </summary>
/// <typeparam name="TException">The specific type of exception to catch. Must inherit from <see cref="Exception"/>.</typeparam>
public static class Try<TException> where TException : Exception
{
    /// <summary>
    /// Executes a function that may throw an exception of type <typeparamref name="TException"/> and converts the result to a <see cref="Result{TOk, TError}"/>.
    /// If the function executes successfully, returns <c>Ok</c> containing the result. If it throws <typeparamref name="TException"/>, returns <c>Error</c> containing the exception.
    /// Other exception types will not be caught and will propagate normally.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value returned by the function.</typeparam>
    /// <param name="function">The function to execute that may throw <typeparamref name="TException"/>.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing either the function's result or the caught exception.</returns>
    public static Result<TOk, TException> Execute<TOk>(Func<TOk> function)
    {
        try
        {
            return Result.ApplyType<TException>.Ok(function());
        }
        catch (TException exception)
        {
            return Result.ApplyType<TOk>.Error(exception);
        }
    }

    /// <summary>
    /// Asynchronously executes a function that may throw an exception of type <typeparamref name="TException"/> and converts the result to a <see cref="Result{TOk, TError}"/>.
    /// If the async function executes successfully, returns <c>Ok</c> containing the result. If it throws <typeparamref name="TException"/>, returns <c>Error</c> containing the exception.
    /// Other exception types will not be caught and will propagate normally.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value returned by the async function.</typeparam>
    /// <param name="functionAsync">The async function to execute that may throw <typeparamref name="TException"/>.</param>
    /// <returns>A <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with either the function's result or the caught exception.</returns>
    public static async Task<Result<TOk, TException>> Execute<TOk>(Func<Task<TOk>> functionAsync)
    {
        try
        {
            return Result.ApplyType<TException>.Ok(await functionAsync());
        }
        catch (TException exception)
        {
            return Result.ApplyType<TOk>.Error(exception);
        }
    }
}

/// <summary>
/// Provides methods for executing functions that may throw any exception and converting them to <see cref="Result{TOk, TError}"/> values.
/// This is a convenience wrapper around <see cref="Try{TException}"/> that catches all exceptions of type <see cref="Exception"/>.
/// </summary>
public static class Try
{
    /// <summary>
    /// Executes a function that may throw any exception and converts the result to a <see cref="Result{TOk, TError}"/>.
    /// If the function executes successfully, returns <c>Ok</c> containing the result. If it throws any exception, returns <c>Error</c> containing the exception.
    /// This is equivalent to <c>Try&lt;Exception&gt;.Execute(function)</c>.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value returned by the function.</typeparam>
    /// <param name="function">The function to execute that may throw an exception.</param>
    /// <returns>A <see cref="Result{TOk, TError}"/> containing either the function's result or any caught exception.</returns>
    public static Result<TOk, Exception> Execute<TOk>(Func<TOk> function) =>
        Try<Exception>.Execute(function);

    /// <summary>
    /// Asynchronously executes a function that may throw any exception and converts the result to a <see cref="Result{TOk, TError}"/>.
    /// If the async function executes successfully, returns <c>Ok</c> containing the result. If it throws any exception, returns <c>Error</c> containing the exception.
    /// This is equivalent to <c>Try&lt;Exception&gt;.Execute(functionAsync)</c>.
    /// </summary>
    /// <typeparam name="TOk">The type of the success value returned by the async function.</typeparam>
    /// <param name="functionAsync">The async function to execute that may throw an exception.</param>
    /// <returns>A <see cref="Task"/> containing a <see cref="Result{TOk, TError}"/> with either the function's result or any caught exception.</returns>
    public static Task<Result<TOk, Exception>> Execute<TOk>(Func<Task<TOk>> functionAsync) =>
        Try<Exception>.Execute(functionAsync);
}