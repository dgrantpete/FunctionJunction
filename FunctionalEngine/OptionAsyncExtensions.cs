using static FunctionalEngine.Functions;

namespace FunctionalEngine;

public static partial class OptionAsyncExtensions
{
    public static Task<Option<T>> SequenceAsync<T>(this Option<Task<T>> option) where T : notnull =>
        option.MapAsync(Identity);
}
