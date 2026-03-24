using UnityEngine;
using CubeCrush.Manager;
using CubeCrush.Data;

namespace NewSideGame
{
    /// <summary>
    /// Editor-only helper: press Space to auto place a spawn block.
    /// Placement scans grid from left-to-right, bottom-to-top.
    /// </summary>
    public class EditorAutoPlaceBlockGroup : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool enable = true;

        private void Update()
        {
            if (!enable) return;
            if (!Application.isPlaying) return;
            if (!Input.GetKeyDown(KeyCode.Space)) return;

            // Respect gameplay pause states.
            if (GameLoopManager.Instance.isGameOver) return;
            if (GameLoopManager.Instance.IsStageClearPending) return;

            int spawnIndex = GetNextUsableSpawnIndex();
            if (spawnIndex < 0) return;

            BlockShape shape = BlockSpawner.Instance.currentShapes[spawnIndex];
            if (shape == null) return;

            Vector2Int pos;
            if (!TryFindFirstPlacePos(shape, out pos)) return;

            GameLoopManager.Instance.OnBlockPlaced(spawnIndex, shape, pos);
        }

        private int GetNextUsableSpawnIndex()
        {
            var shapes = BlockSpawner.Instance.currentShapes;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] != null) return i;
            }
            return -1;
        }

        private bool TryFindFirstPlacePos(BlockShape shape, out Vector2Int pos)
        {
            int cols = GridManager.Instance.cols;
            int rows = GridManager.Instance.rows;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2Int p = new Vector2Int(x, y);
                    if (GridManager.Instance.CanPlace(shape, p))
                    {
                        pos = p;
                        return true;
                    }
                }
            }

            pos = default;
            return false;
        }
#endif
    }
}

