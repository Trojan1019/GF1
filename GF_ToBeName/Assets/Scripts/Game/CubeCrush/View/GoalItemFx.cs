using UnityEngine;

namespace NewSideGame
{
    public class GoalItemFx : ActivatablePoolPrefabBase
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Transform _cachedTransform;

        private void Awake()
        {
            _cachedTransform = transform;
        }

        public void Init(Sprite icon, Color tint)
        {
            if (spriteRenderer == null) return;
            spriteRenderer.sprite = icon;
            spriteRenderer.color = tint;
            spriteRenderer.enabled = icon != null;
            ResetTransform();
        }

        public override void OnSpawn(PoolManager ppm)
        {
            base.OnSpawn(ppm);
            ResetTransform();
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
                spriteRenderer.enabled = false;
                spriteRenderer.color = Color.white;
            }
            ResetTransform();
        }

        private void ResetTransform()
        {
            if (_cachedTransform == null) _cachedTransform = transform;
            _cachedTransform.localScale = Vector3.one;
            _cachedTransform.localPosition = Vector3.zero;
            _cachedTransform.localRotation = Quaternion.identity;
        }
    }
}
