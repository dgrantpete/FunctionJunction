namespace FunctionalEngine;

public static class Functions
{
    public static T Identity<T>(T value) => value;

    public static TIdentity IdentityFirst<TIdentity, T1>(TIdentity value, T1 _) => value;

    public static TIdentity IdentitySecond<TIdentity, T1>(T1 _, TIdentity value) => value;

    public static TIdentity IdentityFirst<TIdentity, T1, T2>(TIdentity value, T1 _, T2 __) => value;

    public static TIdentity IdentitySecond<TIdentity, T1, T2>(T1 _, TIdentity value, T2 __) => value;

    public static TIdentity IdentityThird<TIdentity, T1, T2>(T1 _, T2 __, TIdentity value) => value;

    public static TIdentity IdentityFirst<TIdentity, T1, T2, T3>(TIdentity value, T1 _, T2 __, T3 ___) => value;

    public static TIdentity IdentitySecond<TIdentity, T1, T2, T3>(T1 _, TIdentity value, T2 __, T3 ___) => value;

    public static TIdentity IdentityThird<TIdentity, T1, T2, T3>(T1 _, T2 __, TIdentity value, T3 ___) => value;

    public static TIdentity IdentityFourth<TIdentity, T1, T2, T3>(T1 _, T2 __, T3 ___, TIdentity value) => value;

    public static Func<T> Const<T>(T value) => () => value;

    public static Func<T1, T> Const<T, T1>(T value) => (_) => value;

    public static Func<T1, T2, T> Const<T, T1, T2>(T value) => (_, _) => value;

    public static Func<T1, T2, T3, T> Const<T, T1, T2, T3>(T value) => (_, _, _) => value;

    public static Func<T1, T2, T3, T4, T> Const<T, T1, T2, T3, T4>(T value) => (_, _, _, _) => value;

    public static Func<TResult> Compose<TIntermediate, TResult>(
        Func<TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        () => outputFunction(inputFunction());

    public static Func<T, TResult> Compose<T, TIntermediate, TResult>(
        Func<T, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        input => outputFunction(inputFunction(input));

    public static Func<T1, T2, TResult> Compose<T1, T2, TIntermediate, TResult>(
        Func<T1, T2, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        (firstInput, secondInput) => outputFunction(inputFunction(firstInput, secondInput));

    public static Func<T1, T2, T3, TResult> Compose<T1, T2, T3, TIntermediate, TResult>(
        Func<T1, T2, T3, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        (firstInput, secondInput, thirdInput) => outputFunction(inputFunction(firstInput, secondInput, thirdInput));

    public static Func<T1, T2, T3, T4, TResult> Compose<T1, T2, T3, T4, TIntermediate, TResult>(
        Func<T1, T2, T3, T4, TIntermediate> inputFunction,
        Func<TIntermediate, TResult> outputFunction
    ) =>
        (firstInput, secondInput, thirdInput, fourthInput) =>
            outputFunction(inputFunction(firstInput, secondInput, thirdInput, fourthInput));
}
