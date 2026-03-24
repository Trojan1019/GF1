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
        [System.Serializable]
        public class GoalItemSpriteEntry
        {
            public CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None;
            public Sprite sprite;
        }

        public static GameMain Instance;

        private void Awake()
        {
            Instance = this;
            AwakeStageSurvivalModeFromSetting();
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
        private bool isClearingAnimationPlaying;
        public bool isGameOverFillAnimating;
        public bool IsClearingAnimationPlaying => isClearingAnimationPlaying;
        public bool IsGameOverFillAnimating => isGameOverFillAnimating;

        private SceneBlockItem placementPreview;

        [Header("Prefabs")]
        //public SceneGridCell gridCellPrefab;
        // public SceneBlockItem blockItemPrefab;
        //public BlockUnit blockUnitPrefab; // Single block unit (sprite)
        private SceneGridCell[,] sceneGrid;

        private List<SceneBlockItem> sceneSpawnBlocks = new List<SceneBlockItem>();
        [Header("Goal Item Visuals")]
        [Tooltip("Pooled GoalItemFx asset id, configured in prefab naming system.")]
        public int goalItemFxAssetId = 31004;
        [SerializeField] private List<GoalItemSpriteEntry> goalItemSprites = new List<GoalItemSpriteEntry>();

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
            // 通关 UI 暂停状态下，不再生成提示/动画交互
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsStageClearPending) return;

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

        // 通关 UI 暂停：隐藏提示与预览，避免玩家看到“还能操作”的视觉反馈
        public void OnStageClearPause()
        {
            HideHint();
            ClearPreview();
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
            if (args == null || args.Length < 1) return;

            if (!(args[0] is List<ClearedCellInfo> clearedCells) || clearedCells == null) return;

            // 新需求：逐行/逐列有方向地消失：
            // 行：x 从左到右；列：y 从上到下。
            List<int> clearedRows = args.Length > 1 && args[1] is List<int> listRows ? listRows : null;
            List<int> clearedCols = args.Length > 2 && args[2] is List<int> listCols ? listCols : null;

            // 动画期间禁用交互输入
            isClearingAnimationPlaying = true;

            // 兼容旧触发：如果没传行/列，则退回“并行淡出”
            if (clearedRows == null && clearedCols == null)
            {
                foreach (var cell in clearedCells)
                {
                    if (sceneGrid != null && sceneGrid[cell.pos.x, cell.pos.y] != null)
                        sceneGrid[cell.pos.x, cell.pos.y].PlayClearFx(0f, 0.5f);
                }
                StartCoroutine(EndClearAnimationAfter(0.5f));
                return;
            }

            float lineTotalDuration = 0.5f;
            float cellFxDuration = 0.16f;
            float maxEndTime = 0f;

            // 逐行消失：同一行内部按 x 升序延迟
            if (clearedRows != null)
            {
                foreach (int y in clearedRows)
                {
                    var cellsInRow = clearedCells.FindAll(c => c.pos.y == y);
                    cellsInRow.Sort((a, b) => a.pos.x.CompareTo(b.pos.x));

                    int count = cellsInRow.Count;
                    if (count <= 0) continue;

                    float stepDur = count <= 1 ? 0f : Mathf.Max(0.01f, (lineTotalDuration - cellFxDuration) / (count - 1));
                    for (int i = 0; i < count; i++)
                    {
                        float delay = i * stepDur;
                        if (sceneGrid != null && sceneGrid[cellsInRow[i].pos.x, cellsInRow[i].pos.y] != null)
                            sceneGrid[cellsInRow[i].pos.x, cellsInRow[i].pos.y].PlayClearFx(delay, cellFxDuration);
                        maxEndTime = Mathf.Max(maxEndTime, delay + cellFxDuration);
                    }
                }
            }

            // 逐列消失：同一列内部按 y 降序延迟（从上到下）
            if (clearedCols != null)
            {
                foreach (int x in clearedCols)
                {
                    var cellsInCol = clearedCells.FindAll(c => c.pos.x == x);
                    cellsInCol.Sort((a, b) => b.pos.y.CompareTo(a.pos.y));

                    int count = cellsInCol.Count;
                    if (count <= 0) continue;

                    float stepDur = count <= 1 ? 0f : Mathf.Max(0.01f, (lineTotalDuration - cellFxDuration) / (count - 1));
                    for (int i = 0; i < count; i++)
                    {
                        float delay = i * stepDur;
                        if (sceneGrid != null && sceneGrid[cellsInCol[i].pos.x, cellsInCol[i].pos.y] != null)
                            sceneGrid[cellsInCol[i].pos.x, cellsInCol[i].pos.y].PlayClearFx(delay, cellFxDuration);
                        maxEndTime = Mathf.Max(maxEndTime, delay + cellFxDuration);
                    }
                }
            }
            StartCoroutine(EndClearAnimationAfter(maxEndTime));
        }

        private IEnumerator EndClearAnimationAfter(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            isClearingAnimationPlaying = false;
        }

        private void OnGameOverFillAnimation(params object[] args)
        {
            isGameOverFillAnimating = true;
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
                drop.transform.localScale = Vector3.one;
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
            isGameOverFillAnimating = false;
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
            int[,] goalItems = GridManager.Instance.gridGoalItems;
            int rows = data.GetLength(1);
            int cols = data.GetLength(0);

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    bool filled = data[x, y] != 0;
                    CubeCrushGoalItemType itemType = (CubeCrushGoalItemType)goalItems[x, y];
                    Sprite itemSprite = itemType != CubeCrushGoalItemType.None ? GetGoalItemSprite(itemType) : null;
                    // 丢失兜底：只要 grid 数据里记录了道具类型，这里每次刷新都会重新绑定 sprite
                    sceneGrid[x, y].SetState(filled, filled ? colors[x, y] : Color.gray, itemSprite);
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

                CubeCrushGoalItemType itemType = BlockSpawner.Instance.GetItemAt(i);
                block.Init(shapes[i], i, spawn, itemType);
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

        public Sprite GetGoalItemSprite(CubeCrushGoalItemType itemType)
        {
            if (itemType == CubeCrushGoalItemType.None || goalItemSprites == null) return null;
            for (int i = 0; i < goalItemSprites.Count; i++)
            {
                var e = goalItemSprites[i];
                if (e == null) continue;
                if (e.itemType == itemType) return e.sprite;
            }
            return null;
        }
    }
}