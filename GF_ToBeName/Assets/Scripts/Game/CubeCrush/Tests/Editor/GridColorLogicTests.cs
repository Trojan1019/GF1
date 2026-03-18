using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using CubeCrush.Data;
using NewSideGame;

namespace CubeCrush.Tests
{
    public class GridColorLogicTests
    {
        private GameObject gridManagerObj;
        private GridManager gridManager;

        [SetUp]
        public void Setup()
        {
            gridManagerObj = new GameObject("GridManager");
            gridManager = gridManagerObj.AddComponent<GridManager>();
            gridManager.rows = 8;
            gridManager.cols = 8;
            // MonoSingleton might have an Instance, but we can call InitializeGrid manually
            gridManager.InitializeGrid();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gridManagerObj);
        }

        private BlockShape CreateShape(Color color, params Vector2Int[] cells)
        {
            var shape = ScriptableObject.CreateInstance<BlockShape>();
            shape.blockColor = color;
            shape.SetShape(new List<Vector2Int>(cells));
            return shape;
        }

        [Test]
        public void TestSingleRowClear_ColorChange()
        {
            // Fill row 0 except for x=0
            Color oldColor = Color.blue;
            for (int x = 1; x < gridManager.cols; x++)
            {
                gridManager.grid[x, 0] = 1;
                gridManager.gridColors[x, 0] = oldColor;
            }

            // Place block at (0, 0) to clear the row
            Color newColor = Color.red;
            var shape = CreateShape(newColor, new Vector2Int(0, 0));

            bool success = gridManager.PlaceBlock(shape, new Vector2Int(0, 0), out var clearedRows, out var clearedCols, out var clearedCells);

            Assert.IsTrue(success);
            Assert.AreEqual(1, clearedRows.Count);
            Assert.AreEqual(0, clearedRows[0]);
            Assert.AreEqual(0, clearedCols.Count);

            // Check if all cleared cells have the new color
            Assert.AreEqual(gridManager.cols, clearedCells.Count);
            foreach (var cell in clearedCells)
            {
                Assert.AreEqual(newColor, cell.color, $"Cell at {cell.pos} should have the new preview color.");
            }
        }

        [Test]
        public void TestMultiRowAndColSimultaneousClear_ColorChange()
        {
            // Fill row 0 and col 0, leaving (0,0) empty
            Color oldColor = Color.blue;
            for (int x = 1; x < gridManager.cols; x++)
            {
                gridManager.grid[x, 0] = 1;
                gridManager.gridColors[x, 0] = oldColor;
            }
            for (int y = 1; y < gridManager.rows; y++)
            {
                gridManager.grid[0, y] = 1;
                gridManager.gridColors[0, y] = oldColor;
            }

            // Place block at (0, 0) to clear both row 0 and col 0
            Color newColor = Color.green;
            var shape = CreateShape(newColor, new Vector2Int(0, 0));

            bool success = gridManager.PlaceBlock(shape, new Vector2Int(0, 0), out var clearedRows, out var clearedCols, out var clearedCells);

            Assert.IsTrue(success);
            Assert.AreEqual(1, clearedRows.Count);
            Assert.AreEqual(1, clearedCols.Count);

            // Total cleared cells: rows + cols - 1 (intersection)
            int expectedCount = gridManager.rows + gridManager.cols - 1;
            Assert.AreEqual(expectedCount, clearedCells.Count);

            foreach (var cell in clearedCells)
            {
                Assert.AreEqual(newColor, cell.color, $"Cell at {cell.pos} should have the new preview color.");
            }
        }

        [Test]
        public void TestColorConflict_ExistingColorsAreOverwritten()
        {
            // Fill row 2 with various colors, leaving (0,2) empty
            for (int x = 1; x < gridManager.cols; x++)
            {
                gridManager.grid[x, 2] = 1;
                gridManager.gridColors[x, 2] = new Color(x * 0.1f, 0, 0); // Different colors
            }

            // Place a block that spans 2 cells, clearing row 2
            Color previewColor = Color.yellow;
            var shape = CreateShape(previewColor, new Vector2Int(0, 0), new Vector2Int(0, 1)); 
            // Wait, to clear row 2, we need a cell at (0,2)
            var shapeToPlace = CreateShape(previewColor, new Vector2Int(0, 0));

            bool success = gridManager.PlaceBlock(shapeToPlace, new Vector2Int(0, 2), out var clearedRows, out var clearedCols, out var clearedCells);

            Assert.IsTrue(success);
            Assert.AreEqual(1, clearedRows.Count);

            foreach (var cell in clearedCells)
            {
                Assert.AreEqual(previewColor, cell.color, $"Existing colors should be overwritten by the preview color.");
            }
        }
    }
}
