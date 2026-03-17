// using NUnit.Framework;
// using UnityEngine;
// using CubeCrush.Manager;
// using CubeCrush.Data;
// using System.Collections.Generic;
//
// namespace CubeCrush.Tests
// {
//     public class GridManagerTests
//     {
//         private GridManager gridManager;
//
//         [SetUp]
//         public void SetUp()
//         {
//             var go = new GameObject();
//             gridManager = go.AddComponent<GridManager>();
//             // InitializeGrid is called in Awake, but we can call it again to be sure
//             gridManager.InitializeGrid();
//         }
//
//         [TearDown]
//         public void TearDown()
//         {
//             if (gridManager != null) Object.DestroyImmediate(gridManager.gameObject);
//         }
//
//         [Test]
//         public void InitializeGrid_ShouldCreateEmptyGrid()
//         {
//             Assert.IsNotNull(gridManager);
//             Assert.IsTrue(gridManager.CanPlace(CreateShape(1, 1), new Vector2Int(0, 0)));
//         }
//
//         [Test]
//         public void PlaceBlock_ShouldOccupySpace()
//         {
//             var shape = CreateShape(1, 1);
//             List<int> rows, cols;
//             bool success = gridManager.PlaceBlock(shape, new Vector2Int(0, 0), out rows, out cols);
//
//             Assert.IsTrue(success);
//             Assert.IsFalse(gridManager.CanPlace(shape, new Vector2Int(0, 0)));
//         }
//
//         [Test]
//         public void ClearLine_ShouldClearFullRow()
//         {
//             // Fill a row except last cell
//             for (int x = 0; x < 7; x++)
//             {
//                 gridManager.PlaceBlock(CreateShape(1, 1), new Vector2Int(x, 0), out _, out _);
//             }
//
//             // Place last block
//             List<int> rows, cols;
//             gridManager.PlaceBlock(CreateShape(1, 1), new Vector2Int(7, 0), out rows, out cols);
//
//             Assert.Contains(0, rows);
//         }
//
//         private BlockShape CreateShape(int w, int h)
//         {
//             var shape = ScriptableObject.CreateInstance<BlockShape>();
//             var cells = new List<Vector2Int>();
//             for (int x = 0; x < w; x++)
//             {
//                 for (int y = 0; y < h; y++)
//                 {
//                     cells.Add(new Vector2Int(x, y));
//                 }
//             }
//             shape.SetShape(cells);
//             return shape;
//         }
//     }
// }
