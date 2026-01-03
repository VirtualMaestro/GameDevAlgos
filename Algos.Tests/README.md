# GameDevAlgos Test Suite

Comprehensive test suite for the GameDevAlgos library using xUnit.

## Overview

This test suite provides extensive coverage for all major components:

- **ChainResponsibilitiesTests** - 16 tests covering all chain modes and edge cases
- **LRUCacheTests** - 21 tests covering caching behavior, eviction, and edge cases
- **MinBinaryHeapTests** - 25 tests covering heap operations and invariants
- **PriorityQueueTests** - 28 tests covering queue operations, updates, and boundary conditions
- **GridBaseTests** - 20 tests covering grid operations and import functionality
- **PFinderTests** - 25 tests covering A* pathfinding, optimality, and edge cases
- **PoolTests** - 35+ tests covering pool operations, resizing, and creator lifecycle
- **PoolsTests** - 10 tests covering singleton pool manager

**Total: 180+ comprehensive tests**

## Running Tests

### Using .NET CLI

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~PFinderTests"

# Run tests in parallel
dotnet test --parallel
```

### Using Visual Studio

1. Open `GameDevAlgos.sln`
2. Build the solution
3. Open Test Explorer (Test → Test Explorer)
4. Click "Run All" or select specific tests

### Using Rider

1. Open `GameDevAlgos.sln`
2. Right-click on test project → Run Unit Tests
3. Or use the test runner panel

## Tests That Expose Known Bugs

### 🔴 Critical Bug: A* Pathfinding G-Cost Calculation

**Test:** `PFinderTests.FindPath_OptimalPath_IsShortest`

**Issue:** The A* pathfinding algorithm incorrectly calculates G-costs (actual path cost from start). The G-cost is set to just the movement cost of the last step rather than accumulating the total cost from start.

**Location:** `Algos/Source/Pathfinding/PFinder.cs:183`

**Expected Failure:** Tests verifying optimal path length will fail because the algorithm may find suboptimal paths.

### 🔴 High Bug: Pool Resizing Logic

**Test:** `PoolTests.Put_WhenFull_ExpandsPool`

**Issue:** When the pool is full and needs to resize, it triples the size instead of doubling it.
- Current behavior: Size 4 → calls `_ResizePool(8)` → creates size 12 (4 + 8)
- Expected behavior: Size 4 → creates size 8

**Location:** `Algos/Source/Pools/Pool.cs:94,154-158`

**Expected Failure:** Test expects size 8 but will get size 12.

### 🟡 Moderate Bug: PriorityQueue Boundary Checks

**Test:** `PriorityQueueTests.Update_WithIndexEqualToCount_DoesNotThrow`

**Issue:** Off-by-one errors in boundary checks for the `Update` method. Valid indices are `0` to `_freeIndex - 1`, but the code checks `> _freeIndex` instead of `>= _freeIndex`.

**Location:** `Algos/Source/Pathfinding/PriorityQueue.cs:105,124-125`

**Expected Failure:** May cause array out-of-bounds access in edge cases.

## Test Coverage by Component

### ChainResponsibilities
- ✅ All four chain modes (First, FirstNoOrder, All, StopIfFail)
- ✅ LRU cache integration in FirstNoOrder mode
- ✅ Parameter passing
- ✅ Empty chain handling
- ✅ Multiple responsibilities

### LRUCache
- ✅ Add and eviction behavior
- ✅ LRU ordering
- ✅ Find with predicates
- ✅ Size limits
- ✅ Clear and reuse
- ✅ Edge cases (size 1, null values)

### MinBinaryHeap
- ✅ Insert and maintain heap property
- ✅ Pop in sorted order
- ✅ Peek without removal
- ✅ Auto-expansion
- ✅ Clear, Reset, Resize, Dispose
- ✅ Edge cases (2-3 elements, duplicates, negatives)
- ✅ Stress test with 1000 elements

### PriorityQueue
- ✅ Insert and maintain min-heap
- ✅ Pop in sorted order
- ✅ Update and reheapify
- ✅ HeapIndex tracking
- ✅ Boundary condition handling
- ✅ Auto-expansion
- ✅ Stress test with 1000 elements

### GridBase
- ✅ Grid creation and sizing
- ✅ Walkable/non-walkable cells
- ✅ Import from patterns
- ✅ Edge cases (corners, large grids, single cell)
- ✅ Invalid import handling

### PFinder (A* Pathfinding)
- ✅ Basic pathfinding
- ✅ Optimal path verification (CRITICAL - exposes bug)
- ✅ Obstacle avoidance
- ✅ No path exists handling
- ✅ Diagonal movement
- ✅ Path continuity and validity
- ✅ Boundary checks
- ✅ Corner cutting prevention
- ✅ Multiple directions
- ✅ Large grid performance

### Pool System
- ✅ Get/Put operations
- ✅ Expansion behavior (exposes resizing bug)
- ✅ Creator lifecycle (OnCreate, OnToPool, OnFromPool, OnDispose)
- ✅ PreWarm functionality
- ✅ Clear with/without shrink
- ✅ IsEmpty, IsFull, FreeSlots
- ✅ Multiple expansions
- ✅ Events (OnRemove)

### Pools (Singleton Manager)
- ✅ Singleton pattern
- ✅ Pool creation and retrieval
- ✅ Multiple pool types
- ✅ ClearAll and DisposeAll
- ✅ Has() checking
- ✅ NumPools tracking

## Performance Tests

Several tests verify performance characteristics:

- **MinBinaryHeapTests.Heap_StressTest_LargeNumberOfElements**: 1000 random insertions/extractions
- **PriorityQueueTests.Queue_StressTest_LargeNumberOfElements**: 1000 random insertions/extractions
- **PFinderTests.FindPath_LargeGrid_Performs**: 100×100 grid pathfinding

## Test Conventions

- Tests use descriptive names: `MethodName_Scenario_ExpectedBehavior`
- Each test has a single responsibility
- Tests are independent and can run in any order
- Test data uses fixed seeds for reproducibility
- Helper classes are nested within test classes

## Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Run Tests
  run: dotnet test --no-build --verbosity normal

# Example Azure DevOps
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
```

## Next Steps

1. **Run the tests** - Many will pass, some will fail exposing the bugs
2. **Fix the bugs** - Use failing tests as verification
3. **Verify fixes** - All tests should pass after fixes
4. **Add more tests** - As you find edge cases or add features

## Contributing

When adding new features:

1. Write tests first (TDD approach)
2. Ensure tests cover:
   - Happy path
   - Edge cases
   - Boundary conditions
   - Error conditions
3. Maintain test naming conventions
4. Update this README with new test coverage
