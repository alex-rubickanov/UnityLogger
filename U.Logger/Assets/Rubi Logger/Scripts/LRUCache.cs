using System.Collections.Generic;

namespace Rubickanov.Logger
{
    public class LRUCache<TKey, TValue>
    {
        private readonly int capacity;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache;
        private readonly LinkedList<CacheItem> lruList;

        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            this.cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            this.lruList = new LinkedList<CacheItem>();
        }

        public TValue Get(TKey key)
        {
            if (cache.TryGetValue(key, out var node))
            {
                var value = node.Value.Value;
                lruList.Remove(node);
                lruList.AddLast(node);
                return value;
            }

            return default(TValue);
        }

        public void Add(TKey key, TValue value)
        {
            if (cache.Count >= capacity)
            {
                RemoveFirst();
            }

            var cacheItem = new CacheItem { Key = key, Value = value };
            var node = new LinkedListNode<CacheItem>(cacheItem);
            lruList.AddLast(node);
            cache.Add(key, node);
        }

        private void RemoveFirst()
        {
            var node = lruList.First;
            lruList.RemoveFirst();
            cache.Remove(node.Value.Key);
        }

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }
    }
}