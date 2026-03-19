using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CubeCrush.Manager;
using CubeCrush.Data;
using NewSideGame;
using DG.Tweening;

namespace NewSideGame
{
    public partial class GameMain : MonoBehaviour
    {
        public static GameMain Instance;

        private void Awake()
        {
            Instance = this;
        }

        [Header("Shape Configs")] public List<BlockShape> availableShapes = new List<BlockShape>();

        [Header("Scene References")] public Transform gridOrigin; // Bottom-left of the grid
        public Transform spawnOrigin; // Center/Start of spawn area
        public float cellSize = 3f;
        public float spawnSpacing = 3.0f;
        public float spawnSize = 0.5f;

        [Header("Spawn Settings")] public float spawnAreaWidth = 10f; // 动态生成的区域宽度

        [Header("Hint Settings")] public float hintDelay = 5.0f; // 无操作多久显示提示
        public float hintDuration = 2.0f;
        [Range(0, 1)] public float hintAlpha = 0.5f;

        [Header("Sound Settings")] public int hintSoundId = 10001;

        private float lastInteractionTime;
        private SceneBlockItem currentHint;
        private Vector3 gridStartOffset;

        private SceneBlockItem placementPreview;

        [Header("Prefabs")]
        //public SceneGridCell gridCellPrefab;
        // public SceneBlockItem blockItemPrefab;
        //public BlockUnit blockUnitPrefab; // Single block unit (sprite)
        private SceneGridCell[,] sceneGrid;

        private List<SceneBlockItem> sceneSpawnBlocks = new List<SceneBlockItem>();

        private void OnEnable()
        {
            EventManager.Instance.AddEventListener(Constant.Event.CubeCrushGridUpdated, OnGridUpdated);
            EventManager.Instance.AddEventListener(Constant.Event.CubeCrushSpawnUpdated, OnSpawnUpdated);
            EventManager.Instance.AddEventListener(Constant.Event.CubeCrushGameStart, OnGameStart);
            EventManager.Instance.AddEventListener(Constant.Event.CubeCrushLinesCleared, OnLinesCleared);
            EventManager.Instance.AddEventListener(Constant.Event.GameOverFillAnimation, OnGameOverFillAnimation);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveEventListener(Constant.Event.CubeCrushGridUpdated, OnGridUpdated);
            EventManager.Instance.RemoveEventListener(Constant.Event.CubeCrushSpawnUpdated, OnSpawnUpdated);
            EventManager.Instance.RemoveEventListener(Constant.Event.CubeCrushGameStart, OnGameStart);
            EventManager.Instance.RemoveEventListener(Constant.Event.CubeCrushLinesCleared, OnLinesCleared);
            EventManager.Instance.RemoveEventListener(Constant.Event.GameOverFillAnimation,
                OnGameOverFillAnimation);
        }

        private void Update()
        {
            if (Time.time - lastInteractionTime > hintDelay)
            {
                if (currentHint == null)
                {
                    ShowHint();
                }
            }
        }

        public void OnUserInteraction()
        {
            lastInteractionTime = Time.time;
            HideHint();
        }

        private void ShowHint()
        {
            var shapes = BlockSpawner.Instance.currentShapes;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] == null) continue;

                for (int x = 0; x < GridManager.Instance.cols; x++)
                {
                    for (int y = 0; y < GridManager.Instance.rows; y++)
                    {
                        if (GridManager.Instance.CanPlace(shapes[i], new Vector2Int(x, y)))
                        {
                            CreateHint(shapes[i], new Vector2Int(x, y));
                            GameEntry.Sound.PlaySound(hintSoundId);
                            return;
                        }
                    }
                }
            }
        }

        private void CreateHint(BlockShape shape, Vector2Int pos)
        {
            var hint = GameEntry.PoolManager.SpawnSync<SceneBlockItem>(31001);
            hint.transform.SetParent(gridOrigin, false);
            // 计算提示位置：基于 Grid 的偏移量
            hint.transform.localPosition = gridStartOffset + new Vector3(pos.x * cellSize, pos.y * cellSize, 0);
            hint.Init(shape, -1,false); // index -1 表示这是一个提示，不参与逻辑

            // 设置透明度和动画
            var renderers = hint.GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers)
            {
                Color c = r.color;
                c.a = hintAlpha;
                r.color = c;
            }

            // 禁用碰撞体以免干扰鼠标事件
            // var collider = hint.GetComponent<BoxCollider2D>();
            // if (collider) collider.enabled = false;

            currentHint = hint;

            // 简单的淡入淡出动画可以使用协程或 DoTween，这里简化处理
        }

        private void HideHint()
        {
            if (currentHint != null)
            {
                GameEntry.PoolManager.DeSpawnSync(currentHint);
                currentHint = null;
            }
        }

        public void ShowPlacementPreview(BlockShape shape, Vector2Int gridPos)
        {
            if (GridManager.Instance.CanPlace(shape, gridPos))
            {
                if (placementPreview == null)
                {
                    placementPreview = GameEntry.PoolManager.SpawnSync<SceneBlockItem>(31001);
                    var col = placementPreview.GetComponent<BoxCollider2D>();
                    if (col) col.enabled = false;
                }

                placementPreview.gameObject.SetActive(true);
                placementPreview.transform.SetParent(gridOrigin, false);
                placementPreview.transform.localPosition =
                    gridStartOffset + new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
                placementPreview.Init(shape, -1, false);

                var renderers = placementPreview.GetComponentsInChildren<SpriteRenderer>();
                foreach (var r in renderers)
                {
                    Color c = r.color;
                    c.a = 0.5f; // 50% 透明度
                    r.color = c;
                }

                // 行列消除预览
                GridManager.Instance.GetPredictedClearLines(shape, gridPos, out var rows, out var cols);
                HighlightClearLines(rows, cols, shape.blockColor);
            }
            else
            {
                ClearPreview();
            }
        }

        public void ClearPreview()
        {
            HidePreviewGhost();
            ClearHighlight();
        }

        public void HidePreviewGhost()
        {
            if (placementPreview != null)
            {
                placementPreview.gameObject.SetActive(false);
            }
        }

        public void ClearHighlight()
        {
            HighlightClearLines(new List<int>(), new List<int>(), Color.white);
        }

        private void HighlightClearLines(List<int> rows, List<int> cols, Color highlightColor)
        {
            for (int x = 0; x < GridManager.Instance.cols; x++)
            {
                for (int y = 0; y < GridManager.Instance.rows; y++)
                {
                    bool isHighlighted = rows.Contains(y) || cols.Contains(x);
                    sceneGrid[x, y].SetPreviewState(isHighlighted, highlightColor);
                }
            }
        }

        private void OnLinesCleared(params object[] args)
        {
            if (args.Length > 0 && args[0] is List<ClearedCellInfo> cleared)
            {
                foreach (var cell in cleared)
                {
                    var animObj = new GameObject("ClearAnim");
                    animObj.transform.SetParent(gridOrigin, false);
                    animObj.transform.localPosition =
                        gridStartOffset + new Vector3(cell.pos.x * cellSize, cell.pos.y * cellSize, 0);
                    var sr = animObj.AddComponent<SpriteRenderer>();
                    sr.sprite = sceneGrid[cell.pos.x, cell.pos.y].spriteRenderer.sprite;
                    sr.color = cell.color;

                    animObj.transform.DOScale(0.1f, 0.5f).SetEase(Ease.OutQuad);
                    sr.DOFade(0f, 0.5f).SetEase(Ease.OutQuad).OnComplete(() => Destroy(animObj));
                }
            }
        }

        private void OnGameOverFillAnimation(params object[] args)
        {
            StartCoroutine(FillEmptyCellsCoroutine());
        }

        private List<BlockUnit> dropCell = new List<BlockUnit>();

        private IEnumerator FillEmptyCellsCoroutine()
        {
            var emptyCells = new List<Vector2Int>();
            for (int y = GridManager.Instance.rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < GridManager.Instance.cols; x++)
                {
                    if (GridManager.Instance.grid[x, y] == 0)
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            dropCell.Clear();
            foreach (var pos in emptyCells)
            {
                var cell = sceneGrid[pos.x, pos.y];
                Color randomColor = availableShapes[UnityEngine.Random.Range(0, availableShapes.Count)].blockColor;

                BlockUnit drop = GameEntry.PoolManager.SpawnSync<BlockUnit>(31002);
                drop.transform.SetParent(gridOrigin, false);
                Vector3 targetPos = gridStartOffset + new Vector3(pos.x * cellSize, pos.y * cellSize, 0);
                drop.transform.localPosition = targetPos + new Vector3(0, 10f, 0); // 从上方掉落
                drop.spriteRenderer.color = randomColor;
                drop.transform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    GameEntry.Sound.PlaySound(Constant.SoundId.Place);
                });
                dropCell.Add(drop);
                yield return new WaitForSeconds(0.1f); // 间隔 0.1 秒
            }

            yield return new WaitForSeconds(0.5f);
            EventManager.Instance.NotifyEvent(Constant.Event.GameOver); // 延迟 0.5 秒后打开结算界面
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < dropCell.Count; i++)
            {
                GameEntry.PoolManager.DeSpawnSync(dropCell[i]);
            }
        }

        private void OnGameStart(params object[] args)
        {
            lastInteractionTime = Time.time;
            HideHint();
            GenerateGrid();
            UpdateSpawnArea(true);

            ClearAllPopups();
        }

        private void GenerateGrid()
        {
            if (sceneGrid != null)
            {
                foreach (var cell in sceneGrid)
                {
                    if (cell != null) GameEntry.PoolManager.DeSpawnSync(cell);
                }
            }

            int rows = GridManager.Instance.rows;
            int cols = GridManager.Instance.cols;
            sceneGrid = new SceneGridCell[cols, rows];

            // 优化：居中生成网格
            float gridWidth = cols * cellSize;
            float gridHeight = rows * cellSize;
            // 计算左下角偏移量，使得网格中心位于 gridOrigin (0,0)
            gridStartOffset = new Vector3(-gridWidth / 2f + cellSize / 2f, -gridHeight / 2f + cellSize / 2f, 0);

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var cell = GameEntry.PoolManager.SpawnSync<SceneGridCell>(31000);
                    cell.transform.SetParent(gridOrigin, false);
                    cell.transform.localPosition = gridStartOffset + new Vector3(x * cellSize, y * cellSize, 0);
                    cell.Init(x, y);
                    sceneGrid[x, y] = cell;
                }
            }
        }

        private void OnGridUpdated(params object[] args)
        {
            if (sceneGrid == null) return;
            int[,] data = GridManager.Instance.grid;
            Color[,] colors = GridManager.Instance.gridColors;
            int rows = data.GetLength(1);
            int cols = data.GetLength(0);

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    bool filled = data[x, y] != 0;
                    sceneGrid[x, y].SetState(filled, filled ? colors[x, y] : Color.gray);
                }
            }
        }

        private void OnSpawnUpdated(params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                bool spawn = (bool)args[0];
                UpdateSpawnArea(spawn);
            }
        }

        private void UpdateSpawnArea(bool spawn)
        {
            foreach (var block in sceneSpawnBlocks)
            {
                if (block != null) GameEntry.PoolManager.DeSpawnSync(block);
            }

            sceneSpawnBlocks.Clear();

            // 尝试从 RectTransform 获取宽度，实现动态适配
            float actualWidth = spawnAreaWidth;
            if (spawnOrigin is RectTransform rt)
            {
                actualWidth = rt.rect.width;
            }

            List<BlockShape> shapes = BlockSpawner.Instance.currentShapes;
            int maxSlots = BlockSpawner.Instance.spawnSlots;
            float slotWidth = actualWidth / maxSlots;

            bool hasPlayedSpawnSound = false;
            
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] == null) continue;

                var block = GameEntry.PoolManager.SpawnSync<SceneBlockItem>(31001);
                block.transform.SetParent(spawnOrigin, false);

                // 优化：动态计算生成位置，居中分布
                float xPos = -actualWidth / 2f + slotWidth * i + slotWidth / 2f;
                block.transform.localPosition = new Vector3(xPos, 0, 0);

                block.Init(shapes[i], i,spawn);
                sceneSpawnBlocks.Add(block);
                
                if (spawn && !hasPlayedSpawnSound)
                {
                    GameEntry.Sound.PlaySound(Constant.SoundId.BlockSpawn);
                    hasPlayedSpawnSound = true;
                }
            }
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 localPos = gridOrigin.InverseTransformPoint(worldPos);
            // 考虑 gridStartOffset
            Vector3 relativePos = localPos - gridStartOffset;

            int x = Mathf.RoundToInt(relativePos.x / cellSize);
            int y = Mathf.RoundToInt(relativePos.y / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            Vector3 localPos = gridStartOffset + new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
            return gridOrigin.TransformPoint(localPos);
        }
    }
}