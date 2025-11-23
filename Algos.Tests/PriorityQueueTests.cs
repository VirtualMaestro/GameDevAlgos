using System;
using System.Collections.Generic;
using System.Linq;
using Algos.Source.Pathfinding;
using Xunit;

namespace Algos.Tests
{
    public class PriorityQueueTests
    {
        // Test node implementation
        private class TestNode : PriorityQueue<TestNode>.IPriorityQueueNode
        {
            public int HeapIndex { get; set; }
            public int Value { get; set; }
            public string Data { get; set; }

            public TestNode(int value, string data = null)
            {
                Value = value;
                Data = data ?? value.ToString();
            }
        }

        [Fact]
        public void Constructor_DefaultCapacity_CreatesQueue()
        {
            var queue = new PriorityQueue<TestNode>();

            Assert.True(queue.IsEmpty);
            Assert.Equal(0, queue.Count);
            Assert.Equal(10, queue.Capacity);
        }

        [Fact]
        public void Constructor_CustomCapacity_CreatesQueue()
        {
            var queue = new PriorityQueue<TestNode>(20);

            Assert.Equal(0, queue.Count);
            Assert.Equal(20, queue.Capacity);
        }

        [Fact]
        public void Constructor_SmallCapacity_UsesMinimum()
        {
            var queue = new PriorityQueue<TestNode>(5);

            Assert.Equal(10, queue.Capacity);
        }

        [Fact]
        public void Insert_SingleNode_IncreasesCount()
        {
            var queue = new PriorityQueue<TestNode>();
            var node = new TestNode(5);

            queue.Insert(node);

            Assert.Equal(1, queue.Count);
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Insert_SetsHeapIndex()
        {
            var queue = new PriorityQueue<TestNode>();
            var node = new TestNode(5);

            queue.Insert(node);

            Assert.Equal(0, node.HeapIndex); // Should be at index 0
        }

        [Fact]
        public void Insert_MultipleNodes_MaintainsMinHeapProperty()
        {
            var queue = new PriorityQueue<TestNode>();

            queue.Insert(new TestNode(5));
            queue.Insert(new TestNode(3));
            queue.Insert(new TestNode(7));
            queue.Insert(new TestNode(1));

            queue.Peek(out var node);
            Assert.Equal(1, node.Value);
        }

        [Fact]
        public void Peek_OnEmptyQueue_ReturnsFalse()
        {
            var queue = new PriorityQueue<TestNode>();

            var success = queue.Peek(out var node);

            Assert.False(success);
            Assert.Null(node);
        }

        [Fact]
        public void Peek_DoesNotRemoveElement()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));

            var count1 = queue.Count;
            queue.Peek(out _);
            var count2 = queue.Count;

            Assert.Equal(count1, count2);
        }

        [Fact]
        public void Pop_OnEmptyQueue_ReturnsFalse()
        {
            var queue = new PriorityQueue<TestNode>();

            var success = queue.Pop(out var node);

            Assert.False(success);
            Assert.Null(node);
        }

        [Fact]
        public void Pop_RemovesMinElement()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));
            queue.Insert(new TestNode(3));
            queue.Insert(new TestNode(7));

            var success = queue.Pop(out var node);

            Assert.True(success);
            Assert.Equal(3, node.Value);
            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public void Pop_MultipleTimes_ReturnsInAscendingOrder()
        {
            var queue = new PriorityQueue<TestNode>();
            var values = new[] { 15, 10, 20, 8, 21, 3, 9, 5 };

            foreach (var val in values)
                queue.Insert(new TestNode(val));

            var results = new List<int>();
            while (queue.Pop(out var node))
                results.Add(node.Value);

            Assert.Equal(values.OrderBy(x => x).ToList(), results);
        }

        [Fact]
        public void Pop_WithTwoElements_WorksCorrectly()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));
            queue.Insert(new TestNode(3));

            queue.Pop(out var node1);
            queue.Pop(out var node2);

            Assert.Equal(3, node1.Value);
            Assert.Equal(5, node2.Value);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void Pop_WithThreeElements_WorksCorrectly()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));
            queue.Insert(new TestNode(3));
            queue.Insert(new TestNode(7));

            queue.Pop(out var node1);
            queue.Pop(out var node2);
            queue.Pop(out var node3);

            Assert.Equal(3, node1.Value);
            Assert.Equal(5, node2.Value);
            Assert.Equal(7, node3.Value);
        }

        [Fact]
        public void Update_WithValidIndex_ReordersHeap()
        {
            var queue = new PriorityQueue<TestNode>();
            var node1 = new TestNode(10);
            var node2 = new TestNode(20);
            var node3 = new TestNode(30);

            queue.Insert(node1);
            queue.Insert(node2);
            queue.Insert(node3);

            // Change node3's value to be smallest
            node3.Value = 5;
            queue.Update(node3.HeapIndex);

            // node3 should now be at the top
            queue.Peek(out var minNode);
            Assert.Equal(5, minNode.Value);
        }

        [Fact]
        public void Update_WithInvalidNegativeIndex_DoesNotThrow()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));

            // Should not throw
            queue.Update(-1);
        }

        [Fact]
        public void Update_WithIndexEqualToCount_DoesNotThrow()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));
            queue.Insert(new TestNode(3));

            // Count is 2, so index 2 is out of bounds
            // This test exposes the boundary check bug
            queue.Update(2);
        }

        [Fact]
        public void Update_WithIndexGreaterThanCount_DoesNotThrow()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));

            // Should not throw or cause issues
            queue.Update(100);
        }

        [Fact]
        public void Update_OnRootNode_BubblesDown()
        {
            var queue = new PriorityQueue<TestNode>();
            var node1 = new TestNode(5);
            var node2 = new TestNode(10);
            var node3 = new TestNode(15);

            queue.Insert(node1);
            queue.Insert(node2);
            queue.Insert(node3);

            // Change root's value to be largest
            node1.Value = 20;
            queue.Update(0);

            // Root should no longer be node1
            queue.Peek(out var minNode);
            Assert.NotEqual(20, minNode.Value);
        }

        [Fact]
        public void Update_OnLeafNode_BubblesUp()
        {
            var queue = new PriorityQueue<TestNode>();
            var node1 = new TestNode(5);
            var node2 = new TestNode(10);
            var node3 = new TestNode(15);

            queue.Insert(node1);
            queue.Insert(node2);
            queue.Insert(node3);

            // Change node3's value to be smallest
            node3.Value = 1;
            queue.Update(node3.HeapIndex);

            // node3 should now be at root
            queue.Peek(out var minNode);
            Assert.Equal(1, minNode.Value);
        }

        [Fact]
        public void Insert_ExceedsCapacity_AutomaticallyExtends()
        {
            var queue = new PriorityQueue<TestNode>(10);

            for (int i = 0; i < 15; i++)
                queue.Insert(new TestNode(i));

            Assert.Equal(15, queue.Count);
            Assert.True(queue.Capacity >= 15);
        }

        [Fact]
        public void Clear_RemovesAllElements()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));
            queue.Insert(new TestNode(3));
            queue.Insert(new TestNode(7));

            queue.Clear();

            Assert.True(queue.IsEmpty);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void Clear_AllowsReuse()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));
            queue.Clear();
            queue.Insert(new TestNode(10));

            queue.Peek(out var node);
            Assert.Equal(10, node.Value);
        }

        [Fact]
        public void Resize_IncreasesCapacity()
        {
            var queue = new PriorityQueue<TestNode>(10);

            queue.Resize(20);

            Assert.Equal(20, queue.Capacity);
        }

        [Fact]
        public void Resize_DecreasesCapacity()
        {
            var queue = new PriorityQueue<TestNode>(20);

            queue.Resize(10);

            Assert.Equal(10, queue.Capacity);
        }

        [Fact]
        public void Resize_BelowCount_TruncatesElements()
        {
            var queue = new PriorityQueue<TestNode>();
            for (int i = 0; i < 15; i++)
                queue.Insert(new TestNode(i));

            queue.Resize(10);

            Assert.Equal(10, queue.Count);
        }

        [Fact]
        public void Dispose_MarksQueueAsDisposed()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5));

            queue.Dispose();

            Assert.True(queue.IsDisposed);
        }

        [Fact]
        public void ToArray_ReturnsAllValues()
        {
            var queue = new PriorityQueue<TestNode>();
            var values = new[] { 5, 3, 7, 1, 9 };

            foreach (var val in values)
                queue.Insert(new TestNode(val));

            var array = queue.ToArray();

            Assert.Equal(values.Length, array.Length);
            Assert.All(values, v => Assert.Contains(v, array));
        }

        [Fact]
        public void Queue_HandlesNegativeValues()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(-5));
            queue.Insert(new TestNode(0));
            queue.Insert(new TestNode(-10));
            queue.Insert(new TestNode(5));

            queue.Pop(out var node);

            Assert.Equal(-10, node.Value);
        }

        [Fact]
        public void Queue_HandlesDuplicateValues()
        {
            var queue = new PriorityQueue<TestNode>();
            queue.Insert(new TestNode(5, "a"));
            queue.Insert(new TestNode(5, "b"));
            queue.Insert(new TestNode(3, "c"));

            queue.Pop(out var node1);
            queue.Pop(out var node2);
            queue.Pop(out var node3);

            Assert.Equal(3, node1.Value);
            Assert.Equal(5, node2.Value);
            Assert.Equal(5, node3.Value);
        }

        [Fact]
        public void Queue_StressTest_LargeNumberOfElements()
        {
            var queue = new PriorityQueue<TestNode>();
            var random = new Random(42);
            var values = new List<int>();

            for (int i = 0; i < 1000; i++)
            {
                var val = random.Next(-1000, 1000);
                values.Add(val);
                queue.Insert(new TestNode(val));
            }

            var results = new List<int>();
            while (queue.Pop(out var node))
                results.Add(node.Value);

            Assert.Equal(values.OrderBy(x => x).ToList(), results);
        }

        [Fact]
        public void Queue_MixedOperations_MaintainsCorrectness()
        {
            var queue = new PriorityQueue<TestNode>();
            var node1 = new TestNode(10);
            var node2 = new TestNode(5);

            queue.Insert(node1);
            queue.Insert(node2);
            queue.Pop(out _); // Remove 5

            var node3 = new TestNode(3);
            queue.Insert(node3);
            queue.Insert(new TestNode(15));

            queue.Pop(out var min);
            Assert.Equal(3, min.Value);
        }

        [Fact]
        public void HeapIndex_UpdatedCorrectlyAfterOperations()
        {
            var queue = new PriorityQueue<TestNode>();
            var node1 = new TestNode(10);
            var node2 = new TestNode(5);
            var node3 = new TestNode(15);

            queue.Insert(node1);
            queue.Insert(node2);
            queue.Insert(node3);

            // After insertion, all nodes should have valid heap indices
            Assert.True(node1.HeapIndex >= 0 && node1.HeapIndex < queue.Count);
            Assert.True(node2.HeapIndex >= 0 && node2.HeapIndex < queue.Count);
            Assert.True(node3.HeapIndex >= 0 && node3.HeapIndex < queue.Count);
        }

        [Fact]
        public void Update_MaintainsHeapIndices()
        {
            var queue = new PriorityQueue<TestNode>();
            var nodes = new List<TestNode>();

            for (int i = 0; i < 10; i++)
            {
                var node = new TestNode(i);
                nodes.Add(node);
                queue.Insert(node);
            }

            // Update a middle node
            nodes[5].Value = -1;
            queue.Update(nodes[5].HeapIndex);

            // Verify all nodes still have valid indices
            foreach (var node in nodes)
            {
                if (queue.Count > 0) // Node might have been popped
                {
                    Assert.True(node.HeapIndex >= 0);
                }
            }
        }
    }
}
