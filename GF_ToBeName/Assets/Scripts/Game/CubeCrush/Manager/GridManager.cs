using UnityEngine;
using System.Collections.Generic;
using CubeCrush.Data;

namespace NewSideGame
{
    public class ClearedCellInfo
    {
        public Vector2Int pos;
        public Color color;
    }

    public class GridManager : MonoSingleton<GridManager>
    {
        [Header("Grid Settings")] public int rows = 8;
        public int cols = 8;

        // 0: Empty, 1: Filled
        public int[,] grid;
        public Color[,] gridColors;

        private void Awake()
        {
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            grid = new int[cols, rows];
            gridColors = new Color[cols, rows];
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    grid[x, y] = 0;
                    gridColors[x, y] = Color.clear;
                }
            }

            NotifyGridUpdate();
        }

        public bool CanPlace(BlockShape shape, Vector2Int pos)
        {
            if (shape == null) return false;
            foreach (var cell in shape.cells)
            {
                int targetX = pos.x + cell.x;
                int targetY = pos.y + cell.y;

                if (targetX < 0 || targetX >= cols || targetY < 0 || targetY >= rows)
                    return false;

                if (grid[targetX, targetY] != 0)
                    return false;
            }

            return true;
        }

        public void GetPredictedClearLines(BlockShape shape, Vector2Int pos, out List<int> fullRows,
            out List<int> fullCols)
        {
            fullRows = new List<int>();
            fullCols = new List<int>();
            if (!CanPlace(shape, pos)) return;

            // Simulate placement
            foreach (var cell in shape.cells)
            {
                grid[pos.x + cell.x, pos.y + cell.y] = 1;
            }

            CheckLines(out fullRows, out fullCols);

            // Revert placement
            foreach (var cell in shape.cells)
            {
                grid[pos.x + cell.x, pos.y + cell.y] = 0;
            }
        }

        public bool PlaceBlock(BlockShape shape, Vector2Int pos, out List<int> clearedRows, out List<int> clearedCols,
            out List<ClearedCellInfo> clearedCells)
        {
            clearedRows = new List<int>();
            clearedCols = new List<int>();
            clearedCells = new List<ClearedCellInfo>();

            if (!CanPlace(shape, pos)) return false;

            foreach (var cell in shape.cells)
            {
                grid[pos.x + cell.x, pos.y + cell.y] = 1;
                gridColors[pos.x + cell.x, pos.y + cell.y] = shape.blockColor;
            }

            CheckLines(out clearedRows, out clearedCols);
            
            // Collect cleared cells info before clearing
            foreach (int y in clearedRows)
            {
                for (int x = 0; x < cols; x++)
                {
                    gridColors[x, y] = shape.blockColor;
                    clearedCells.Add(new ClearedCellInfo { pos = new Vector2Int(x, y), color = gridColors[x, y] });
                }
            }

            foreach (int x in clearedCols)
            {
                for (int y = 0; y < rows; y++)
                {
                    gridColors[x, y] = shape.blockColor;
                    // Avoid duplicate cells at intersection
                    if (!clearedRows.Contains(y))
                    {
                        clearedCells.Add(new ClearedCellInfo
                            { pos = new Vector2Int(x, y), color = gridColors[x, y] });
                    }
                }
            }

            ClearLines(clearedRows, clearedCols);

            NotifyGridUpdate();
            return true;
        }

        private void CheckLines(out List<int> fullRows, out List<int> fullCols)
        {
            fullRows = new List<int>();
            fullCols = new List<int>();

            for (int y = 0; y < rows; y++)
            {
                bool isFull = true;
                for (int x = 0; x < cols; x++)
                {
                    if (grid[x, y] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull) fullRows.Add(y);
            }

            for (int x = 0; x < cols; x++)
            {
                bool isFull = true;
                for (int y = 0; y < rows; y++)
                {
                    if (grid[x, y] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull) fullCols.Add(x);
            }
        }

        private void ClearLines(List<int> rowsToClear, List<int> colsToClear)
        {
            foreach (int y in rowsToClear)
            {
                for (int x = 0; x < cols; x++) grid[x, y] = 0;
            }

            foreach (int x in colsToClear)
            {
                for (int y = 0; y < rows; y++) grid[x, y] = 0;
            }
        }

        public bool CheckGameOver(List<BlockShape> availableShapes)
        {
            if (availableShapes == null || availableShapes.Count == 0) return false;

            bool anyShapeValid = false;
            foreach (var shape in availableShapes)
            {
                if (shape != null)
                {
                    anyShapeValid = true;
                    break;
                }
            }

            if (!anyShapeValid) return false; // If all shapes used, not game over

            foreach (var shape in availableShapes)
            {
                if (shape == null) continue;

                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        if (CanPlace(shape, new Vector2Int(x, y)))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void NotifyGridUpdate()
        {
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
        }
    }
}