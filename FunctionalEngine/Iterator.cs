using System.Collections;

namespace FunctionalEngine;

public static class Iterator
{
    public static IEnumerable<T> Iterate<T>(Func<T> seedProvider, Func<T, T> iterator) => 
        new FunctionEnumerable<T>(() => new IteratorEnumerator<T>(seedProvider, iterator));

    public static IAsyncEnumerable<T> Iterate<T>(Func<Task<T>> seedProviderAsync, Func<T, Task<T>> iteratorAsync) => 
        new FunctionEnumerableAsync<T>(cancellationToken => new IteratorAsyncEnumerator<T>(seedProviderAsync, iteratorAsync, cancellationToken));

    private class FunctionEnumerable<T>(Func<IEnumerator<T>> enumeratorProvider) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => enumeratorProvider();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class FunctionEnumerableAsync<T>(Func<CancellationToken, IAsyncEnumerator<T>> asyncEnumeratorProvider) : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => 
            asyncEnumeratorProvider(cancellationToken);
    }

    private class IteratorAsyncEnumerator<T>(Func<Task<T>> seedProviderAsync, Func<T, Task<T>> iteratorAsync, CancellationToken cancellationToken) : IAsyncEnumerator<T>
    {
        public T Current { get; private set; } = default!;

        private bool isMoved = false;

        public async ValueTask<bool> MoveNextAsync()
        {
            cancellationToken.ThrowIfCancellationRequested();

            Current = isMoved switch
            {
                true => await iteratorAsync(Current),
                false => await seedProviderAsync()
            };

            isMoved = true;
            return true;
        }

        public ValueTask DisposeAsync() => new();
    }

    private class IteratorEnumerator<T>(Func<T> seedProvider, Func<T, T> iterator) : IEnumerator<T>
    {
        public T Current { get; private set; } = default!;

        private bool isMoved = false;

        object IEnumerator.Current => Current!;

        public void Dispose() { }

        public bool MoveNext()
        {
            Current = isMoved switch
            {
                true => iterator(Current),
                false => seedProvider()
            };

            isMoved = true;
            return true;
        }

        public void Reset() => throw new NotSupportedException();
    }
}
