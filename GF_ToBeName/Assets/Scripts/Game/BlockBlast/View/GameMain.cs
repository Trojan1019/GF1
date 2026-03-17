using UnityEngine;
using System.Collections.Generic;
using BlockBlast.Manager;
using BlockBlast.Data;
using NewSideGame;

namespace BlockBlast.View
{
    public class GameMain : MonoBehaviour
    {
        public static GameMain Instance;
        private void Awake()
        {
            Instance = this;
        }

        [Header("Shape Configs")] public List<BlockShape> availableShapes = new List<BlockShape>();

        [Header("Scene References")] public Transform gridOrigin; // Bottom-left of the grid
        public Transform spawnOrigin; // Center/Start of spawn area
        public float cellSize = 1.0f;
        public float spawnSpacing = 3.0f;

        [Header("Prefabs")]
        //public SceneGridCell gridCellPrefab;
        // public SceneBlockItem blockItemPrefab;
        //public BlockUnit blockUnitPrefab; // Single block unit (sprite)
        private SceneGridCell[,] sceneGrid;

        private List<SceneBlockItem> sceneSpawnBlocks = new List<SceneBlockItem>();

        private void OnEnable()
        {
            EventManager.Instance.AddEventListener(Constant.Event.BlockBlastGridUpdated, OnGridUpdated);
            EventManager.Instance.AddEventListener(Constant.Event.BlockBlastSpawnUpdated, OnSpawnUpdated);
            EventManager.Instance.AddEventListener(Constant.Event.BlockBlastGameStart, OnGameStart);
        }

        private void OnDisable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RemoveEventListener(Constant.Event.BlockBlastGridUpdated, OnGridUpdated);
                EventManager.Instance.RemoveEventListener(Constant.Event.BlockBlastSpawnUpdated, OnSpawnUpdated);
                EventManager.Instance.RemoveEventListener(Constant.Event.BlockBlastGameStart, OnGameStart);
            }
        }

        private void OnGameStart(params object[] args)
        {
            GenerateGrid();
            UpdateSpawnArea();
        }

        private void GenerateGrid()
        {
            if (sceneGrid != null)
            {
                foreach (var cell in sceneGrid)
                {
                    if (cell != null) Destroy(cell.gameObject);
                }
            }

            int rows = GridManager.Instance.rows;
            int cols = GridManager.Instance.cols;
            sceneGrid = new SceneGridCell[cols, rows];

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var cell = GameEntry.PoolManager.SpawnSync<SceneGridCell>(31000);
                    cell.transform.SetParent(gridOrigin, false);
                    cell.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0);
                    cell.Init(x, y);
                    sceneGrid[x, y] = cell;
                }
            }
        }

        private void OnGridUpdated(params object[] args)
        {
            if (sceneGrid == null) return;
            int[,] data = GridManager.Instance.grid;
            int rows = data.GetLength(1);
            int cols = data.GetLength(0);

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    bool filled = data[x, y] != 0;
                    sceneGrid[x, y].SetState(filled, filled ? Color.blue : Color.gray);
                }
            }
        }

        private void OnSpawnUpdated(params object[] args)
        {
            UpdateSpawnArea();
        }

        private void UpdateSpawnArea()
        {
            foreach (var block in sceneSpawnBlocks)
            {
                if (block != null) Destroy(block.gameObject);
            }

            sceneSpawnBlocks.Clear();

            List<BlockShape> shapes = BlockSpawner.Instance.currentShapes;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] == null) continue;

                var block = GameEntry.PoolManager.SpawnSync<SceneBlockItem>(31001);
                block.transform.SetParent(spawnOrigin, false);
                block.transform.localPosition = new Vector3((i - 1) * spawnSpacing, 0, 0);
                block.Init(shapes[i], i);
                sceneSpawnBlocks.Add(block);
            }
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 localPos = gridOrigin.InverseTransformPoint(worldPos);
            int x = Mathf.RoundToInt(localPos.x / cellSize);
            int y = Mathf.RoundToInt(localPos.y / cellSize);
            return new Vector2Int(x, y);
        }
    }
}