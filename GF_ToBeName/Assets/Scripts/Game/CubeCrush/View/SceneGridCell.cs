using UnityEngine;
using NewSideGame;

namespace CubeCrush.View
{
    public class SceneGridCell : ActivatablePoolPrefabBase
    {
        [Header("Components")]
        public SpriteRenderer spriteRenderer; // Assign in Prefab
        
        public int x, y;
        public bool isFilled;
        private Color currentColor;

        public void Init(int x, int y)
        {
            this.x = x;
            this.y = y;
            currentColor = Color.gray;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isFilled;
            }
            isFilled = false;
        }

        public void SetState(bool filled, Color color)
        {
            isFilled = filled;
            currentColor = filled ? color : Color.gray;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isFilled;
                spriteRenderer.color = currentColor;
            }
        }

        public void SetPreviewState(bool isPreview, Color previewColor)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isPreview || isFilled;
                spriteRenderer.color = isPreview ? previewColor : currentColor;
            }
        }
    }
}
