using System.Collections.ObjectModel;

namespace QFace.Sdk.Extensions;

/// <summary>
/// Extension methods for collections and enumerables
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Determines whether a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }
    
    /// <summary>
    /// Returns a default value if the collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="defaultValue">The default value to return if the collection is null or empty.</param>
    /// <returns>The original collection if not null or empty; otherwise, the default value.</returns>
    public static IEnumerable<T> DefaultIfEmpty<T>(this IEnumerable<T>? collection, IEnumerable<T> defaultValue)
    {
        return collection.IsNullOrEmpty() ? defaultValue : collection;
    }
    
    /// <summary>
    /// Adds a range of items to a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to add items to.</param>
    /// <param name="items">The items to add.</param>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (items == null) return;
        
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
    
    /// <summary>
    /// Converts an enumerable to a read-only collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to convert.</param>
    /// <returns>A read-only collection containing the elements from the input collection.</returns>
    public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        
        return new ReadOnlyCollection<T>(collection.ToList());
    }
    
    /// <summary>
    /// Returns distinct elements from a sequence by using a specified key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key to distinguish elements by.</typeparam>
    /// <param name="source">The sequence to remove duplicate elements from.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An IEnumerable&lt;T&gt; that contains distinct elements from the source sequence.</returns>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        
        var knownKeys = new HashSet<TKey>();
        foreach (var element in source)
        {
            if (knownKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
    
    /// <summary>
    /// Splits a collection into chunks of a specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to split.</param>
    /// <param name="chunkSize">The size of each chunk.</param>
    /// <returns>A collection of chunks.</returns>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (chunkSize <= 0) throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));
        
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return GetChunk(enumerator, chunkSize);
        }
    }
    
    private static IEnumerable<T> GetChunk<T>(IEnumerator<T> enumerator, int chunkSize)
    {
        yield return enumerator.Current;
        
        for (int i = 1; i < chunkSize && enumerator.MoveNext(); i++)
        {
            yield return enumerator.Current;
        }
    }
    
    /// <summary>
    /// Returns a random element from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to get a random element from.</param>
    /// <returns>A random element from the collection.</returns>
    /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
    public static T GetRandomElement<T>(this IEnumerable<T> source)
    {
        if (source.IsNullOrEmpty()) throw new ArgumentException("The collection cannot be empty.", nameof(source));
        
        var random = new Random();
        var list = source.ToList();
        var index = random.Next(0, list.Count);
        return list[index];
    }
    
    /// <summary>
    /// Performs an action for each element in a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to process.</param>
    /// <param name="action">The action to perform on each element.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        foreach (var item in source)
        {
            action(item);
        }
    }
    
    /// <summary>
    /// Performs an async action for each element in a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to process.</param>
    /// <param name="action">The async action to perform on each element.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        foreach (var item in source)
        {
            await action(item);
        }
    }
    
    /// <summary>
    /// Performs an async action for each element in a collection in parallel.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to process.</param>
    /// <param name="action">The async action to perform on each element.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent tasks.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ForEachParallelAsync<T>(this IEnumerable<T> source, Func<T, Task> action, int maxDegreeOfParallelism = 0)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        var items = source.ToList();
        if (items.Count == 0) return;
        
        if (maxDegreeOfParallelism <= 0)
        {
            maxDegreeOfParallelism = Environment.ProcessorCount;
        }
        
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        
        foreach (var item in items)
        {
            await semaphore.WaitAsync();
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await action(item);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// Checks if a collection contains any of the specified items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <param name="items">The items to look for.</param>
    /// <returns>True if the collection contains any of the specified items; otherwise, false.</returns>
    public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> items)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (items == null) throw new ArgumentNullException(nameof(items));
        
        return items.Any(source.Contains);
    }
    
    /// <summary>
    /// Checks if a collection contains all of the specified items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <param name="items">The items to look for.</param>
    /// <returns>True if the collection contains all of the specified items; otherwise, false.</returns>
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> items)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (items == null) throw new ArgumentNullException(nameof(items));
        
        return items.All(source.Contains);
    }
    
    /// <summary>
    /// Checks if two collections have the same elements (same elements in the same quantities, regardless of order).
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="first">The first collection.</param>
    /// <param name="second">The second collection.</param>
    /// <returns>True if the collections have the same elements; otherwise, false.</returns>
    public static bool HasSameElements<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        if (first == null && second == null) return true;
        if (first == null || second == null) return false;
        
        var firstCounts = first.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var secondCounts = second.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        
        if (firstCounts.Count != secondCounts.Count) return false;
        
        foreach (var pair in firstCounts)
        {
            if (!secondCounts.TryGetValue(pair.Key, out int count) || count != pair.Value)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Returns the index of the first occurrence of an item in a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to search.</param>
    /// <param name="item">The item to look for.</param>
    /// <returns>The index of the first occurrence of the item, or -1 if not found.</returns>
    public static int IndexOf<T>(this IEnumerable<T> source, T item)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        int index = 0;
        foreach (var sourceItem in source)
        {
            if (EqualityComparer<T>.Default.Equals(sourceItem, item))
            {
                return index;
            }
            index++;
        }
        
        return -1;
    }
    
    /// <summary>
    /// Returns the item at the specified index in a collection, or a default value if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to get an item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <param name="defaultValue">The default value to return if the index is out of range.</param>
    /// <returns>The item at the specified index, or the default value if the index is out of range.</returns>
    public static T ElementAtOrDefault<T>(this IEnumerable<T> source, int index, T defaultValue)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        if (index < 0) return defaultValue;
        
        int currentIndex = 0;
        foreach (var item in source)
        {
            if (currentIndex == index) return item;
            currentIndex++;
        }
        
        return defaultValue;
    }
}