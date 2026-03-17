using UnityEngine;
using NewSideGame;

namespace BlockBlast.View
{
    public class SceneGridCell : ActivatablePoolPrefabBase
    {
        [Header("Components")]
        public SpriteRenderer spriteRenderer; // Assign in Prefab
        
        public int x, y;
        public bool isFilled;

        public void Init(int x, int y)
        {
            this.x = x;
            this.y = y;
            if (spriteRenderer != null) spriteRenderer.color = Color.gray;
            isFilled = false;
        }

        public void SetState(bool filled, Color color)
        {
            isFilled = filled;
            if (spriteRenderer != null) spriteRenderer.color = filled ? color : Color.gray;
        }
    }
}
