using UnityEngine;
using System.Collections.Generic;

namespace BlockBlast.Data
{
    [CreateAssetMenu(fileName = "NewBlockShape", menuName = "BlockBlast/BlockShape")]
    public class BlockShape : ScriptableObject
    {
        [Header("Shape Definition")]
        public List<Vector2Int> cells = new List<Vector2Int>();

        [Header("Visual")]
        public Color blockColor = Color.white;
        public Sprite blockSprite;

        // Configure shape from a list of points (for editor or dynamic creation)
        public void SetShape(List<Vector2Int> newCells)
        {
            cells = new List<Vector2Int>(newCells);
        }

        public int Width
        {
            get
            {
                if (cells.Count == 0) return 0;
                int minX = int.MaxValue, maxX = int.MinValue;
                foreach (var cell in cells)
                {
                    if (cell.x < minX) minX = cell.x;
                    if (cell.x > maxX) maxX = cell.x;
                }
                return maxX - minX + 1;
            }
        }

        public int Height
        {
            get
            {
                if (cells.Count == 0) return 0;
                int minY = int.MaxValue, maxY = int.MinValue;
                foreach (var cell in cells)
                {
                    if (cell.y < minY) minY = cell.y;
                    if (cell.y > maxY) maxY = cell.y;
                }
                return maxY - minY + 1;
            }
        }
    }
}
