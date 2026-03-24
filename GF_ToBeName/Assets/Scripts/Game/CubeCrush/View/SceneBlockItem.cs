using UnityEngine;
using CubeCrush.Data;
using CubeCrush.Manager;
using NewSideGame;
using DG.Tweening;
using System.Collections.Generic;

namespace NewSideGame
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SceneBlockItem : ActivatablePoolPrefabBase
    {
        [Header("Components")] public BoxCollider2D boxCollider; // Assign in Prefab

        private BlockShape shape;
        private int spawnIndex;
        private CubeCrushGoalItemType goalItemType = CubeCrushGoalItemType.None;
        private readonly List<GoalItemFx> activeGoalItemFx = new List<GoalItemFx>();
        private Vector3 originalPos;
        private Vector3 dragOffset;
        private bool isDragging;
        private Vector2Int lastGridPos = new Vector2Int(-999, -999);
        private bool hasScaledToFullSize;
        private bool isFirstSpawn = true;

        public void Init(BlockShape shape, int index,bool spawn, CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None)
        {
            ClearCells(); // 确保复用时清理旧的子物体
            this.shape = shape;
            this.spawnIndex = index;
            this.goalItemType = itemType;
            hasScaledToFullSize = false;
            isFirstSpawn = spawn;
            
            if (index != -1)
            {
                if (isFirstSpawn)
                {
                    transform.localScale = Vector3.zero;
                }
                else
                {
                    transform.localScale = Vector3.one * GameMain.Instance.spawnSize;
                }
            }
            else
            {
                transform.localScale = Vector3.one;
            }

            // Construct visual
            foreach (var cell in shape.cells)
            {
                var unit = GameEntry.PoolManager.SpawnSync<BlockUnit>(31002);
                unit.transform.SetParent(transform);
                unit.transform.localPosition = new Vector3(cell.x * GameMain.Instance.cellSize,
                    cell.y * GameMain.Instance.cellSize, 0);
                unit.transform.localScale = Vector3.one;

                unit.spriteRenderer.color = shape.blockColor;

                if (goalItemType != CubeCrushGoalItemType.None && GameMain.Instance.goalItemFxAssetId > 0)
                {
                    GoalItemFx itemFx = GameEntry.PoolManager.SpawnSync<GoalItemFx>(GameMain.Instance.goalItemFxAssetId);
                    if (itemFx != null)
                    {
                        itemFx.transform.SetParent(unit.transform, false);
                        itemFx.transform.localPosition = Vector3.zero;
                        itemFx.transform.localScale = Vector3.one;
                        itemFx.Init(GameMain.Instance.GetGoalItemSprite(goalItemType), Color.white);
                        activeGoalItemFx.Add(itemFx);
                    }
                }
            }

            // Adjust collider to fit shape
            UpdateCollider(GameMain.Instance.cellSize);

            if (index != -1 && isFirstSpawn)
            {
                transform.DOScale(GameMain.Instance.spawnSize, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
                isFirstSpawn = false;
            }
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);
            ClearCells();
        }

        private void ClearCells()
        {
            for (int i = 0; i < activeGoalItemFx.Count; i++)
            {
                if (activeGoalItemFx[i] != null)
                {
                    GameEntry.PoolManager.DeSpawnSync(activeGoalItemFx[i]);
                }
            }
            activeGoalItemFx.Clear();

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
            if (GameMain.Instance.IsClearingAnimationPlaying) return;
            if (GameMain.Instance.IsGameOverFillAnimating) return;
            if (GameLoopManager.Instance.isGameOver || GameLoopManager.Instance.IsStageClearPending) return; // 游戏结束/通关暂停后禁用交互

            // 播放方块拾取音效
            GameEntry.Sound.PlaySound(Constant.SoundId.BlockPickup);

            // 通知用户操作，重置空闲计时器
            GameMain.Instance?.OnUserInteraction();

            originalPos = transform.position;
            Vector3 mouseWorldPos = GetMouseWorldPos();
            dragOffset = transform.position - mouseWorldPos;
            isDragging = true;
            lastGridPos = new Vector2Int(-999, -999);

            // 选中时放大效果
            Sequence seq = DOTween.Sequence();
            if (!hasScaledToFullSize)
            {
                seq.Append(transform.DOScale(1f, 0.15f).SetUpdate(true));
                hasScaledToFullSize = true;
            }

            seq.Append(transform.DOScale(1.1f, 0.1f).SetUpdate(true));
        }

        private void OnMouseDrag()
        {
            if (spawnIndex == -1) return;
            if (GameMain.Instance.IsClearingAnimationPlaying) return;
            if (GameMain.Instance.IsGameOverFillAnimating) return;
            if (GameLoopManager.Instance.isGameOver || GameLoopManager.Instance.IsStageClearPending) return;

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
            if (GameMain.Instance.IsClearingAnimationPlaying) return;
            if (GameMain.Instance.IsGameOverFillAnimating) return;
            if (GameLoopManager.Instance.isGameOver || GameLoopManager.Instance.IsStageClearPending) return;

            isDragging = false;

            Vector2Int gridPos = GameMain.Instance.WorldToGrid(transform.position);

            if (GridManager.Instance.CanPlace(shape, gridPos))
            {
                // Valid placement: 不做“底部方块组飞行移动”，直接落逻辑
                GameMain.Instance.HidePreviewGhost();
                GameMain.Instance.ClearHighlight();
                GameEntry.Sound.PlaySound(Constant.SoundId.Bubble1);
                GameLoopManager.Instance.OnBlockPlaced(spawnIndex, shape, gridPos);
            }
            else
            {
                // Invalid placement: clear preview completely
                GameMain.Instance.ClearPreview();

                // Return to spawn
                transform.DOMove(originalPos, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
                // 回到“生成区大小”：只有放置到桌面成功后才会触发槽位刷新/重新 Init。
                float targetScale = GameMain.Instance.spawnSize;
                transform.DOScale(targetScale, 0.2f).SetUpdate(true);

                // 重置状态：下一次拿起时仍走完整拾取缩放流程
                hasScaledToFullSize = false;
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