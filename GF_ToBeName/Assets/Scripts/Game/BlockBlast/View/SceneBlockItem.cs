using UnityEngine;
using BlockBlast.Data;
using BlockBlast.Manager;
using NewSideGame;

namespace BlockBlast.View
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

        public void Init(BlockShape shape, int index)
        {
            this.shape = shape;
            this.spawnIndex = index;

            // Construct visual
            foreach (var cell in shape.cells)
            {
                var unit = GameEntry.PoolManager.SpawnSync<BlockUnit>(31002);
                unit.transform.SetParent(transform);
                unit.transform.localPosition = new Vector3(cell.x * GameMain.Instance.cellSize, cell.y * GameMain.Instance.cellSize, 0);
                unit.spriteRenderer.color = shape.blockColor;
            }

            // Adjust collider to fit shape
            UpdateCollider(GameMain.Instance.cellSize);
        }

        private void UpdateCollider(float cellSize)
        {
            if (boxCollider == null) return;
            if (shape.cells.Count == 0) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var cell in shape.cells)
            {
                minX = Mathf.Min(minX, cell.x * cellSize);
                minY = Mathf.Min(minY, cell.y * cellSize);
                maxX = Mathf.Max(maxX, (cell.x + 1) * cellSize);
                maxY = Mathf.Max(maxY, (cell.y + 1) * cellSize);
            }

            boxCollider.offset = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
            boxCollider.size = new Vector2(maxX - minX, maxY - minY);
        }

        private void OnMouseDown()
        {
            originalPos = transform.position;
            Vector3 mouseWorldPos = GetMouseWorldPos();
            dragOffset = transform.position - mouseWorldPos;
            isDragging = true;

            // Optional: Scale up or visual feedback
            transform.localScale = Vector3.one * 1.2f;
        }

        private void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector3 mouseWorldPos = GetMouseWorldPos();
                transform.position = mouseWorldPos + dragOffset;
            }
        }

        private void OnMouseUp()
        {
            isDragging = false;
            transform.localScale = Vector3.one;

            // Try to place
            // Calculate grid position of the pivot (0,0) of the shape
            // Note: SceneBlockItem local (0,0) corresponds to shape cell (0,0)

            Vector2Int gridPos = GameMain.Instance.WorldToGrid(transform.position);

            if (GameLoopManager.Instance != null)
            {
                // Check if valid first (optional, GameLoopManager checks too but good for feedback)
                if (GridManager.Instance.CanPlace(shape, gridPos))
                {
                    GameLoopManager.Instance.OnBlockPlaced(spawnIndex, shape, gridPos);
                    // Destroy self handled by SpawnUpdate event usually, 
                    // but for immediate feedback we might hide it or wait for event.
                    // GameLoopManager will fire SpawnUpdate which rebuilds the spawn area.
                }
                else
                {
                    // Return to spawn
                    transform.position = originalPos;
                }
            }
            else
            {
                transform.position = originalPos;
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