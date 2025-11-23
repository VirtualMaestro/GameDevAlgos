using Algos.Source.Pathfinding;
using Xunit;

namespace Algos.Tests
{
    public class GridBaseTests
    {
        [Fact]
        public void Constructor_CreatesGrid()
        {
            var grid = new GridBase(10, 10);

            Assert.Equal(10, grid.Columns);
            Assert.Equal(10, grid.Rows);
        }

        [Fact]
        public void Constructor_AllCellsWalkableByDefault()
        {
            var grid = new GridBase(5, 5);

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Assert.True(grid.IsWalkable(x, y));
                }
            }
        }

        [Fact]
        public void SetWalkable_SetsCell()
        {
            var grid = new GridBase(5, 5);

            grid.SetWalkable(2, 3, false);

            Assert.False(grid.IsWalkable(2, 3));
        }

        [Fact]
        public void SetWalkable_DoesNotAffectOtherCells()
        {
            var grid = new GridBase(5, 5);

            grid.SetWalkable(2, 3, false);

            Assert.True(grid.IsWalkable(2, 2));
            Assert.True(grid.IsWalkable(2, 4));
            Assert.True(grid.IsWalkable(1, 3));
            Assert.True(grid.IsWalkable(3, 3));
        }

        [Fact]
        public void SetWalkable_CanToggle()
        {
            var grid = new GridBase(5, 5);

            grid.SetWalkable(2, 3, false);
            Assert.False(grid.IsWalkable(2, 3));

            grid.SetWalkable(2, 3, true);
            Assert.True(grid.IsWalkable(2, 3));
        }

        [Fact]
        public void Import_WithValidPattern_UpdatesGrid()
        {
            var grid = new GridBase(3, 3);
            var pattern = new int[]
            {
                1, 0, 1,
                1, 1, 0,
                0, 1, 1
            };

            grid.Import(pattern);

            Assert.True(grid.IsWalkable(0, 0));   // 1
            Assert.False(grid.IsWalkable(1, 0));  // 0
            Assert.True(grid.IsWalkable(2, 0));   // 1
            Assert.True(grid.IsWalkable(0, 1));   // 1
            Assert.True(grid.IsWalkable(1, 1));   // 1
            Assert.False(grid.IsWalkable(2, 1));  // 0
            Assert.False(grid.IsWalkable(0, 2));  // 0
            Assert.True(grid.IsWalkable(1, 2));   // 1
            Assert.True(grid.IsWalkable(2, 2));   // 1
        }

        [Fact]
        public void Import_WithNullPattern_DoesNothing()
        {
            var grid = new GridBase(3, 3);
            grid.SetWalkable(1, 1, false);

            grid.Import(null);

            // Grid should remain unchanged
            Assert.False(grid.IsWalkable(1, 1));
            Assert.True(grid.IsWalkable(0, 0));
        }

        [Fact]
        public void Import_WithWrongSizePattern_DoesNothing()
        {
            var grid = new GridBase(3, 3);
            grid.SetWalkable(1, 1, false);

            var pattern = new int[] { 1, 0, 1 }; // Wrong size

            grid.Import(pattern);

            // Grid should remain unchanged
            Assert.False(grid.IsWalkable(1, 1));
            Assert.True(grid.IsWalkable(0, 0));
        }

        [Fact]
        public void Import_PositiveValuesAreWalkable()
        {
            var grid = new GridBase(3, 1);
            var pattern = new int[] { 1, 5, 100 };

            grid.Import(pattern);

            Assert.True(grid.IsWalkable(0, 0));
            Assert.True(grid.IsWalkable(1, 0));
            Assert.True(grid.IsWalkable(2, 0));
        }

        [Fact]
        public void Import_ZeroAndNegativeValuesAreNotWalkable()
        {
            var grid = new GridBase(4, 1);
            var pattern = new int[] { 0, -1, -5, -100 };

            grid.Import(pattern);

            Assert.False(grid.IsWalkable(0, 0));
            Assert.False(grid.IsWalkable(1, 0));
            Assert.False(grid.IsWalkable(2, 0));
            Assert.False(grid.IsWalkable(3, 0));
        }

        [Fact]
        public void GetFinder_ReturnsPFinder()
        {
            var grid = new GridBase(10, 10);

            var finder = grid.GetFinder();

            Assert.NotNull(finder);
            Assert.IsType<PFinder>(finder);
        }

        [Fact]
        public void GetFinder_ReturnsNewInstanceEachTime()
        {
            var grid = new GridBase(10, 10);

            var finder1 = grid.GetFinder();
            var finder2 = grid.GetFinder();

            Assert.NotSame(finder1, finder2);
        }

        [Fact]
        public void Grid_Large_WorksCorrectly()
        {
            var grid = new GridBase(100, 100);

            grid.SetWalkable(50, 50, false);

            Assert.False(grid.IsWalkable(50, 50));
            Assert.True(grid.IsWalkable(49, 50));
            Assert.True(grid.IsWalkable(51, 50));
        }

        [Fact]
        public void Grid_SingleCell_WorksCorrectly()
        {
            var grid = new GridBase(1, 1);

            Assert.True(grid.IsWalkable(0, 0));

            grid.SetWalkable(0, 0, false);

            Assert.False(grid.IsWalkable(0, 0));
        }

        [Fact]
        public void Grid_RectangularGrid_WorksCorrectly()
        {
            var grid = new GridBase(5, 10);

            Assert.Equal(5, grid.Columns);
            Assert.Equal(10, grid.Rows);

            grid.SetWalkable(4, 9, false); // Last cell
            Assert.False(grid.IsWalkable(4, 9));
        }

        [Fact]
        public void Grid_CornerCells_WorkCorrectly()
        {
            var grid = new GridBase(5, 5);

            // Test all four corners
            grid.SetWalkable(0, 0, false); // Top-left
            grid.SetWalkable(4, 0, false); // Top-right
            grid.SetWalkable(0, 4, false); // Bottom-left
            grid.SetWalkable(4, 4, false); // Bottom-right

            Assert.False(grid.IsWalkable(0, 0));
            Assert.False(grid.IsWalkable(4, 0));
            Assert.False(grid.IsWalkable(0, 4));
            Assert.False(grid.IsWalkable(4, 4));

            // Center should still be walkable
            Assert.True(grid.IsWalkable(2, 2));
        }

        [Fact]
        public void Import_OverwritesPreviousState()
        {
            var grid = new GridBase(2, 2);
            grid.SetWalkable(0, 0, false);
            grid.SetWalkable(1, 1, false);

            var pattern = new int[] { 1, 1, 1, 1 }; // All walkable

            grid.Import(pattern);

            // All cells should now be walkable
            Assert.True(grid.IsWalkable(0, 0));
            Assert.True(grid.IsWalkable(1, 0));
            Assert.True(grid.IsWalkable(0, 1));
            Assert.True(grid.IsWalkable(1, 1));
        }

        [Fact]
        public void Grid_SetMultipleCells_WorksCorrectly()
        {
            var grid = new GridBase(5, 5);

            // Create a cross pattern of non-walkable cells
            for (int i = 0; i < 5; i++)
            {
                grid.SetWalkable(2, i, false); // Vertical line
                grid.SetWalkable(i, 2, false); // Horizontal line
            }

            // Verify the cross pattern
            for (int i = 0; i < 5; i++)
            {
                Assert.False(grid.IsWalkable(2, i));
                Assert.False(grid.IsWalkable(i, 2));
            }

            // Verify corners are still walkable
            Assert.True(grid.IsWalkable(0, 0));
            Assert.True(grid.IsWalkable(4, 0));
            Assert.True(grid.IsWalkable(0, 4));
            Assert.True(grid.IsWalkable(4, 4));
        }
    }
}
