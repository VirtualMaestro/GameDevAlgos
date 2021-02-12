namespace Algos.Source.Pathfinding
{
    public class GridBase
    {
        private readonly bool[] _grid;
        private readonly int _gridSize;
        private readonly int _cols;
        private readonly int _rows;

        public int Columns => _cols;
        public int Rows => _rows;

        public GridBase(int cols, int rows)
        {
            _cols = cols;
            _rows = rows;
            _gridSize = cols * rows;
            _grid = new bool[_gridSize];

            for (var i = 0; i < _gridSize; i++)
                _grid[i] = true;
        }

        public void SetWalkable(int x, int y, bool isWalkable) => _grid[x + y * _cols] = isWalkable;
        public bool IsWalkable(int x, int y) => _grid[x + y * _cols];

        public void Import(int[] pattern)
        {
            if (pattern == null || pattern.Length != _grid.Length)
                return;

            for (var i = 0; i < pattern.Length; i++)
            {
                _grid[i] = pattern[i] > 0;
            }
        }

        public PFinder GetFinder()
        {
            return new PFinder(this);
        }
    }
}
