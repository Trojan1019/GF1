using UnityEngine;
using NewSideGame;
using System.Collections;

namespace NewSideGame
{
    public class SceneGridCell : ActivatablePoolPrefabBase
    {
        [Header("Components")]
        public SpriteRenderer spriteRenderer; // Assign in Prefab
        [SerializeField] private SpriteRenderer clearFxRenderer; // Assign in Prefab (recommended)
        [SerializeField] private SpriteRenderer goalItemIconRenderer; // Assign in Prefab (optional)
        
        public int x, y;
        public bool isFilled;
        private Color currentColor;

        private Coroutine clearFxCoroutine;
        private bool _warnedMissingClearFxRenderer;

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
            if (clearFxRenderer != null)
            {
                clearFxRenderer.enabled = false;
                clearFxRenderer.transform.localScale = Vector3.one;
            }
            if (goalItemIconRenderer != null)
            {
                goalItemIconRenderer.enabled = false;
                goalItemIconRenderer.sprite = null;
                goalItemIconRenderer.color = Color.white;
            }
        }

        public void SetState(bool filled, Color color, Sprite goalItemSprite)
        {
            isFilled = filled;
            currentColor = filled ? color : Color.gray;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isFilled;
                spriteRenderer.color = currentColor;
            }

            if (goalItemIconRenderer != null)
            {
                goalItemIconRenderer.sprite = goalItemSprite;
                goalItemIconRenderer.enabled = isFilled && goalItemSprite != null;
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

        public void PlayClearFx(float delay, float totalDuration)
        {
            if (spriteRenderer == null) return;
            if (clearFxRenderer == null)
            {
                if (!_warnedMissingClearFxRenderer)
                {
                    _warnedMissingClearFxRenderer = true;
                    Debug.LogWarning($"[SceneGridCell] Missing clearFxRenderer on cell ({x},{y}). Assign it in prefab to enable clear FX.");
                }
                return;
            }

            if (clearFxCoroutine != null)
            {
                StopCoroutine(clearFxCoroutine);
                clearFxCoroutine = null;
            }

            clearFxRenderer.sprite = spriteRenderer.sprite;
            clearFxRenderer.color = currentColor;
            clearFxRenderer.enabled = true;
            clearFxRenderer.transform.localScale = Vector3.one;

            clearFxCoroutine = StartCoroutine(ClearFxCoroutine(delay, totalDuration));
        }

        private IEnumerator ClearFxCoroutine(float delay, float totalDuration)
        {
            // 1) 延迟
            float t = 0f;
            while (t < delay)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // 2) 先放大 0.1f（1.0 -> 1.1）
            float popDuration = Mathf.Max(0.06f, totalDuration * 0.2f);
            float shrinkDuration = Mathf.Max(0.06f, totalDuration - popDuration);

            t = 0f;
            while (t < popDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / popDuration);
                float scale = Mathf.Lerp(1f, 1.2f, p);
                clearFxRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            // 3) 再缩小到 0 并透明度渐变到 0
            Color startColor = clearFxRenderer.color;
            t = 0f;
            while (t < shrinkDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / shrinkDuration);
                float scale = Mathf.Lerp(1.1f, 0f, p);
                clearFxRenderer.transform.localScale = new Vector3(scale, scale, 1f);

                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, p);
                clearFxRenderer.color = c;
                yield return null;
            }

            clearFxRenderer.enabled = false;
            clearFxRenderer.transform.localScale = Vector3.one;
            clearFxCoroutine = null;
        }
    }
}
