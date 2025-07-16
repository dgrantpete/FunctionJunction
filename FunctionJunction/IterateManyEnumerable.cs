using System.Collections;

namespace FunctionJunction;

public record IterateManyEnumerable<T> : IEnumerable<T>
{
    private readonly T seed;

    private readonly Func<T, IEnumerable<T>> iterator;

    private bool DepthFirst { get; init; }

    private bool ChildrenFirst { get; init; }

    internal IterateManyEnumerable(T seed, Func<T, IEnumerable<T>> iterator)
    {
        this.seed = seed;
        this.iterator = iterator;
    }

    public IterateManyEnumerable<T> WithParentFirst() => this with { ChildrenFirst = false };

    public IterateManyEnumerable<T> WithChildrenFirst() => this with { ChildrenFirst = true };

    public IterateManyEnumerable<T> WithBreadthFirst() => this with { DepthFirst = false };

    public IterateManyEnumerable<T> WithDepthFirst() => this with { DepthFirst = true };

    public IEnumerator<T> GetEnumerator() => (ChildrenFirst, DepthFirst) switch
    {
        (false, false) => IterateParentFirstBreadth(seed, iterator),
        (false, true) => IterateParentFirstDepth(seed, iterator),
        (true, false) => IterateChildrenFirstBreadth(seed, iterator),
        (true, true) => IterateChildrenFirstDepth(seed, iterator)
    };

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static IEnumerator<T> IterateParentFirstBreadth(T seed, Func<T, IEnumerable<T>> iterator)
    {
        var nextValues = new Queue<T>([seed]);

        while (nextValues.Count is > 0)
        {
            var value = nextValues.Dequeue();
            yield return value;

            foreach (var child in iterator(value))
            {
                nextValues.Enqueue(child);
            }
        }
    }

    private static IEnumerator<T> IterateChildrenFirstBreadth(T seed, Func<T, IEnumerable<T>> iterator)
    {
        var layers = new Stack<T[]>([[seed]]);

        while (layers.Peek().Length > 0)
        {
            var nextLevel = layers.Peek()
                .SelectMany(iterator)
                .ToArray();

            layers.Push(nextLevel);
        }

        while (layers.Count > 0)
        {
            foreach (var value in layers.Pop())
            {
                yield return value;
            }
        }
    }

    private static IEnumerator<T> IterateParentFirstDepth(T seed, Func<T, IEnumerable<T>> iterator)
    {
        var stack = new Stack<T>([seed]);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            var children = iterator(current).Reverse().ToArray();
            foreach (var child in children)
            {
                stack.Push(child);
            }
        }
    }

    private static IEnumerator<T> IterateChildrenFirstDepth(T seed, Func<T, IEnumerable<T>> iterator)
    {
        var visited = new HashSet<T>();
        var stack = new Stack<T>([seed]);
        var result = new List<T>();

        void Visit(T node)
        {
            if (visited.Contains(node))
                return;

            visited.Add(node);
            var children = iterator(node).ToArray();
            
            foreach (var child in children)
            {
                Visit(child);
            }
            
            result.Add(node);
        }

        Visit(seed);
        
        foreach (var item in result)
        {
            yield return item;
        }
    }
}
