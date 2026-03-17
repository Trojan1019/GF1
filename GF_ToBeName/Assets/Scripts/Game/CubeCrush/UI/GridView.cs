// using System;
// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using CubeCrush.Data;
// using CubeCrush.Manager;
//
// namespace NewSideGame
// {
//     public class GridView : MonoBehaviour
//     {
//         [Header("References")] public Transform gridContainer;
//         public Transform spawnContainer;
//
//         private GridCell[,] uiGrid;
//         private List<BlockItem> uiSpawnBlocks = new List<BlockItem>();
//
//         public void OnEnable()
//         {
//             EventManager.Instance.AddEventListener(Constant.Event.RefreshScore, OnRefreshGrid);
//         }
//
//         public void OnDisable()
//         {
//             EventManager.Instance.RemoveEventListener(Constant.Event.RefreshScore, OnRefreshGrid);
//         }
//
//         public void OnRefreshGrid(params object[] objs)
//         {
//             UpdateGrid(GridManager.Instance.grid);
//             UpdateSpawnArea(BlockSpawner.Instance.currentShapes);
//         }
//
//         public void Start()
//         {
//             // Generate Grid UI if empty
//             if (uiGrid == null)
//             {
//                 GenerateGridUI();
//             }
//         }
//
//         private void GenerateGridUI()
//         {
//             if (gridContainer == null) return;
//
//             // Clear old
//             foreach (Transform child in gridContainer) Destroy(child.gameObject);
//
//             int rows = GridManager.Instance.rows;
//             int cols = GridManager.Instance.cols;
//             uiGrid = new GridCell[cols, rows];
//
//             // Assume GridContainer has a GridLayoutGroup
//             // Or we manually position them. Let's assume GridLayoutGroup is used on GridContainer.
//
//             for (int y = 0; y < rows; y++)
//             {
//                 for (int x = 0; x < cols; x++)
//                 {
//                     GridCell cellObj = GameEntry.PoolManager.SpawnSync<GridCell>(30000);
//                     cellObj.transform.SetParent(gridContainer, false);
//                     cellObj.name = $"Cell_{x}_{y}";
//                     cellObj.Init();
//                     uiGrid[x, y] = cellObj;
//                 }
//             }
//         }
//
//         public void UpdateGrid(int[,] gridData)
//         {
//             if (uiGrid == null) return;
//
//             int rows = gridData.GetLength(1);
//             int cols = gridData.GetLength(0);
//
//             for (int x = 0; x < cols; x++)
//             {
//                 for (int y = 0; y < rows; y++)
//                 {
//                     bool filled = gridData[x, y] != 0;
//                     uiGrid[x, y].SetState(filled, filled ? Color.blue : Color.gray); // Simple color
//                 }
//             }
//         }
//
//         public void UpdateSpawnArea(List<BlockShape> shapes)
//         {
//             if (spawnContainer == null) return;
//
//             // Clear old
//             foreach (Transform child in spawnContainer) Destroy(child.gameObject);
//             uiSpawnBlocks.Clear();
//
//             for (int i = 0; i < shapes.Count; i++)
//             {
//                 if (shapes[i] == null) continue;
//
//                 BlockItem blockObj = GameEntry.PoolManager.SpawnSync<BlockItem>(31001);
//                 blockObj.transform.SetParent(spawnContainer, false);
//                 blockObj.Init(shapes[i], i);
//                 uiSpawnBlocks.Add(blockObj);
//             }
//         }
//
//
//         // Called by BlockItem when dropped
//         public bool TryPlaceBlock(BlockItem item)
//         {
//             GridCell closestCell = null;
//             float minDist = float.MaxValue;
//
//             Vector3 blockWorldPos = item.transform.position; // Pivot position
//
//             foreach (var cell in uiGrid)
//             {
//                 float dist = Vector3.Distance(blockWorldPos, cell.transform.position);
//                 if (dist < minDist)
//                 {
//                     minDist = dist;
//                     closestCell = cell;
//                 }
//             }
//
//             // Threshold for snapping (e.g., 50 pixels)
//             if (minDist < 100f && closestCell != null)
//             {
//                 Vector2Int pos = new Vector2Int(closestCell.x, closestCell.y);
//
//                 // Try to place via Manager
//                 // Note: GameLoopManager should verify if valid
//                 if (GridManager.Instance.CanPlace(item.shape, pos))
//                 {
//                     GameLoopManager.Instance.OnBlockPlaced(item.spawnIndex, item.shape, pos);
//                     return true;
//                 }
//             }
//
//             return false;
//         }
//     }
// }