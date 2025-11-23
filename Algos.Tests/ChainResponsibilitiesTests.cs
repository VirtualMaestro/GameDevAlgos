using System;
using System.Collections.Generic;
using Algos.Source.Architectural;
using Xunit;

namespace Algos.Tests
{
    public class ChainResponsibilitiesTests
    {
        // Test helper class
        private class TestResponsibility : IResponsibility
        {
            public string Name { get; set; }
            public Func<object[], bool> CanProcessFunc { get; set; }
            public Action<object[]> ProcessAction { get; set; }
            public int ProcessCallCount { get; private set; }

            public TestResponsibility(string name)
            {
                Name = name;
                ProcessCallCount = 0;
            }

            public bool CanProcess(params object[] list)
            {
                return CanProcessFunc?.Invoke(list) ?? false;
            }

            public void Process(params object[] list)
            {
                ProcessCallCount++;
                ProcessAction?.Invoke(list);
            }
        }

        [Fact]
        public void Constructor_CreatesEmptyChain()
        {
            var chain = new ChainResponsibilities(ChainMode.First);
            Assert.Equal(0, chain.NumResponsibilities);
        }

        [Fact]
        public void AddResponsibility_IncreasesCount()
        {
            var chain = new ChainResponsibilities();
            var resp = new TestResponsibility("Test");

            chain.AddResponsibility(resp);

            Assert.Equal(1, chain.NumResponsibilities);
        }

        [Fact]
        public void AddResponsibility_MultipleItems_IncreasesCount()
        {
            var chain = new ChainResponsibilities();

            chain.AddResponsibility(new TestResponsibility("R1"));
            chain.AddResponsibility(new TestResponsibility("R2"));
            chain.AddResponsibility(new TestResponsibility("R3"));

            Assert.Equal(3, chain.NumResponsibilities);
        }

        [Fact]
        public void FirstMode_StopsAfterFirstMatch()
        {
            var chain = new ChainResponsibilities(ChainMode.First);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => false
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };
            var resp3 = new TestResponsibility("R3")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);
            chain.AddResponsibility(resp3);

            var result = chain.Process("test");

            Assert.True(result);
            Assert.Equal(0, resp1.ProcessCallCount);
            Assert.Equal(1, resp2.ProcessCallCount);
            Assert.Equal(0, resp3.ProcessCallCount); // Should not be called
        }

        [Fact]
        public void FirstMode_ReturnsFalseWhenNoMatch()
        {
            var chain = new ChainResponsibilities(ChainMode.First);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => false
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => false
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);

            var result = chain.Process("test");

            Assert.False(result);
            Assert.Equal(0, resp1.ProcessCallCount);
            Assert.Equal(0, resp2.ProcessCallCount);
        }

        [Fact]
        public void AllMode_ProcessesAllMatchingResponsibilities()
        {
            var chain = new ChainResponsibilities(ChainMode.All);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => false
            };
            var resp3 = new TestResponsibility("R3")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);
            chain.AddResponsibility(resp3);

            var result = chain.Process("test");

            Assert.True(result);
            Assert.Equal(1, resp1.ProcessCallCount);
            Assert.Equal(0, resp2.ProcessCallCount);
            Assert.Equal(1, resp3.ProcessCallCount);
        }

        [Fact]
        public void AllMode_ReturnsFalseWhenNoneMatch()
        {
            var chain = new ChainResponsibilities(ChainMode.All);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => false
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => false
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);

            var result = chain.Process("test");

            Assert.False(result);
        }

        [Fact]
        public void StopIfFailMode_StopsAtFirstFailure()
        {
            var chain = new ChainResponsibilities(ChainMode.StopIfFail);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => false  // This should stop the chain
            };
            var resp3 = new TestResponsibility("R3")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);
            chain.AddResponsibility(resp3);

            var result = chain.Process("test");

            Assert.False(result);
            Assert.Equal(1, resp1.ProcessCallCount);
            Assert.Equal(0, resp2.ProcessCallCount); // CanProcess returns false
            Assert.Equal(0, resp3.ProcessCallCount); // Should not be called
        }

        [Fact]
        public void StopIfFailMode_ProcessesAllWhenAllCanProcess()
        {
            var chain = new ChainResponsibilities(ChainMode.StopIfFail);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };
            var resp3 = new TestResponsibility("R3")
            {
                CanProcessFunc = (args) => true,
                ProcessAction = (args) => { }
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);
            chain.AddResponsibility(resp3);

            var result = chain.Process("test");

            Assert.True(result);
            Assert.Equal(1, resp1.ProcessCallCount);
            Assert.Equal(1, resp2.ProcessCallCount);
            Assert.Equal(1, resp3.ProcessCallCount);
        }

        [Fact]
        public void FirstNoOrderMode_UsesLRUCache()
        {
            var chain = new ChainResponsibilities(ChainMode.FirstNoOrder);
            var processedItems = new List<string>();

            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => args[0].ToString() == "item1",
                ProcessAction = (args) => processedItems.Add("R1")
            };
            var resp2 = new TestResponsibility("R2")
            {
                CanProcessFunc = (args) => args[0].ToString() == "item2",
                ProcessAction = (args) => processedItems.Add("R2")
            };

            chain.AddResponsibility(resp1);
            chain.AddResponsibility(resp2);

            // First call - should find R2 in the chain
            var result1 = chain.Process("item2");
            Assert.True(result1);
            Assert.Single(processedItems);
            Assert.Equal("R2", processedItems[0]);

            // Second call - should find R2 in cache (testing cache works)
            processedItems.Clear();
            var result2 = chain.Process("item2");
            Assert.True(result2);
            Assert.Single(processedItems);
            Assert.Equal("R2", processedItems[0]);
        }

        [Fact]
        public void FirstNoOrderMode_ReturnsFalseWhenNoMatch()
        {
            var chain = new ChainResponsibilities(ChainMode.FirstNoOrder);
            var resp1 = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) => false
            };

            chain.AddResponsibility(resp1);

            var result = chain.Process("test");

            Assert.False(result);
            Assert.Equal(0, resp1.ProcessCallCount);
        }

        [Fact]
        public void Process_PassesParametersCorrectly()
        {
            var chain = new ChainResponsibilities(ChainMode.First);
            object[] receivedParams = null;

            var resp = new TestResponsibility("R1")
            {
                CanProcessFunc = (args) =>
                {
                    receivedParams = args;
                    return true;
                },
                ProcessAction = (args) => { }
            };

            chain.AddResponsibility(resp);

            chain.Process("param1", 42, true);

            Assert.NotNull(receivedParams);
            Assert.Equal(3, receivedParams.Length);
            Assert.Equal("param1", receivedParams[0]);
            Assert.Equal(42, receivedParams[1]);
            Assert.Equal(true, receivedParams[2]);
        }

        [Fact]
        public void Process_WithEmptyChain_ReturnsFalse()
        {
            var chain = new ChainResponsibilities(ChainMode.First);

            var result = chain.Process("test");

            Assert.False(result);
        }
    }
}
