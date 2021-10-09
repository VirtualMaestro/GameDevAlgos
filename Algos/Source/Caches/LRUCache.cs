using System.Collections.Generic;

namespace Algos.Source.Caches
{
    /// <summary>
    /// Least recently used.
    /// </summary>
    public class LRUCache<T>
    {
        public delegate bool FindItem(T item, params object[] list);
        
        private readonly Dictionary<T, LinkedListNode<T>> _map;
        private readonly LinkedList<T> _list;
        private readonly int _size;

        public LRUCache(int size = 5)
        {
            _size = size;
            _map = new Dictionary<T, LinkedListNode<T>>(_size + 1);
            _list = new LinkedList<T>();
        }

        public void Add(T item)
        {
            if (_map.TryGetValue(item, out var node))
            {
                _list.Remove(node);
                _list.AddFirst(node);
            }
            else
            {
                node = _list.AddFirst(item);
                _map.Add(item, node);

                if (_list.Count > _size)
                {
                    _map.Remove(_list.Last.Value);
                    _list.RemoveLast();
                }
            }
        }

        /// <summary>
        /// Returns 'true' if item was found in cache and processed. 
        /// </summary>
        public bool Find(FindItem findFunction, params object[] list)
        {
            foreach (var item in _list)
            {
                if (findFunction(item, list))
                {
                    Add(item);
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _map.Clear();
            _list.Clear();
        }
    }
}