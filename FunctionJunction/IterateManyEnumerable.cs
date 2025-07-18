using System.Collections;

namespace FunctionJunction;

/// <summary>
/// Provides an enumerable that traverses hierarchical data structures using different traversal strategies.
/// Supports parent-first/children-first ordering and breadth-first/depth-first traversal patterns.
/// </summary>
/// <typeparam name="T">The type of elements in the enumerable.</typeparam>
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

    /// <summary>
    /// Returns a new enumerable that visits parent nodes before their children.
    /// </summary>
    /// <returns>A new <see cref="IterateManyEnumerable{T}"/> configured for parent-first traversal.</returns>
    public IterateManyEnumerable<T> WithParentFirst() => this with { ChildrenFirst = false };

    /// <summary>
    /// Returns a new enumerable that visits children nodes before their parents.
    /// </summary>
    /// <returns>A new <see cref="IterateManyEnumerable{T}"/> configured for children-first traversal.</returns>
    public IterateManyEnumerable<T> WithChildrenFirst() => this with { ChildrenFirst = true };

    /// <summary>
    /// Returns a new enumerable that traverses nodes level by level (breadth-first).
    /// </summary>
    /// <returns>A new <see cref="IterateManyEnumerable{T}"/> configured for breadth-first traversal.</returns>
    public IterateManyEnumerable<T> WithBreadthFirst() => this with { DepthFirst = false };

    /// <summary>
    /// Returns a new enumerable that traverses nodes by going as deep as possible before backtracking (depth-first).
    /// </summary>
    /// <returns>A new <see cref="IterateManyEnumerable{T}"/> configured for depth-first traversal.</returns>
    public IterateManyEnumerable<T> WithDepthFirst() => this with { DepthFirst = true };

    /// <summary>
    /// Returns an enumerator that iterates through the collection using the configured traversal strategy.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/> for the collection.</returns>
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
