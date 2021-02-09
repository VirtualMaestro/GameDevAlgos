using System;
using System.Runtime.CompilerServices;

namespace Algos.Source.Heaps
{
    /// <summary>
    /// Classic min binary heap ds based on struct.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MinBinaryHeap<T>
    {
        private HeapNode[] _heap;
        private int _freeIndex;
        private int _maxItemsInUse;
        
        // min capacity is 10
        public MinBinaryHeap(int initCapacity = 10)
        {
            initCapacity = initCapacity < 10 ? 10 : initCapacity;
            _heap = new HeapNode[initCapacity];
            _PopulateNodes(0, initCapacity);
            _freeIndex = 0;
            _maxItemsInUse = 0;
        }

        // Num of elements in the heap
        public int Count => _freeIndex;
        // Capacity of the heap
        public int Capacity => _heap.Length;
        public bool IsEmpty => _freeIndex == 0;
        public bool IsDisposed => _heap == null;
        
        // Returns min value without removing from the heap
        public bool Peek(out int value, out T payload)
        {
            if (_freeIndex == 0)
            {
                value = int.MinValue;
                payload = default;
                return false;
            }
            
            ref var node = ref _heap[0];
            value = node.Value;
            payload = node.Payload;
            return true;
        }

        // Insert to the heap
        public void Insert(int value, T payload = default)
        {
            if (_freeIndex == _heap.Length)
                _Extend();
            
            _heap[_freeIndex++].Set(value, payload);
            _maxItemsInUse = _freeIndex > _maxItemsInUse ? _freeIndex : _maxItemsInUse;
            
            // try push node on top
            _TryToBubbleUp(_freeIndex - 1);
        }

        // Return value, for 'out' param return payload and remove this item from the heap
        public bool Pop(out int value, out T payload)
        {
            if (_freeIndex > 3)
            {
                ref var headNode = ref _heap[0];
                payload = headNode.Payload;
                value = headNode.Value;

                // get last added element, potential head 
                ref var lastNode = ref _heap[--_freeIndex];
                
                _MoveTo(ref headNode, ref lastNode);
                
                // try push node down
                _TryToBubbleDown(0);
            }
            else
            {
                if (_freeIndex == 0)
                {
                    value = int.MinValue;
                    payload = default;
                    return false;
                }

                ref var headNode = ref _heap[0];
                payload = headNode.Payload;
                value = headNode.Value;
                --_freeIndex;

                if (_freeIndex > 0)
                {
                    var lastNode = _heap[_freeIndex];
                    
                    _MoveTo(ref headNode, ref lastNode);

                    if (_freeIndex == 2)
                    {
                        var leftChildIndex = _GetLeftIndex(0);
                        ref var leftChild = ref _heap[leftChildIndex];

                        if (headNode.Value > leftChild.Value)
                        {
                            _Swap(ref leftChild, ref headNode);
                        }
                    }
                }
            }
            
            return true;
        }

        // Resizes heap to the given size, but not less than 10 
        public void Resize(int capacity = 10)
        {
            var heapLen = _heap.Length;
            if (capacity == heapLen) return;
            capacity = capacity < 10 ? 10 : capacity;

            Array.Resize(ref _heap, capacity);

            if (capacity > heapLen)
                _PopulateNodes(_freeIndex, capacity - heapLen);
            else if (capacity < _freeIndex)
                _freeIndex = capacity;
        }

        // Clear the heap
        public void Clear()
        {
            for(var i = 0; i < _freeIndex; i++)
                _heap[i].Payload = default;

            _freeIndex = 0;
        }
        
        // Resets the heap for reusing.
        // It will Clear and Resize the heap accordingly to max items which were in use + 5%.
        public void Reset()
        {
            var newSize = (int) (_maxItemsInUse + _maxItemsInUse * 0.05);
            Clear();
            Resize(newSize);
            _maxItemsInUse = 0;
        }

        // Dispose the heap. After this operation heap can't be used.
        public void Dispose()
        {
            _heap = null;
        }

        // Creates and returns array with used elements.
        public int[] ToArray()
        {
            var result = new int[_freeIndex];
            for (var i = 0; i < _freeIndex; i++)
            {
                result[i] = _heap[i].Value;
            }

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _TryToBubbleUp(int moveNodeIndex)
        {
            while (moveNodeIndex > 0)
            {
                var parentIndex = _GetParentIndex(moveNodeIndex);
                ref var parentNode = ref _heap[parentIndex];

                ref var newNode = ref _heap[moveNodeIndex];

                if (newNode.Value >= parentNode.Value)
                    return;
                
                _Swap(ref newNode, ref parentNode);

                moveNodeIndex = parentIndex;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _TryToBubbleDown(int moveNodeIndex)
        {
            while (true)
            {
                var leftChildIndex = _GetLeftIndex(moveNodeIndex);
                var rightChildIndex = _GetRightIndex(moveNodeIndex);

                if (rightChildIndex < _freeIndex)
                {
                    ref var leftChild = ref _heap[leftChildIndex];
                    ref var rightChild = ref _heap[rightChildIndex];

                    if (leftChild.Value <= rightChild.Value)
                    {
                        ref var moveNode = ref _heap[moveNodeIndex];
                        if (moveNode.Value <= leftChild.Value) 
                            return;
                        
                        _Swap(ref leftChild, ref moveNode);
                        
                        moveNodeIndex = leftChildIndex;
                    }
                    else
                    {
                        ref var moveNode = ref _heap[moveNodeIndex];
                        if (moveNode.Value <= rightChild.Value) 
                            return;
                        
                        _Swap(ref rightChild, ref moveNode);
                        
                        moveNodeIndex = rightChildIndex;
                    }
                }
                else if (leftChildIndex < _freeIndex)
                {
                    ref var moveNode = ref _heap[moveNodeIndex];
                    ref var leftChild = ref _heap[leftChildIndex];
                    if (moveNode.Value <= leftChild.Value)
                        return;

                    _Swap(ref moveNode, ref leftChild);

                    moveNodeIndex = leftChildIndex;
                }
                else break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _MoveTo(ref HeapNode destination, ref HeapNode source)
        {
            destination.Value = source.Value;
            destination.Payload = source.Payload;

            source.Value = int.MinValue;
            source.Payload = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Swap(ref HeapNode node1, ref HeapNode node2)
        {
            var tVal1 = node1.Value;
            var tPayload1 = node1.Payload;

            node1.Value = node2.Value;
            node1.Payload = node2.Payload;

            node2.Value = tVal1;
            node2.Payload = tPayload1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetParentIndex(int index)
        {
            return (index - 1) / 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetLeftIndex(int parentIndex)
        {
            return parentIndex * 2 + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetRightIndex(int parentIndex)
        {
            return parentIndex * 2 + 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Extend()
        {
            var newHeap = new HeapNode[_freeIndex * 2];
            Array.Copy(_heap, newHeap, _freeIndex);
            _heap = newHeap;
            
            _PopulateNodes(_freeIndex, _freeIndex);
        }

        private void _PopulateNodes(int startIndex, int numNodes)
        {
            while (numNodes > 0)
            {
                _heap[startIndex++] = new HeapNode();
                --numNodes;
            }
        }
        
        private struct HeapNode
        {
            public int Value;
            public T Payload;

            public void Set(int value, T payload)
            {
                Value = value;
                Payload = payload;
            }
        }
    }
}