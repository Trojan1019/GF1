using UnityEngine;
using NewSideGame;

namespace NewSideGame
{
    public class BlockUnit : ActivatablePoolPrefabBase
    {
        [Header("Components")] public SpriteRenderer spriteRenderer; // Assign in Prefab

        public void Init()
        {
            transform.localScale = Vector3.one;
            if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);
            transform.localScale = Vector3.one;
        }
    }
}