namespace FunctionalEngine;

public static class Try<TException> where TException : Exception
{
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

    public static async Task<Result<TOk, TException>> ExecuteAsync<TOk>(Func<Task<TOk>> functionAsync)
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

public static class Try
{
    public static Result<TOk, Exception> Execute<TOk>(Func<TOk> function) =>
        Try<Exception>.Execute(function);

    public static Task<Result<TOk, Exception>> ExecuteAsync<TOk>(Func<Task<TOk>> functionAsync) =>
        Try<Exception>.ExecuteAsync(functionAsync);
}