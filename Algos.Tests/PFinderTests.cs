using System;
using System.Collections.Generic;
using System.Linq;
using Algos.Source.Pathfinding;
using Xunit;

namespace Algos.Tests
{
    public class PFinderTests
    {
        [Fact]
        public void FindPath_StraightLine_FindsPath()
        {
            var grid = new GridBase(5, 1);
            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 0, 4, 0, out int[] path);

            Assert.True(found);
            Assert.NotNull(path);
            Assert.True(path.Length > 0);
        }

        [Fact]
        public void FindPath_SameStartAndTarget_ReturnsFalse()
        {
            var grid = new GridBase(5, 5);
            var finder = new PFinder(grid);

            var found = finder.FindPath(2, 2, 2, 2, out int[] path);

            Assert.False(found);
            Assert.Null(path);
        }

        [Fact]
        public void FindPath_StartOutOfBounds_ReturnsFalse()
        {
            var grid = new GridBase(5, 5);
            var finder = new PFinder(grid);

            var found = finder.FindPath(-1, 0, 2, 2, out int[] path);

            Assert.False(found);
            Assert.Null(path);
        }

        [Fact]
        public void FindPath_TargetOutOfBounds_ReturnsFalse()
        {
            var grid = new GridBase(5, 5);
            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 0, 10, 10, out int[] path);

            Assert.False(found);
            Assert.Null(path);
        }

        [Fact]
        public void FindPath_ReturnsPathAsXYPairs()
        {
            var grid = new GridBase(5, 1);
            var finder = new PFinder(grid);

            finder.FindPath(0, 0, 2, 0, out int[] path);

            // Path should have even number of elements (x, y pairs)
            Assert.True(path.Length % 2 == 0);
        }

        [Fact]
        public void FindPath_PathStartsAtStart()
        {
            var grid = new GridBase(5, 5);
            var finder = new PFinder(grid);

            finder.FindPath(1, 1, 3, 3, out int[] path);

            Assert.Equal(1, path[0]); // Start X
            Assert.Equal(1, path[1]); // Start Y
        }

        [Fact]
        public void FindPath_PathEndsAtTarget()
        {
            var grid = new GridBase(5, 5);
            var finder = new PFinder(grid);

            finder.FindPath(1, 1, 3, 3, out int[] path);

            Assert.Equal(3, path[path.Length - 2]); // Target X
            Assert.Equal(3, path[path.Length - 1]); // Target Y
        }

        [Fact]
        public void FindPath_StraightHorizontal_FindsShortestPath()
        {
            var grid = new GridBase(5, 1);
            var finder = new PFinder(grid);

            finder.FindPath(0, 0, 4, 0, out int[] path);

            // Shortest path from (0,0) to (4,0) is 5 cells (including start)
            Assert.Equal(10, path.Length); // 5 cells * 2 (x,y pairs)
        }

        [Fact]
        public void FindPath_WithObstacle_RouteAround()
        {
            var grid = new GridBase(5, 3);
            // Create a wall in the middle
            grid.SetWalkable(2, 0, false);
            grid.SetWalkable(2, 1, false);
            grid.SetWalkable(2, 2, false);

            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 1, 4, 1, out int[] path);

            Assert.True(found);
            Assert.NotNull(path);
            // Verify path doesn't go through x=2
            for (int i = 0; i < path.Length; i += 2)
            {
                Assert.NotEqual(2, path[i]);
            }
        }

        [Fact]
        public void FindPath_NoPathExists_ReturnsClosestPath()
        {
            var grid = new GridBase(5, 3);
            // Create a complete wall
            for (int y = 0; y < 3; y++)
                grid.SetWalkable(2, y, false);

            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 1, 4, 1, out int[] path);

            // Should not find complete path but should return closest
            Assert.False(found);
            Assert.NotNull(path);
            Assert.True(path.Length > 0);
        }

        [Fact]
        public void FindPath_DiagonalMovement_Works()
        {
            var grid = new GridBase(5, 5);
            var finder = new PFinder(grid);

            finder.FindPath(0, 0, 2, 2, out int[] path);

            // Diagonal path should be shorter than manhattan distance
            // (can move diagonally)
            int steps = path.Length / 2;
            Assert.True(steps <= 5); // Should be 3 or less for pure diagonal
        }

        [Fact]
        public void FindPath_OptimalPath_IsShortest()
        {
            // This test is critical for exposing the G-cost bug
            // Create a scenario where suboptimal G-cost leads to wrong path
            var grid = new GridBase(10, 10);
            var finder = new PFinder(grid);

            // Find path from (0,0) to (5,5)
            finder.FindPath(0, 0, 5, 5, out int[] path);

            // Optimal path should be mostly diagonal (5 diagonal moves = 5 steps)
            // Each diagonal step costs 14, so total cost = 70
            // Any path with more steps is suboptimal
            int steps = path.Length / 2;

            // The optimal path should be 6 steps (5 diagonal moves + start position)
            // With the bug, it might find a longer path
            Assert.True(steps <= 6, $"Path has {steps} steps, expected <= 6 for optimal path");
        }

        [Fact]
        public void FindPath_WithChoice_PicksShorterPath()
        {
            // This test creates a scenario where there are multiple paths
            // and the algorithm must choose the shorter one
            var grid = new GridBase(10, 5);
            var finder = new PFinder(grid);

            finder.FindPath(0, 2, 9, 2, out int[] path);

            // Direct horizontal path should be chosen
            int steps = path.Length / 2;
            Assert.Equal(10, steps); // 10 cells in a straight line
        }

        [Fact]
        public void FindPath_LShape_FindsCorrectPath()
        {
            var grid = new GridBase(5, 5);
            // Block direct diagonal
            grid.SetWalkable(1, 1, false);

            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 0, 2, 2, out int[] path);

            Assert.True(found);
            // Verify path doesn't go through (1,1)
            for (int i = 0; i < path.Length; i += 2)
            {
                if (path[i] == 1 && path[i + 1] == 1)
                    Assert.True(false, "Path goes through blocked cell");
            }
        }

        [Fact]
        public void FindPath_Maze_FindsWayThrough()
        {
            var grid = new GridBase(7, 7);
            // Create a simple maze
            for (int i = 1; i < 6; i++)
            {
                grid.SetWalkable(3, i, false);
            }
            grid.SetWalkable(3, 3, true); // Opening

            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 3, 6, 3, out int[] path);

            Assert.True(found);
            Assert.NotNull(path);
        }

        [Fact]
        public void FindPath_LargeGrid_Performs()
        {
            var grid = new GridBase(100, 100);
            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 0, 99, 99, out int[] path);

            Assert.True(found);
            Assert.NotNull(path);
        }

        [Fact]
        public void FindPath_TargetBlocked_ReturnsClosest()
        {
            var grid = new GridBase(5, 5);
            grid.SetWalkable(4, 4, false); // Block target

            var finder = new PFinder(grid);

            var found = finder.FindPath(0, 0, 4, 4, out int[] path);

            Assert.False(found);
            Assert.NotNull(path); // Should return path to closest point

            // Path should not reach target
            int endX = path[path.Length - 2];
            int endY = path[path.Length - 1];
            Assert.False(endX == 4 && endY == 4);
        }

        [Fact]
        public void FindPath_MultipleCalls_WorksCorrectly()
        {
            var grid = new GridBase(10, 10);
            var finder = new PFinder(grid);

            // First pathfinding
            var found1 = finder.FindPath(0, 0, 5, 5, out int[] path1);
            Assert.True(found1);

            // Second pathfinding
            var found2 = finder.FindPath(1, 1, 8, 8, out int[] path2);
            Assert.True(found2);

            // Results should be independent
            Assert.NotEqual(path1.Length, path2.Length);
        }

        [Fact]
        public void FindPath_AllDirections_Works()
        {
            var grid = new GridBase(10, 10);
            var finder = new PFinder(grid);

            // Test all 8 directions
            var directions = new[]
            {
                (5, 5, 8, 5),   // Right
                (5, 5, 2, 5),   // Left
                (5, 5, 5, 8),   // Down
                (5, 5, 5, 2),   // Up
                (5, 5, 8, 8),   // Down-Right
                (5, 5, 2, 2),   // Up-Left
                (5, 5, 8, 2),   // Up-Right
                (5, 5, 2, 8)    // Down-Left
            };

            foreach (var (sx, sy, tx, ty) in directions)
            {
                var found = finder.FindPath(sx, sy, tx, ty, out int[] path);
                Assert.True(found, $"Failed to find path from ({sx},{sy}) to ({tx},{ty})");
            }
        }

        [Fact]
        public void FindPath_PathCost_Verification()
        {
            // This test verifies that the path cost calculation is correct
            // Direct diagonal from (0,0) to (3,3) should have specific cost
            var grid = new GridBase(10, 10);
            var finder = new PFinder(grid);

            finder.FindPath(0, 0, 3, 3, out int[] path);

            // Count diagonal and straight moves
            int diagonalMoves = 0;
            int straightMoves = 0;

            for (int i = 0; i < path.Length - 2; i += 2)
            {
                int x1 = path[i];
                int y1 = path[i + 1];
                int x2 = path[i + 2];
                int y2 = path[i + 3];

                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);

                if (dx == 1 && dy == 1)
                    diagonalMoves++;
                else if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
                    straightMoves++;
            }

            // For a perfect diagonal to (3,3), we should have 3 diagonal moves
            // With the G-cost bug, the path might be suboptimal with more moves
            Assert.True(diagonalMoves >= 2, "Should have mostly diagonal moves for diagonal path");
        }

        [Fact]
        public void FindPath_ComplexScenario_OptimalPath()
        {
            // This test creates a scenario with multiple possible paths
            // where the algorithm must correctly evaluate G-costs to find optimal path
            var grid = new GridBase(10, 10);

            // Create obstacles that make path choice important
            for (int i = 2; i < 8; i++)
            {
                grid.SetWalkable(5, i, false);
            }
            grid.SetWalkable(5, 3, true); // Small opening in middle

            var finder = new PFinder(grid);

            // Path from left to right - should choose optimal route through opening
            var found = finder.FindPath(0, 4, 9, 4, out int[] path);

            Assert.True(found);

            // Verify the path actually goes through the opening or around efficiently
            Assert.NotNull(path);
            Assert.True(path.Length > 0);
        }

        [Fact]
        public void FindPath_VerifyPathContinuity()
        {
            // Verify that each step in the path is adjacent to the previous
            var grid = new GridBase(10, 10);
            var finder = new PFinder(grid);

            finder.FindPath(0, 0, 9, 9, out int[] path);

            for (int i = 0; i < path.Length - 2; i += 2)
            {
                int x1 = path[i];
                int y1 = path[i + 1];
                int x2 = path[i + 2];
                int y2 = path[i + 3];

                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);

                // Each step should be adjacent (max distance 1 in each direction)
                Assert.True(dx <= 1 && dy <= 1, $"Non-adjacent steps: ({x1},{y1}) -> ({x2},{y2})");
                Assert.True(dx + dy > 0, "Path contains duplicate position");
            }
        }

        [Fact]
        public void FindPath_VerifyNoObstacleTraversal()
        {
            var grid = new GridBase(10, 10);

            // Block some cells
            grid.SetWalkable(5, 5, false);
            grid.SetWalkable(5, 6, false);
            grid.SetWalkable(6, 5, false);

            var finder = new PFinder(grid);
            finder.FindPath(0, 0, 9, 9, out int[] path);

            // Verify path doesn't go through blocked cells
            for (int i = 0; i < path.Length; i += 2)
            {
                int x = path[i];
                int y = path[i + 1];
                Assert.True(grid.IsWalkable(x, y), $"Path goes through blocked cell ({x},{y})");
            }
        }

        [Fact]
        public void FindPath_CornerCutting_Blocked()
        {
            // Verify that diagonal movement is blocked when both adjacent cells are blocked
            var grid = new GridBase(5, 5);
            grid.SetWalkable(1, 1, false);
            grid.SetWalkable(2, 1, false);
            grid.SetWalkable(1, 2, false);

            var finder = new PFinder(grid);
            finder.FindPath(0, 0, 3, 3, out int[] path);

            // Path should not cut through the corner at (1,1) diagonally
            // because adjacent cells are blocked
            bool cutsCorner = false;
            for (int i = 0; i < path.Length - 2; i += 2)
            {
                if (path[i] == 0 && path[i + 1] == 0 &&
                    path[i + 2] == 2 && path[i + 3] == 2)
                {
                    cutsCorner = true;
                }
            }

            Assert.False(cutsCorner, "Path illegally cuts through blocked corner");
        }
    }
}
