using System;
using System.Collections.Generic;
using Algos.Source.Pools;
using Xunit;

namespace Algos.Tests
{
    public class PoolTests
    {
        // Test class for pooling
        private class TestItem
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
        }

        // Test creator implementation
        private class TestCreator : ICreator<TestItem>
        {
            public int CreateCount { get; private set; }
            public int ToPoolCount { get; private set; }
            public int FromPoolCount { get; private set; }
            public int DisposeCount { get; private set; }

            private int _nextId = 0;

            public TestItem OnCreate()
            {
                CreateCount++;
                return new TestItem { Id = _nextId++, IsActive = false };
            }

            public void OnToPool(TestItem t)
            {
                ToPoolCount++;
                t.IsActive = false;
            }

            public void OnFromPool(TestItem t)
            {
                FromPoolCount++;
                t.IsActive = true;
            }

            public void OnDispose(TestItem t)
            {
                DisposeCount++;
            }

            public void Dispose()
            {
                // Cleanup if needed
            }
        }

        [Fact]
        public void Constructor_DefaultCapacity_CreatesPool()
        {
            var pool = new Pool<TestItem>();

            Assert.True(pool.IsEmpty);
            Assert.Equal(0, pool.AvailableItems);
            Assert.Equal(16, pool.Size); // Default capacity
        }

        [Fact]
        public void Constructor_CustomCapacity_CreatesPool()
        {
            var pool = new Pool<TestItem>(10);

            Assert.Equal(10, pool.Size);
        }

        [Fact]
        public void Constructor_SmallCapacity_UsesMinimum()
        {
            var pool = new Pool<TestItem>(2);

            Assert.Equal(4, pool.Size); // Minimum is 4
        }

        [Fact]
        public void Constructor_WithCreateMethod_Initializes()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());

            Assert.NotNull(pool.CreateMethod);
        }

        [Fact]
        public void Constructor_WithCreator_Initializes()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator);

            Assert.NotNull(pool.Creator);
        }

        [Fact]
        public void Get_WhenEmpty_CreatesNewInstance()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem { Id = 99 });

            var item = pool.Get();

            Assert.NotNull(item);
            Assert.Equal(99, item.Id);
        }

        [Fact]
        public void Get_WhenNotEmpty_ReturnsPooledInstance()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            var original = new TestItem { Id = 42 };
            pool.Put(original);

            var retrieved = pool.Get();

            Assert.Same(original, retrieved);
            Assert.Equal(42, retrieved.Id);
        }

        [Fact]
        public void Put_AddsItemToPool()
        {
            var pool = new Pool<TestItem>();
            var item = new TestItem();

            pool.Put(item);

            Assert.Equal(1, pool.AvailableItems);
        }

        [Fact]
        public void Put_WhenFull_ExpandsPool()
        {
            var pool = new Pool<TestItem>(4, () => new TestItem());

            // Fill the pool
            for (int i = 0; i < 4; i++)
                pool.Put(new TestItem());

            Assert.Equal(4, pool.Size);
            Assert.True(pool.IsFull);

            // Add one more - should expand (double the size)
            pool.Put(new TestItem());

            Assert.Equal(8, pool.Size); // Pool doubles in size: 4 -> 8
            Assert.Equal(5, pool.AvailableItems);
        }

        [Fact]
        public void GetPut_RoundTrip_WorksCorrectly()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            var item = new TestItem { Id = 123 };

            pool.Put(item);
            var retrieved = pool.Get();

            Assert.Same(item, retrieved);
        }

        [Fact]
        public void IsEmpty_ReflectsPoolState()
        {
            var pool = new Pool<TestItem>();

            Assert.True(pool.IsEmpty);

            pool.Put(new TestItem());
            Assert.False(pool.IsEmpty);

            pool.Get();
            Assert.True(pool.IsEmpty);
        }

        [Fact]
        public void IsFull_ReflectsPoolState()
        {
            var pool = new Pool<TestItem>(4, () => new TestItem());

            Assert.False(pool.IsFull);

            for (int i = 0; i < 4; i++)
                pool.Put(new TestItem());

            Assert.True(pool.IsFull);
        }

        [Fact]
        public void FreeSlots_CalculatesCorrectly()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());

            Assert.Equal(10, pool.FreeSlots);

            pool.Put(new TestItem());
            Assert.Equal(9, pool.FreeSlots);

            pool.Put(new TestItem());
            pool.Put(new TestItem());
            Assert.Equal(7, pool.FreeSlots);
        }

        [Fact]
        public void PreWarm_FillsPool()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());

            pool.PreWarm(5);

            Assert.Equal(5, pool.AvailableItems);
        }

        [Fact]
        public void PreWarm_NoArgument_FillsToCapacity()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            pool.Put(new TestItem());
            pool.Put(new TestItem());

            pool.PreWarm();

            Assert.Equal(10, pool.AvailableItems);
        }

        [Fact]
        public void PreWarm_ExceedsCapacity_ExtendsPool()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());

            pool.PreWarm(15);

            Assert.True(pool.Size >= 15);
            Assert.Equal(15, pool.AvailableItems);
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            pool.PreWarm(5);

            pool.Clear();

            Assert.True(pool.IsEmpty);
            Assert.Equal(0, pool.AvailableItems);
        }

        [Fact]
        public void Clear_WithShrink_ReducesSize()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            pool.PreWarm(20); // Expand pool

            pool.Clear(shrink: true);

            Assert.Equal(10, pool.Size); // Back to initial capacity
        }

        [Fact]
        public void Dispose_MarksPoolAsDisposed()
        {
            var pool = new Pool<TestItem>();

            pool.Dispose();

            Assert.True(pool.IsDisposed);
        }

        [Fact]
        public void Creator_OnCreate_CalledWhenCreating()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator);

            pool.Get();

            Assert.Equal(1, creator.CreateCount);
        }

        [Fact]
        public void Creator_OnToPool_CalledWhenPutting()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator);

            pool.Put(new TestItem());

            Assert.Equal(1, creator.ToPoolCount);
        }

        [Fact]
        public void Creator_OnFromPool_CalledWhenGetting()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator);
            pool.Put(new TestItem());

            pool.Get();

            Assert.Equal(1, creator.FromPoolCount);
        }

        [Fact]
        public void Creator_OnDispose_CalledWhenClearing()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator);
            pool.PreWarm(3);

            pool.Clear();

            Assert.Equal(3, creator.DisposeCount);
        }

        [Fact]
        public void SetCreator_ReplacesCreateMethod()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem { Id = 1 });
            var creator = new TestCreator();

            pool.Creator = creator;

            Assert.Null(pool.CreateMethod);
            Assert.NotNull(pool.Creator);
        }

        [Fact]
        public void SetCreateMethod_ReplacesCreator()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator);

            pool.CreateMethod = () => new TestItem { Id = 99 };

            Assert.NotNull(pool.CreateMethod);
            Assert.Null(pool.Creator);
        }

        [Fact]
        public void Pool_WithConstructorPrewarm_InitializesImmediately()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(10, creator, preWarm: true);

            Assert.Equal(10, pool.AvailableItems);
            Assert.Equal(10, creator.CreateCount);
        }

        [Fact]
        public void Pool_MultipleGetPut_MaintainsIntegrity()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            var items = new List<TestItem>();

            // Get 5 items
            for (int i = 0; i < 5; i++)
                items.Add(pool.Get());

            Assert.Equal(0, pool.AvailableItems);

            // Put 3 back
            for (int i = 0; i < 3; i++)
                pool.Put(items[i]);

            Assert.Equal(3, pool.AvailableItems);

            // Get 2 more
            pool.Get();
            pool.Get();

            Assert.Equal(1, pool.AvailableItems);
        }

        [Fact]
        public void ToString_ReturnsInfo()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            pool.PreWarm(3);

            var str = pool.ToString();

            Assert.Contains("Size", str);
            Assert.Contains("10", str);
            Assert.Contains("3", str);
        }

        [Fact]
        public void Pool_ResizeBehavior_MultipleExpansions()
        {
            // Test that pool doubles in size on each expansion
            var pool = new Pool<TestItem>(4, () => new TestItem());

            // First expansion: 4 -> 8 (fill 4, add 5th triggers expansion)
            for (int i = 0; i < 5; i++)
                pool.Put(new TestItem());

            int sizeAfterFirst = pool.Size;

            // Second expansion: 8 -> 16 (fill remaining 3, add 4 more triggers expansion at item 9)
            for (int i = 0; i < 5; i++)
                pool.Put(new TestItem());

            int sizeAfterSecond = pool.Size;

            // Pool should double in size: 4 -> 8 -> 16
            Assert.Equal(8, sizeAfterFirst);
            Assert.Equal(16, sizeAfterSecond);
        }

        [Fact]
        public void Pool_OnRemoveEvent_FiredOnDispose()
        {
            var pool = new Pool<TestItem>();
            bool eventFired = false;
            Type receivedType = null;

            pool.OnRemove += (sender, type) =>
            {
                eventFired = true;
                receivedType = type;
            };

            pool.Dispose();

            Assert.True(eventFired);
            Assert.Equal(typeof(TestItem), receivedType);
        }

        [Fact]
        public void Pool_CreatorLifecycle_FullCycle()
        {
            var creator = new TestCreator();
            var pool = new Pool<TestItem>(5, creator);

            // Create and put
            pool.PreWarm(2);
            Assert.Equal(2, creator.CreateCount);
            Assert.Equal(2, creator.ToPoolCount);

            // Get from pool
            var item1 = pool.Get();
            Assert.Equal(1, creator.FromPoolCount);
            Assert.True(item1.IsActive);

            // Put back
            pool.Put(item1);
            Assert.Equal(3, creator.ToPoolCount);
            Assert.False(item1.IsActive);

            // Clear
            pool.Clear();
            Assert.Equal(2, creator.DisposeCount);
        }

        [Fact]
        public void Pool_NullSafety_HandlesNullCreator()
        {
            var pool = new Pool<TestItem>(10, () => new TestItem());
            // Creator is null, should work fine

            var item = pool.Get();
            Assert.NotNull(item);

            pool.Put(item);
            Assert.Equal(1, pool.AvailableItems);
        }
    }

    public class PoolsTests
    {
        private class TestPoolItem
        {
            public int Value { get; set; }
        }

        private class TestItem
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Singleton_ReturnsInstance()
        {
            var instance = Pools.I;

            Assert.NotNull(instance);
        }

        [Fact]
        public void Singleton_ReturnsSameInstance()
        {
            var instance1 = Pools.I;
            var instance2 = Pools.I;

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void Get_DefaultCreator_CreatesPool()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state

            var pool = pools.Get<TestPoolItem>();

            Assert.NotNull(pool);
            Assert.NotNull(pool.CreateMethod);
        }

        [Fact]
        public void Get_SameTypeTwice_ReturnsSamePool()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state

            var pool1 = pools.Get<TestPoolItem>();
            var pool2 = pools.Get<TestPoolItem>();

            Assert.Same(pool1, pool2);
        }

        [Fact]
        public void Get_WithCreator_UsesCreator()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state
            var creator = new TestCreator();

            var pool = pools.Get<TestItem>(10, creator);

            Assert.Same(creator, pool.Creator);
        }

        [Fact]
        public void Get_WithCreateMethod_UsesMethod()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state
            Func<TestPoolItem> method = () => new TestPoolItem { Value = 42 };

            var pool = pools.Get<TestPoolItem>(10, method);

            Assert.Same(method, pool.CreateMethod);
        }

        [Fact]
        public void Get_WithPrewarm_PrewarmsPool()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state

            var pool = pools.Get<TestPoolItem>(10, prewarm: true);

            Assert.Equal(10, pool.AvailableItems);
        }

        [Fact]
        public void Has_ReturnsTrueWhenExists()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state
            pools.Get<TestPoolItem>();

            var exists = pools.Has<TestPoolItem>();

            Assert.True(exists);
        }

        [Fact]
        public void Has_ReturnsFalseWhenNotExists()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state

            var exists = pools.Has<PoolsTests>(); // Using test class itself

            Assert.False(exists);
        }

        [Fact]
        public void ClearAll_ClearsAllPools()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state
            var pool1 = pools.Get<TestPoolItem>();
            var pool2 = pools.Get<TestItem>();

            pool1.PreWarm(5);
            pool2.PreWarm(3);

            pools.ClearAll();

            Assert.True(pool1.IsEmpty);
            Assert.True(pool2.IsEmpty);
        }

        [Fact]
        public void ClearAll_WithShrink_ShrinksAllPools()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state
            var pool = pools.Get<TestPoolItem>(10);
            pool.PreWarm(20); // Expand

            pools.ClearAll(shrink: true);

            Assert.Equal(10, pool.Size);
        }

        [Fact]
        public void DisposeAll_RemovesAllPools()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Ensure clean state
            pools.Get<TestPoolItem>();

            pools.DisposeAll();

            Assert.Equal(0, pools.NumPools);
        }

        [Fact]
        public void NumPools_ReflectsCount()
        {
            var pools = Pools.I;
            pools.DisposeAll(); // Start fresh

            Assert.Equal(0, pools.NumPools);

            pools.Get<TestPoolItem>();
            Assert.Equal(1, pools.NumPools);

            pools.Get<TestItem>();
            Assert.Equal(2, pools.NumPools);
        }

        private class TestCreator : ICreator<TestItem>
        {
            public TestItem OnCreate() => new TestItem();
            public void OnToPool(TestItem t) { }
            public void OnFromPool(TestItem t) { }
            public void OnDispose(TestItem t) { }
            public void Dispose() { }
        }
    }
}
