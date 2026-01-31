using System.Collections.Generic;

namespace InventoryTools.Compendium.Models;

public sealed class CompendiumDataCacher<TKey, TValue>
    where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map;
    private readonly LinkedList<(TKey Key, TValue Value)> _list;

    public CompendiumDataCacher(int capacity)
    {
        _capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(capacity);
        _list = new LinkedList<(TKey, TValue)>();
    }

    public bool TryGet(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _list.Remove(node);
            _list.AddFirst(node);
            value = node.Value.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        if (_map.TryGetValue(key, out var existing))
        {
            _list.Remove(existing);
        }
        else if (_map.Count >= _capacity)
        {
            var lru = _list.Last!;
            _map.Remove(lru.Value.Key);
            _list.RemoveLast();
        }

        var node = new LinkedListNode<(TKey, TValue)>((key, value));
        _list.AddFirst(node);
        _map[key] = node;
    }
}