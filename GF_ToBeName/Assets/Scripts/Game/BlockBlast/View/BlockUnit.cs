using UnityEngine;
using NewSideGame;

namespace BlockBlast.View
{
    public class BlockUnit : ActivatablePoolPrefabBase
    {
        [Header("Components")] public SpriteRenderer spriteRenderer; // Assign in Prefab

        public void Init()
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        }
    }
}