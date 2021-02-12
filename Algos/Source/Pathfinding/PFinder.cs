using System;
using System.Runtime.CompilerServices;

namespace Algos.Source.Pathfinding
{
    public class PFinder
    {
        private const int DiagonalCost = 14;
        private const int NonDiagonalCost = 10;

        private readonly PriorityQueue<CellInfo> _openList;
        private bool[] _isInCloseList;
        private bool[] _isInOpenList;
        private readonly GridBase _grid;
        private readonly int _gridCols;
        private readonly int _gridRows;
        private readonly int _totalCells;
        private readonly CellInfo[] _cells;
        private int _targetX;
        private int _targetY;

        public PFinder(GridBase grid)
        {
            _grid = grid;
            _gridCols = _grid.Columns;
            _gridRows = _grid.Rows;
            _totalCells = _gridCols * _gridRows;
            _cells = new CellInfo[_totalCells];
            _openList = new PriorityQueue<CellInfo>();
        }

        public bool FindPath(int startX, int startY, int targetX, int targetY, out int[] path)
        {
            path = null;

            if (startX == targetX && startY == targetY ||
                (startX < 0 || startX >= _gridCols) || (startY < 0 || startY >= _gridRows) ||
                (targetX < 0 || targetX >= _gridCols) || (targetY < 0 || targetY >= _gridRows))
            {
                return false;
            }

            _targetX = targetX;
            _targetY = targetY;
            _isInCloseList = new bool[_totalCells];
            _isInOpenList = new bool[_totalCells];
            CellInfo closestPoint = null;
            var smallestHCost = int.MaxValue;
            
            _ProcessNeighbors(_GetCell(_GetCellId(startX, startY), startX, startY));
            
            while (_openList.Pop(out var cell))
            {
                // if reached the target position
                if (cell.X == _targetX && cell.Y == _targetY)
                {
                    // retrace path
                    path = _RetracePath(cell);
                    return true;
                }

                _ProcessNeighbors(cell);

                if (smallestHCost > cell.HCost)
                {
                    smallestHCost = cell.HCost;
                    closestPoint = cell;
                }
            }

            // if path can't be find return a closest path to the target
            path = _RetracePath(closestPoint);
            
            return false;
        }

        private int[] _RetracePath(CellInfo cell)
        {
            var path = new int[cell.OrderNumber * 2];

            for (var i = path.Length - 1; i >= 0; i -= 2)
            {
                path[i] = cell.Y;
                path[i - 1] = cell.X;

                cell = cell.Parent;
            }

            return path;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _ProcessNeighbors(CellInfo currentCell)
        {
            var curX = currentCell.X;
            var curY = currentCell.Y;

            var topIndex = curY + 1;
            var bottomIndex = curY - 1;
            var leftIndex = curX - 1;
            var rightIndex = curX + 1;

            var isTop = topIndex < _gridRows && _grid.IsWalkable(curX, topIndex);
            var isBottom = bottomIndex >= 0 && _grid.IsWalkable(curX, bottomIndex);
            var isLeft = leftIndex >= 0 && _grid.IsWalkable(leftIndex, curY);
            var isRight = rightIndex < _gridCols && _grid.IsWalkable(rightIndex, curY);

            if (isTop)
                _ProcessCell(curX, topIndex, false, currentCell);

            if (isBottom)
                _ProcessCell(curX, bottomIndex, false, currentCell);

            if (isLeft)
                _ProcessCell(leftIndex, curY, false, currentCell);

            if (isRight)
                _ProcessCell(rightIndex, curY, false, currentCell);

            if (isTop && isRight && _grid.IsWalkable(rightIndex, topIndex))
                _ProcessCell(rightIndex, topIndex, true, currentCell);

            if (isTop && isLeft && _grid.IsWalkable(leftIndex, topIndex))
                _ProcessCell(leftIndex, topIndex, true, currentCell);

            if (isBottom && isLeft && _grid.IsWalkable(leftIndex, bottomIndex))
                _ProcessCell(leftIndex, bottomIndex, true, currentCell);

            if (isBottom && isRight && _grid.IsWalkable(rightIndex, bottomIndex))
                _ProcessCell(rightIndex, bottomIndex, true, currentCell);

            _isInCloseList[currentCell.CellIndex] = true;
            _isInOpenList[currentCell.CellIndex] = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _ProcessCell(int x, int y, bool isDiagonal, CellInfo parentCell)
        {
            var cellId = _GetCellId(x, y);
            if (_isInCloseList[cellId]) return;

            var cell = _GetCell(cellId, x, y);

            if (_isInOpenList[cellId])
            {
                var moveCost = _CalculateNeighborGCost(cell, parentCell);
                if (moveCost < cell.GCost)
                {
                    cell.GCost = moveCost;
                    cell.Parent = parentCell;
                    cell.OrderNumber = parentCell.OrderNumber + 1;

                    _openList.Update(cell.HeapIndex);
                }
            }
            else
            {
                cell.Parent = parentCell;
                cell.OrderNumber = parentCell.OrderNumber + 1;

                _AddToOpenList(cell, isDiagonal);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CellInfo _GetCell(int cellId, int x, int y)
        {
            var cell = _cells[cellId];

            if (cell == null)
            {
                // TODO: Create pool
                cell = new CellInfo(cellId, x, y);
                _cells[cellId] = cell;
            }

            return cell;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _AddToOpenList(CellInfo cell, bool isDiagonal)
        {
            cell.GCost = isDiagonal ? DiagonalCost : NonDiagonalCost;
            cell.HCost = _CalculateHeuristicCost(cell.X, cell.Y, _targetX, _targetY);

            _isInOpenList[cell.CellIndex] = true;
            _openList.Insert(cell);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetCellId(int x, int y)
        {
            return x + y * _gridCols;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _CalculateNeighborGCost(CellInfo neighbor, CellInfo parent)
        {
            var isDiagonal = _IsDiagonal(neighbor.X, neighbor.Y, parent.X, parent.Y);
            return (isDiagonal ? DiagonalCost : NonDiagonalCost) + parent.GCost;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _IsDiagonal(int x1, int y1, int x2, int y2)
        {
            var x = Math.Abs(x1 - x2);
            var y = Math.Abs(y1 - y2);
            return x + y == 2;
        }

        /// Used heuristic method 'Diagonal Shortcut'
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _CalculateHeuristicCost(int startX, int startY, int targetX, int targetY)
        {
            var distX = Math.Abs(startX - targetX);
            var distY = Math.Abs(startY - targetY);

            if (distX > distY)
            {
                return DiagonalCost * distY + NonDiagonalCost * (distX - distY);
            }

            return DiagonalCost * distX + NonDiagonalCost * (distY - distX);
        }

        private class CellInfo : PriorityQueue<CellInfo>.IPriorityQueueNode
        {
            // Cell id calculated from X and Y position.
            public readonly int CellIndex;
            public readonly int X;
            public readonly int Y;

            // Dynamic values
            public int HCost;
            public int GCost;
            public int OrderNumber;
            public CellInfo Parent;

            public CellInfo(int cellIndex, int x, int y)
            {
                CellIndex = cellIndex;
                X = x;
                Y = y;
                HCost = 0;
                GCost = 0;
                OrderNumber = 0;
                Parent = null;
            }

            // Data for priority queue
            public int HeapIndex { get; set; }
            public int Value => GCost + HCost;
        }
    }
}