namespace FunctionJunction;

/// <summary>
/// Provides common functional programming utilities including identity functions, constant functions, and function composition.
/// These utilities are foundational building blocks for functional programming patterns and are commonly used throughout functional codebases.
/// </summary>
public static class Prelude
{
    /// <summary>
    /// The identity function that returns its input unchanged.
    /// This is a fundamental function in functional programming, often used as a default transformation or placeholder.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to return unchanged.</param>
    /// <returns>The same value that was passed in.</returns>
    public static T Identity<T>(T value) => value;

    /// <summary>
    /// Returns the first parameter, ignoring the second. Useful for projecting the first value in tuple-like operations.
    /// </summary>
    /// <typeparam name="TIdentity">The type of the value to return.</typeparam>
    /// <typeparam name="T1">The type of the ignored parameter.</typeparam>
    /// <param name="value">The value to return.</param>
    /// <param name="_">The ignored parameter.</param>
    /// <returns>The first parameter.</returns>
    public static TIdentity IdentityFirst<TIdentity, T1>(TIdentity value, T1 _) => value;

    /// <summary>
    /// Returns the second parameter, ignoring the first. Useful for projecting the second value in tuple-like operations.
    /// </summary>
    /// <typeparam name="TIdentity">The type of the value to return.</typeparam>
    /// <typeparam name="T1">The type of the ignored parameter.</typeparam>
    /// <param name="_">The ignored parameter.</param>
    /// <param name="value">The value to return.</param>
    /// <returns>The second parameter.</returns>
    public static TIdentity IdentitySecond<TIdentity, T1>(T1 _, TIdentity value) => value;

    /// <summary>Returns the first parameter from a 3-parameter function, ignoring the others.</summary>
    public static TIdentity IdentityFirst<TIdentity, T1, T2>(TIdentity value, T1 _, T2 __) => value;

    /// <summary>Returns the second parameter from a 3-parameter function, ignoring the others.</summary>
    public static TIdentity IdentitySecond<TIdentity, T1, T2>(T1 _, TIdentity value, T2 __) => value;

    /// <summary>Returns the third parameter from a 3-parameter function, ignoring the others.</summary>
    public static TIdentity IdentityThird<TIdentity, T1, T2>(T1 _, T2 __, TIdentity value) => value;

    /// <summary>Returns the first parameter from a 4-parameter function, ignoring the others.</summary>
    public static TIdentity IdentityFirst<TIdentity, T1, T2, T3>(TIdentity value, T1 _, T2 __, T3 ___) => value;

    /// <summary>Returns the second parameter from a 4-parameter function, ignoring the others.</summary>
    public static TIdentity IdentitySecond<TIdentity, T1, T2, T3>(T1 _, TIdentity value, T2 __, T3 ___) => value;

    /// <summary>Returns the third parameter from a 4-parameter function, ignoring the others.</summary>
    public static TIdentity IdentityThird<TIdentity, T1, T2, T3>(T1 _, T2 __, TIdentity value, T3 ___) => value;

    /// <summary>Returns the fourth parameter from a 4-parameter function, ignoring the others.</summary>
    public static TIdentity IdentityFourth<TIdentity, T1, T2, T3>(T1 _, T2 __, T3 ___, TIdentity value) => value;

    /// <summary>
    /// Creates a function that always returns the specified constant value, ignoring any parameters.
    /// Useful for creating functions that return fixed values regardless of input.
    /// </summary>
    /// <typeparam name="T">The type of the constant value.</typeparam>
    /// <param name="value">The constant value to always return.</param>
    /// <returns>A function that always returns the specified value.</returns>
    public static Func<T> Const<T>(T value) => () => value;

    /// <summary>Creates a function that always returns the specified constant value, ignoring its single parameter.</summary>
    public static Func<T1, T> Const<T, T1>(T value) => (_) => value;

    /// <summary>Creates a function that always returns the specified constant value, ignoring its two parameters.</summary>
    public static Func<T1, T2, T> Const<T, T1, T2>(T value) => (_, _) => value;

    /// <summary>Creates a function that always returns the specified constant value, ignoring its three parameters.</summary>
    public static Func<T1, T2, T3, T> Const<T, T1, T2, T3>(T value) => (_, _, _) => value;

    /// <summary>Creates a function that always returns the specified constant value, ignoring its four parameters.</summary>
    public static Func<T1, T2, T3, T4, T> Const<T, T1, T2, T3, T4>(T value) => (_, _, _, _) => value;

    /// <summary>
    /// Composes two functions where the output of the first becomes the input of the second.
    /// This creates a new function that represents the mathematical composition (g ∘ f)() = g(f()).
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate type produced by the input function and consumed by the output function.</typeparam>
    /// <typeparam name="TResult">The final result type.</typeparam>
    /// <param name="inputFunction">The first function to execute.</param>
    /// <param name="outputFunction">The second function to execute, using the result of the first.</param>
    /// <returns>A composed function that applies both functions in sequence.</returns>
    public static Func<TResult> Compose<TIntermediate, TResult>(
        Func<TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        () => outputFunction(inputFunction());

    /// <summary>Composes two functions: (g ∘ f)(x) = g(f(x)).</summary>
    public static Func<T, TResult> Compose<T, TIntermediate, TResult>(
        Func<T, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        input => outputFunction(inputFunction(input));

    /// <summary>Composes two functions: (g ∘ f)(x, y) = g(f(x, y)).</summary>
    public static Func<T1, T2, TResult> Compose<T1, T2, TIntermediate, TResult>(
        Func<T1, T2, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        (firstInput, secondInput) => outputFunction(inputFunction(firstInput, secondInput));

    /// <summary>Composes two functions: (g ∘ f)(x, y, z) = g(f(x, y, z)).</summary>
    public static Func<T1, T2, T3, TResult> Compose<T1, T2, T3, TIntermediate, TResult>(
        Func<T1, T2, T3, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        (firstInput, secondInput, thirdInput) => outputFunction(inputFunction(firstInput, secondInput, thirdInput));

    /// <summary>Composes two functions: (g ∘ f)(w, x, y, z) = g(f(w, x, y, z)).</summary>
    public static Func<T1, T2, T3, T4, TResult> Compose<T1, T2, T3, T4, TIntermediate, TResult>(
        Func<T1, T2, T3, T4, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        (firstInput, secondInput, thirdInput, fourthInput) =>
            outputFunction(inputFunction(firstInput, secondInput, thirdInput, fourthInput));

    /// <summary>
    /// Calls the function which is passed in. Useful for compositional and point-free code.
    /// </summary>
    /// <param name="function">The function to be called.</param>
    /// <returns>The return value of the function being passed in.</returns>
    /// <remarks>
    /// <code>
    /// IEnumerable&lt;Func&lt;T&gt;&gt; lazyValues = ...;
    /// 
    /// IEnumerable&lt;T&gt; computedValues = lazyValues.Select(Invoke);
    /// </code>
    /// </remarks>
    public static T Invoke<T>(Func<T> function) => function();
}
