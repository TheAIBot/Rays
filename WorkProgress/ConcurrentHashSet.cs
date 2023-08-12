using System.Collections;
using System.Collections.Concurrent;

namespace WorkProgress;

public sealed class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, T> _storage = new();

    public bool Add(T item)
    {
        return _storage.TryAdd(item, item);
    }

    public bool Remove(T item)
    {
        return _storage.TryRemove(item, out T? _);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _storage.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _storage.Keys.GetEnumerator();
    }
}
