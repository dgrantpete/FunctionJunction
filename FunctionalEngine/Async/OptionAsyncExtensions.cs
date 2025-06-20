using static FunctionalEngine.Prelude;

namespace FunctionalEngine.Async;

public static partial class OptionAsyncExtensions
{
    public static Task<Option<T>> Sequence<T>(this Option<Task<T>> option) where T : notnull =>
        option.Map(Identity);
}
