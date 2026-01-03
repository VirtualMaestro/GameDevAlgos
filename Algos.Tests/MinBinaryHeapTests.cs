using System;
using System.Collections.Generic;
using System.Linq;
using Algos.Source.Heaps;
using Xunit;

namespace Algos.Tests
{
    public class MinBinaryHeapTests
    {
        [Fact]
        public void Constructor_DefaultCapacity_CreatesHeap()
        {
            var heap = new MinBinaryHeap<string>();

            Assert.True(heap.IsEmpty);
            Assert.Equal(0, heap.Count);
            Assert.Equal(10, heap.Capacity); // Min capacity is 10
        }

        [Fact]
        public void Constructor_CustomCapacity_CreatesHeap()
        {
            var heap = new MinBinaryHeap<string>(20);

            Assert.True(heap.IsEmpty);
            Assert.Equal(0, heap.Count);
            Assert.Equal(20, heap.Capacity);
        }

        [Fact]
        public void Constructor_SmallCapacity_UsesMinimum()
        {
            var heap = new MinBinaryHeap<string>(5);

            Assert.Equal(10, heap.Capacity); // Should enforce minimum of 10
        }

        [Fact]
        public void Insert_SingleValue_IncreasesCount()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");

            Assert.False(heap.IsEmpty);
            Assert.Equal(1, heap.Count);
        }

        [Fact]
        public void Insert_MultipleValues_MaintainsMinHeapProperty()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");
            heap.Insert(3, "three");
            heap.Insert(7, "seven");
            heap.Insert(1, "one");

            // Min value should be at the top
            var success = heap.Peek(out int value, out string payload);
            Assert.True(success);
            Assert.Equal(1, value);
            Assert.Equal("one", payload);
        }

        [Fact]
        public void Peek_OnEmptyHeap_ReturnsFalse()
        {
            var heap = new MinBinaryHeap<string>();

            var success = heap.Peek(out int value, out string payload);

            Assert.False(success);
            Assert.Equal(int.MinValue, value);
            Assert.Null(payload);
        }

        [Fact]
        public void Peek_DoesNotRemoveElement()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");

            var count1 = heap.Count;
            heap.Peek(out int value, out string payload);
            var count2 = heap.Count;

            Assert.Equal(count1, count2);
        }

        [Fact]
        public void Pop_OnEmptyHeap_ReturnsFalse()
        {
            var heap = new MinBinaryHeap<string>();

            var success = heap.Pop(out int value, out string payload);

            Assert.False(success);
            Assert.Equal(int.MinValue, value);
            Assert.Null(payload);
        }

        [Fact]
        public void Pop_RemovesMinElement()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");
            heap.Insert(3, "three");
            heap.Insert(7, "seven");

            var success = heap.Pop(out int value, out string payload);

            Assert.True(success);
            Assert.Equal(3, value);
            Assert.Equal("three", payload);
            Assert.Equal(2, heap.Count);
        }

        [Fact]
        public void Pop_MultipleTimes_ReturnsInAscendingOrder()
        {
            var heap = new MinBinaryHeap<string>();
            var values = new[] { 15, 10, 20, 8, 21, 3, 9, 5 };

            foreach (var val in values)
                heap.Insert(val, val.ToString());

            var results = new List<int>();
            while (heap.Pop(out int value, out _))
                results.Add(value);

            Assert.Equal(values.OrderBy(x => x).ToList(), results);
        }

        [Fact]
        public void Pop_WithTwoElements_WorksCorrectly()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");
            heap.Insert(3, "three");

            heap.Pop(out int val1, out _);
            heap.Pop(out int val2, out _);

            Assert.Equal(3, val1);
            Assert.Equal(5, val2);
            Assert.True(heap.IsEmpty);
        }

        [Fact]
        public void Pop_WithThreeElements_WorksCorrectly()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");
            heap.Insert(3, "three");
            heap.Insert(7, "seven");

            heap.Pop(out int val1, out _);
            heap.Pop(out int val2, out _);
            heap.Pop(out int val3, out _);

            Assert.Equal(3, val1);
            Assert.Equal(5, val2);
            Assert.Equal(7, val3);
            Assert.True(heap.IsEmpty);
        }

        [Fact]
        public void Insert_ExceedsCapacity_AutomaticallyExtends()
        {
            var heap = new MinBinaryHeap<string>(10);

            for (int i = 0; i < 15; i++)
                heap.Insert(i, i.ToString());

            Assert.Equal(15, heap.Count);
            Assert.True(heap.Capacity >= 15);
        }

        [Fact]
        public void Clear_RemovesAllElements()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");
            heap.Insert(3, "three");
            heap.Insert(7, "seven");

            heap.Clear();

            Assert.True(heap.IsEmpty);
            Assert.Equal(0, heap.Count);
        }

        [Fact]
        public void Clear_AllowsReuse()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");
            heap.Clear();
            heap.Insert(10, "ten");

            var success = heap.Peek(out int value, out string payload);
            Assert.True(success);
            Assert.Equal(10, value);
            Assert.Equal("ten", payload);
        }

        [Fact]
        public void Resize_IncreasesCapacity()
        {
            var heap = new MinBinaryHeap<string>(10);

            heap.Resize(20);

            Assert.Equal(20, heap.Capacity);
        }

        [Fact]
        public void Resize_DecreasesCapacity()
        {
            var heap = new MinBinaryHeap<string>(20);

            heap.Resize(10);

            Assert.Equal(10, heap.Capacity);
        }

        [Fact]
        public void Resize_BelowMinimum_UsesMinimum()
        {
            var heap = new MinBinaryHeap<string>(20);

            heap.Resize(5);

            Assert.Equal(10, heap.Capacity); // Minimum is 10
        }

        [Fact]
        public void Resize_BelowCount_TruncatesElements()
        {
            var heap = new MinBinaryHeap<string>();
            for (int i = 0; i < 15; i++)
                heap.Insert(i, i.ToString());

            heap.Resize(10);

            Assert.Equal(10, heap.Count);
            Assert.Equal(10, heap.Capacity);
        }

        [Fact]
        public void Reset_ClearsAndResizes()
        {
            var heap = new MinBinaryHeap<string>();
            for (int i = 0; i < 20; i++)
                heap.Insert(i, i.ToString());

            heap.Reset();

            Assert.True(heap.IsEmpty);
            Assert.Equal(0, heap.Count);
            // Capacity should be adjusted to maxItemsInUse + 5%
            Assert.True(heap.Capacity >= 20);
        }

        [Fact]
        public void Dispose_MarksHeapAsDisposed()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five");

            heap.Dispose();

            Assert.True(heap.IsDisposed);
        }

        [Fact]
        public void ToArray_ReturnsAllValues()
        {
            var heap = new MinBinaryHeap<string>();
            var values = new[] { 5, 3, 7, 1, 9 };

            foreach (var val in values)
                heap.Insert(val, val.ToString());

            var array = heap.ToArray();

            Assert.Equal(values.Length, array.Length);
            Assert.All(values, v => Assert.Contains(v, array));
        }

        [Fact]
        public void ToArray_OnEmptyHeap_ReturnsEmptyArray()
        {
            var heap = new MinBinaryHeap<string>();

            var array = heap.ToArray();

            Assert.Empty(array);
        }

        [Fact]
        public void Heap_HandlesNegativeValues()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(-5, "minus five");
            heap.Insert(0, "zero");
            heap.Insert(-10, "minus ten");
            heap.Insert(5, "five");

            heap.Pop(out int value, out _);

            Assert.Equal(-10, value);
        }

        [Fact]
        public void Heap_HandlesDuplicateValues()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, "five-1");
            heap.Insert(5, "five-2");
            heap.Insert(3, "three");

            heap.Pop(out int val1, out _);
            heap.Pop(out int val2, out _);
            heap.Pop(out int val3, out _);

            Assert.Equal(3, val1);
            Assert.Equal(5, val2);
            Assert.Equal(5, val3);
        }

        [Fact]
        public void Heap_StressTest_LargeNumberOfElements()
        {
            var heap = new MinBinaryHeap<string>();
            var random = new Random(42); // Fixed seed for reproducibility
            var values = new List<int>();

            // Insert 1000 random values
            for (int i = 0; i < 1000; i++)
            {
                var val = random.Next(-1000, 1000);
                values.Add(val);
                heap.Insert(val, val.ToString());
            }

            // Pop all values and verify they come out in sorted order
            var results = new List<int>();
            while (heap.Pop(out int value, out _))
                results.Add(value);

            Assert.Equal(values.OrderBy(x => x).ToList(), results);
        }

        [Fact]
        public void Heap_WithNullPayload_WorksCorrectly()
        {
            var heap = new MinBinaryHeap<string>();
            heap.Insert(5, null);
            heap.Insert(3, null);

            heap.Pop(out int value, out string payload);

            Assert.Equal(3, value);
            Assert.Null(payload);
        }

        [Fact]
        public void Heap_MixedOperations_MaintainsCorrectness()
        {
            var heap = new MinBinaryHeap<string>();

            heap.Insert(10, "ten");
            heap.Insert(5, "five");
            heap.Pop(out _, out _); // Remove 5

            heap.Insert(3, "three");
            heap.Insert(15, "fifteen");
            heap.Pop(out _, out _); // Remove 3

            heap.Peek(out int value, out _);
            Assert.Equal(10, value); // 10 should be at top now
        }
    }
}
