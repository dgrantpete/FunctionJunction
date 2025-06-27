<p align="center">
  <img src="FunctionJunction.png" alt="Function Junction Icon" width="128"/>
</p>

# FunctionJunction

A functional programming library for C# that provides Option and Result types, discriminated unions, and functional combinators with comprehensive async support.

## Installation

```bash
dotnet add package FunctionJunction
```

## Core Types

### Option<T>

Represents a value that may or may not exist.

```csharp
using FunctionJunction;
using static FunctionJunction.Prelude;

// Create options
var some = Some(42);
var none = None<int>();

// Transform values
var doubled = some.Map(x => x * 2);
var parsed = "123".TryParse(out int n) ? Some(n) : None<int>();

// Chain operations
var result = LoadConfig()
    .FlatMap(config => config.ConnectionString)
    .Filter(conn => !string.IsNullOrEmpty(conn))
    .UnwrapOr(() => "DefaultConnection");
```

### Result<TOk, TError>

Represents an operation that can succeed with TOk or fail with TError.

```csharp
// Create results
Result<int, string> success = 42;  // Implicit conversion
Result<int, string> failure = "Error occurred";

// Transform and validate
var result = ParseInt(userInput)
    .Map(x => x * 2)
    .Validate(x => x > 0, _ => "Value must be positive")
    .MapError(error => $"Validation failed: {error}");

// Error recovery
var recovered = result
    .Recover(error => TryAlternativeMethod(error))
    .UnwrapOr(error => DefaultValue);
```

### Discriminated Unions

Create sum types with automatic pattern matching via source generation.

```csharp
[DiscriminatedUnion]
public partial record PaymentResult
{
    public record Success(string TransactionId, decimal Amount) : PaymentResult;
    public record Declined(string Reason) : PaymentResult;
    public record Error(Exception Exception) : PaymentResult;
}

// Generated Match method
var message = paymentResult.Match(
    onSuccess: (id, amount) => $"Payment ${amount} succeeded: {id}",
    onDeclined: reason => $"Payment declined: {reason}",
    onError: ex => $"Payment failed: {ex.Message}"
);
```

## Async Support

All operations have async counterparts that work with Task-returning functions with `Await` prefix:

```csharp
// Async operations
var userData = await userIdOption
    .FlatMap(async id => await FetchUser(id))
    .AwaitFilter(user => user.IsActive)
    .AwaitMap(async user => await EnrichUserData(user))
    .AwaitUnwrapOr(async () => await GetDefaultUser());

// Combining multiple async operations
var result = await Result.All(
    ValidateEmail(email),
    CheckUserExists(email),
    VerifyNotBlacklisted(email)
);

// Async enumerable extensions
await productsIds
    .ToAsyncEnumerable()
    .SelectWhere(async id => await TryLoadProduct(id))
    .Scan(0m, (total, product) => total + product.Price)
    .LastAsync();
```

## API Reference

### Option<T> Methods

- `Map<TResult>(Func<T, TResult>)` - Transform the value if present
- `FlatMap<TResult>(Func<T, Option<TResult>>)` - Chain operations that return Options
- `Filter(Func<T, bool>)` - Keep value only if predicate returns true
- `Or(Func<Option<T>>)` - Provide alternative if None
- `And<TOther>(Func<Option<TOther>>)` - Combine two Options into tuple
- `UnwrapOr(Func<T>)` - Extract value or provide default
- `UnwrapOrThrow<TException>(Func<TException>)` - Extract value or throw
- `TryUnwrap(out T?)` - Try pattern for safe extraction

### Result<TOk, TError> Methods

- `Map<TResult>(Func<TOk, TResult>)` - Transform success value
- `MapError<TResult>(Func<TError, TResult>)` - Transform error value
- `FlatMap<TResult>(Func<TOk, Result<TResult, TError>>)` - Chain operations
- `Recover<TResult>(Func<TError, Result<TOk, TResult>>)` - Attempt error recovery
- `Validate(Func<TOk, bool>, Func<TOk, TError>)` - Add validation
- `And<TOther>(Func<Result<TOther, TError>>)` - Combine if both succeed
- `Or<TOther>(Func<Result<TOk, TOther>>)` - Try alternative on error
- `Swap()` - Exchange success and error positions
- `TryUnwrap(out TOk?)` / `TryUnwrapError(out TError?)` - Try patterns

### Static Helpers

- `Option.Some<T>(T)` / `Option.None<T>()` - Create Options
- `Result.Ok<TOk, TError>(TOk)` / `Result.Error<TOk, TError>(TError)` - Create Results
- `Option.All(IEnumerable<Option<T>>)` - Combine Options (all must be Some)
- `Option.Any(IEnumerable<Option<T>>)` - Find first Some
- `Result.All(IEnumerable<Result<TOk, TError>>)` - Combine Results (all must succeed)
- `Result.Any(IEnumerable<Result<TOk, TError>>)` - Find first success
- `Try.Execute<TOk>(Func<TOk>)` - Convert exceptions to Results
- `Try<TException>.Execute<TOk>(Func<TOk>)` - Catch specific exception types

### Iterator Extensions

- `Enumerate<T>()` - Pair elements with indices
- `Scan<TSource, TResult>()` - Running accumulation with intermediates
- `SelectWhere<TSource, TResult>()` - Combined Select+Where using Option
- `TakeWhileInclusive<T>()` - Take while true, including first false

## Source Generators

### DiscriminatedUnion Attribute

Configure discriminated union generation:

```csharp
[DiscriminatedUnion(
    MatchOn = MatchUnionOn.Properties,  // or Type, None
    GeneratePolymorphicSerialization = true,
    GeneratePrivateConstructor = true
)]
public partial record Command { /* ... */ }
```
