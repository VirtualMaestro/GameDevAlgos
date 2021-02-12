using System;
using System.Runtime.CompilerServices;

namespace Algos.Source.Pathfinding
{
    /// <summary>
    /// This is implementation of MinBinaryHeap data structure.
    /// </summary>
    public class PriorityQueue<T> where T : PriorityQueue<T>.IPriorityQueueNode
    {
        private HeapNode[] _heap;
        private int _freeIndex;
        
        // min capacity is 10
        public PriorityQueue(int initCapacity = 10)
        {
            initCapacity = initCapacity < 10 ? 10 : initCapacity;
            _heap = new HeapNode[initCapacity];
            _freeIndex = 0;
            _Indexing(_freeIndex);
        }

        // Num of elements in the heap
        public int Count => _freeIndex;

        // Capacity of the heap
        public int Capacity => _heap.Length;
        public bool IsEmpty => _freeIndex == 0;
        public bool IsDisposed => _heap == null;

        // Returns min value without removing from the heap
        public bool Peek(out T pqNode)
        {
            if (_freeIndex == 0)
            {
                pqNode = default;
                return false;
            }

            ref var node = ref _heap[0];
            pqNode = node.Payload;
            return true;
        }

        // Insert to the heap
        public void Insert(T pqNode)
        {
            if (_freeIndex == _heap.Length)
                _Extend();

            _heap[_freeIndex++].Set(pqNode);

            // try push node on top
            _TryToBubbleUp(_freeIndex - 1);
        }

        // Return value, for 'out' param return payload and remove this item from the heap
        public bool Pop(out T pqNode)
        {
            if (_freeIndex > 3)
            {
                ref var headNode = ref _heap[0];
                pqNode = headNode.Payload;

                _MoveTo(ref headNode, ref _heap[--_freeIndex]);
                
                // try push node down
                _TryToBubbleDown(0);
            }
            else
            {
                if (_freeIndex == 0)
                {
                    pqNode = default;
                    return false;
                }

                ref var headNode = ref _heap[0];
                pqNode = headNode.Payload;
                --_freeIndex;

                if (_freeIndex > 0)
                {
                    _MoveTo(ref headNode, ref _heap[_freeIndex]);

                    if (_freeIndex == 2)
                    {
                        ref var leftChild = ref _heap[1];

                        if (headNode.Value > leftChild.Value)
                        {
                            _Swap(ref leftChild, ref headNode);
                        }
                    }
                }
            }

            return true;
        }

        // If value was change it is possible to call this method with given heap index and it will be 'heapify' (ordered properly in the heap).
        // it is possible to get from IPriorityQueueNode.HeapIndex
        public void Update(int heapIndex)
        {
            if (heapIndex < 0 || heapIndex > _freeIndex)
                return;

            ref var node = ref _heap[heapIndex];
            node.Sync();

            if (heapIndex == 0)
            {
                _TryToBubbleDown(0);
                return;
            }

            if (node.Value < _heap[(heapIndex - 1) / 2].Value)
                _TryToBubbleUp(heapIndex);
            else
            {
                var leftChildIndex = heapIndex * 2 + 1;
                var rightChildIndex = leftChildIndex + 1;

                if (leftChildIndex <= _freeIndex && _heap[leftChildIndex].Value < node.Value ||
                    rightChildIndex <= _freeIndex && _heap[rightChildIndex].Value < node.Value)
                    _TryToBubbleDown(heapIndex);
            }
        }

        // Resizes heap to the given size, but not less than 10 
        public void Resize(int capacity = 10)
        {
            var heapLen = _heap.Length;
            if (capacity == heapLen) return;
            capacity = capacity < 10 ? 10 : capacity;

            Array.Resize(ref _heap, capacity);

            if (capacity < _freeIndex)
                _freeIndex = capacity;
            
            _Indexing(_freeIndex);
        }

        // Clear the heap
        public void Clear()
        {
            for(var i = 0; i < _freeIndex; i++)
                _heap[i].Payload = default;

            _freeIndex = 0;
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
                var parentIndex = (moveNodeIndex - 1) / 2;
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
                var leftChildIndex = moveNodeIndex * 2 + 1;
                var rightChildIndex = leftChildIndex + 1;

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
            destination.Payload.HeapIndex = destination.Index;

            source.Value = default;
            source.Payload = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Swap(ref HeapNode node1, ref HeapNode node2)
        {
            var tVal1 = node1.Value;
            var tPayload1 = node1.Payload;

            node1.Value = node2.Value;
            node1.Payload = node2.Payload;
            node1.Payload.HeapIndex = node1.Index;

            node2.Value = tVal1;
            node2.Payload = tPayload1;
            node2.Payload.HeapIndex = node2.Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Extend()
        {
            var newHeap = new HeapNode[_freeIndex * 2];
            Array.Copy(_heap, newHeap, _freeIndex);
            _heap = newHeap;
            
            _Indexing(_freeIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Indexing(int startIndex)
        {
            for (var i = startIndex; i < _heap.Length; i++)
                _heap[i].Index = i;
        }

        // Internal structure for PriorityQueue
        private struct HeapNode
        {
            public int Index;
            public int Value;
            public T Payload;

            public void Set(T payload)
            {
                Value = payload.Value;
                Payload = payload;
                Payload.HeapIndex = Index;
            }

            public void Sync()
            {
                Value = Payload.Value;
            }
        }

        // Public interface for using custom structure in PriorityQueue
        public interface IPriorityQueueNode
        {
            int HeapIndex { get; set; }
            int Value { get; }
        }
    }
}