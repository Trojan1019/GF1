using UnityEngine;
using CubeCrush.Data;
using CubeCrush.Manager;
using NewSideGame;
using DG.Tweening;

namespace NewSideGame
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SceneBlockItem : ActivatablePoolPrefabBase
    {
        [Header("Components")] public BoxCollider2D boxCollider; // Assign in Prefab

        private BlockShape shape;
        private int spawnIndex;
        private Vector3 originalPos;
        private Vector3 dragOffset;
        private bool isDragging;
        private Vector2Int lastGridPos = new Vector2Int(-999, -999);

        public void Init(BlockShape shape, int index)
        {
            ClearCells(); // 确保复用时清理旧的子物体
            this.shape = shape;
            this.spawnIndex = index;

            // Construct visual
            foreach (var cell in shape.cells)
            {
                var unit = GameEntry.PoolManager.SpawnSync<BlockUnit>(31002);
                unit.transform.SetParent(transform);
                unit.transform.localPosition = new Vector3(cell.x * GameMain.Instance.cellSize,
                    cell.y * GameMain.Instance.cellSize, 0);
                unit.transform.localScale = Vector3.one;

                unit.spriteRenderer.color = shape.blockColor;
            }

            // Adjust collider to fit shape
            UpdateCollider(GameMain.Instance.cellSize);
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);
            ClearCells();
        }

        private void ClearCells()
        {
            var units = GetComponentsInChildren<BlockUnit>();
            foreach (var unit in units)
            {
                GameEntry.PoolManager.DeSpawnSync(unit);
            }
        }

        private void UpdateCollider(float cellSize)
        {
            if (boxCollider == null) return;
            if (shape.cells.Count == 0) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            // 修正碰撞体计算逻辑，确保包裹整个方块
            // 假设 BlockUnit 的 pivot 是中心，位置是 (cell.x * size, cell.y * size)
            // 那么它的边界是 [pos - size/2, pos + size/2]
            float halfSize = cellSize / 2f;

            foreach (var cell in shape.cells)
            {
                float centerX = cell.x * cellSize;
                float centerY = cell.y * cellSize;

                minX = Mathf.Min(minX, centerX - halfSize);
                minY = Mathf.Min(minY, centerY - halfSize);
                maxX = Mathf.Max(maxX, centerX + halfSize);
                maxY = Mathf.Max(maxY, centerY + halfSize);
            }

            boxCollider.offset = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
            boxCollider.size = new Vector2(maxX - minX, maxY - minY);
        }

        private void OnMouseDown()
        {
            if (spawnIndex == -1) return; // 提示虚影不响应点击
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.isGameOver) return; // 游戏结束后禁用交互

            // 通知用户操作，重置空闲计时器
            GameMain.Instance?.OnUserInteraction();

            originalPos = transform.position;
            Vector3 mouseWorldPos = GetMouseWorldPos();
            dragOffset = transform.position - mouseWorldPos;
            isDragging = true;
            lastGridPos = new Vector2Int(-999, -999);

            // 选中时放大效果
            transform.DOScale(1.1f, 0.1f).SetUpdate(true);
        }

        private void OnMouseDrag()
        {
            if (spawnIndex == -1) return;
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.isGameOver) return;

            if (isDragging)
            {
                Vector3 mouseWorldPos = GetMouseWorldPos();
                transform.position = mouseWorldPos + dragOffset;

                Vector2Int gridPos = GameMain.Instance.WorldToGrid(transform.position);
                if (gridPos != lastGridPos)
                {
                    GameMain.Instance.ShowPlacementPreview(shape, gridPos);
                    lastGridPos = gridPos;
                }
            }
        }

        private void OnMouseUp()
        {
            if (spawnIndex == -1) return;
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.isGameOver) return;

            isDragging = false;

            Vector2Int gridPos = GameMain.Instance.WorldToGrid(transform.position);

            if (GameLoopManager.Instance != null)
            {
                if (GridManager.Instance.CanPlace(shape, gridPos))
                {
                    // Valid placement: hide the ghost but keep the grid highlight until placed
                    GameMain.Instance.HidePreviewGhost();

                    Vector3 targetWorldPos = GameMain.Instance.GridToWorld(gridPos);
                    // 播放拖拽方块音效（鼠标按下瞬间）
                    if (GameLoopManager.Instance != null)
                    {
                        GameEntry.Sound.PlaySound(Constant.SoundId.Bubble1);
                    }

                    // 放置时缩放0.9倍后回弹动画
                    Sequence seq = DOTween.Sequence();
                    seq.Append(transform.DOMove(targetWorldPos, 0.05f).SetUpdate(true));
                    seq.Join(transform.DOScale(0.9f, 0.1f).SetUpdate(true));
                    seq.Append(transform.DOScale(1f, 0.1f).SetUpdate(true));
                    seq.OnComplete(() =>
                    {
                        GameMain.Instance.ClearHighlight();
                        GameLoopManager.Instance.OnBlockPlaced(spawnIndex, shape, gridPos);
                    });
                }
                else
                {
                    // Invalid placement: clear preview completely
                    GameMain.Instance.ClearPreview();

                    // Return to spawn
                    transform.DOMove(originalPos, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
                    transform.DOScale(1f, 0.2f).SetUpdate(true);
                }
            }
            else
            {
                GameMain.Instance.ClearPreview();
                transform.position = originalPos;
                transform.DOScale(1f, 0.1f).SetUpdate(true);
            }
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = -Camera.main.transform.position.z; // Distance to camera
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}