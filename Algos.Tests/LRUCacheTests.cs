using System;
using System.Collections.Generic;
using Algos.Source.Caches;
using Xunit;

namespace Algos.Tests
{
    public class LRUCacheTests
    {
        [Fact]
        public void Constructor_DefaultSize_CreatesCache()
        {
            var cache = new LRUCache<string>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Constructor_CustomSize_CreatesCache()
        {
            var cache = new LRUCache<string>(10);
            Assert.NotNull(cache);
        }

        [Fact]
        public void Add_SingleItem_StoresItem()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("item1");

            var found = cache.Find((item, args) => item == "item1");
            Assert.True(found);
        }

        [Fact]
        public void Add_MultipleItems_StoresAllItems()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("item1");
            cache.Add("item2");
            cache.Add("item3");

            Assert.True(cache.Find((item, args) => item == "item1"));
            Assert.True(cache.Find((item, args) => item == "item2"));
            Assert.True(cache.Find((item, args) => item == "item3"));
        }

        [Fact]
        public void Add_SameItemTwice_UpdatesPosition()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("item1");
            cache.Add("item2");
            cache.Add("item1"); // Re-add item1, should move to front

            // Both items should still be in cache
            Assert.True(cache.Find((item, args) => item == "item1"));
            Assert.True(cache.Find((item, args) => item == "item2"));
        }

        [Fact]
        public void Add_ExceedsCapacity_EvictsLRU()
        {
            var cache = new LRUCache<string>(3);
            cache.Add("item1");
            cache.Add("item2");
            cache.Add("item3");
            cache.Add("item4"); // Should evict item1

            Assert.False(cache.Find((item, args) => item == "item1")); // Should be evicted
            Assert.True(cache.Find((item, args) => item == "item2"));
            Assert.True(cache.Find((item, args) => item == "item3"));
            Assert.True(cache.Find((item, args) => item == "item4"));
        }

        [Fact]
        public void Add_ExceedsCapacity_EvictsCorrectItem()
        {
            var cache = new LRUCache<string>(3);
            cache.Add("item1");
            cache.Add("item2");
            cache.Add("item3");
            cache.Add("item2"); // Access item2, moving it to front
            cache.Add("item4"); // Should evict item1 (not item2, since item2 was just accessed)

            Assert.False(cache.Find((item, args) => item == "item1")); // Should be evicted
            Assert.True(cache.Find((item, args) => item == "item2"));  // Should still be in cache
            Assert.True(cache.Find((item, args) => item == "item3"));
            Assert.True(cache.Find((item, args) => item == "item4"));
        }

        [Fact]
        public void Find_WithMatchingPredicate_ReturnsTrue()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("test");

            var result = cache.Find((item, args) => item == "test");

            Assert.True(result);
        }

        [Fact]
        public void Find_WithNonMatchingPredicate_ReturnsFalse()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("test");

            var result = cache.Find((item, args) => item == "notfound");

            Assert.False(result);
        }

        [Fact]
        public void Find_WithParameters_PassesParametersToDelegate()
        {
            var cache = new LRUCache<int>(5);
            cache.Add(10);
            cache.Add(20);
            cache.Add(30);

            var result = cache.Find((item, args) =>
            {
                var threshold = (int)args[0];
                return item > threshold;
            }, 15);

            Assert.True(result); // Should find 20 or 30
        }

        [Fact]
        public void Find_ExecutesActionInDelegate()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("item1");
            cache.Add("item2");

            var foundItem = "";
            var result = cache.Find((item, args) =>
            {
                if (item == "item2")
                {
                    foundItem = item;
                    return true;
                }
                return false;
            });

            Assert.True(result);
            Assert.Equal("item2", foundItem);
        }

        [Fact]
        public void Find_OnEmptyCache_ReturnsFalse()
        {
            var cache = new LRUCache<string>(5);

            var result = cache.Find((item, args) => item == "test");

            Assert.False(result);
        }

        [Fact]
        public void Find_UpdatesItemPosition()
        {
            var cache = new LRUCache<string>(3);
            cache.Add("item1");
            cache.Add("item2");
            cache.Add("item3");

            // Access item1, moving it to front
            cache.Find((item, args) => item == "item1");

            // Add new item, should evict item2 (not item1, since it was just accessed)
            cache.Add("item4");

            Assert.True(cache.Find((item, args) => item == "item1")); // Should still be in cache
            Assert.False(cache.Find((item, args) => item == "item2")); // Should be evicted
            Assert.True(cache.Find((item, args) => item == "item3"));
            Assert.True(cache.Find((item, args) => item == "item4"));
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("item1");
            cache.Add("item2");
            cache.Add("item3");

            cache.Clear();

            Assert.False(cache.Find((item, args) => item == "item1"));
            Assert.False(cache.Find((item, args) => item == "item2"));
            Assert.False(cache.Find((item, args) => item == "item3"));
        }

        [Fact]
        public void Clear_AllowsReuse()
        {
            var cache = new LRUCache<string>(5);
            cache.Add("item1");
            cache.Clear();
            cache.Add("item2");

            Assert.False(cache.Find((item, args) => item == "item1"));
            Assert.True(cache.Find((item, args) => item == "item2"));
        }

        [Fact]
        public void Cache_WorksWithComplexTypes()
        {
            var cache = new LRUCache<(int, string)>(5);
            cache.Add((1, "one"));
            cache.Add((2, "two"));

            var found = cache.Find((item, args) => item.Item1 == 2 && item.Item2 == "two");

            Assert.True(found);
        }

        [Fact]
        public void Cache_MaintainsOrderCorrectly()
        {
            var cache = new LRUCache<int>(5);
            var visitedItems = new List<int>();

            cache.Add(1);
            cache.Add(2);
            cache.Add(3);
            cache.Add(4);
            cache.Add(5);

            // Find iterates from front (most recently added) to back
            // After adds: front=5, 4, 3, 2, back=1
            // Searching for 2 will visit: 5, 4, 3, 2 (and stop)
            cache.Find((item, args) =>
            {
                visitedItems.Add(item);
                return item == 2;
            });

            // Verify iteration order: most recent first
            Assert.Equal(5, visitedItems[0]);
            Assert.Equal(4, visitedItems.Count); // Should visit 5, 4, 3, 2
            Assert.Equal(2, visitedItems[3]); // Last visited should be 2
        }

        [Fact]
        public void Cache_SizeOf1_WorksCorrectly()
        {
            var cache = new LRUCache<string>(1);
            cache.Add("item1");
            cache.Add("item2"); // Should evict item1

            Assert.False(cache.Find((item, args) => item == "item1"));
            Assert.True(cache.Find((item, args) => item == "item2"));
        }

        [Fact]
        public void Cache_HandlesNullValues()
        {
            var cache = new LRUCache<string>(5);
            cache.Add(null);

            var found = cache.Find((item, args) => item == null);

            Assert.True(found);
        }
    }
}
