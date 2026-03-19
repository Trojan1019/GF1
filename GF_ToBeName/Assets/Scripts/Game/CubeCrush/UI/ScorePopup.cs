using UnityEngine;
using DG.Tweening;
using TMPro;
using NewSideGame;

namespace NewSideGame
{
    public class ScorePopup : ActivatablePoolPrefabBase
    {
        [Header("组件引用")] public TextMeshProUGUI scoreText;
        public CanvasGroup canvasGroup;
        public RectTransform rectTransform;

        [Header("动画配置")] public float moveDistance = 10f;
        public float animationDuration = 2.5f;
        public float scaleUpAmount = 1.3f;
        public float scaleUpDuration = 0.3f;

        [Header("颜色配置")] public Color normalColor = Color.yellow;
        public Color bigScoreColor = Color.magenta;
        public int bigScoreThreshold = 500;

        private Vector3 _startPosition;
        private Tween _currentTween;

        public void Init(int score, Vector3 localPos)
        {
            if (scoreText == null) return;

            scoreText.text = $"+{score}";
            scoreText.color = score >= bigScoreThreshold ? bigScoreColor : normalColor;

            rectTransform.localPosition = localPos;
            _startPosition = localPos;

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (_currentTween != null)
            {
                _currentTween.Kill();
            }

            Sequence sequence = DOTween.Sequence();

            sequence.Append(rectTransform.DOScale(scaleUpAmount, scaleUpDuration).SetEase(Ease.OutBack));
            sequence.Append(rectTransform.DOScale(1f, scaleUpDuration * 0.5f).SetEase(Ease.InQuad));

            sequence.Join(rectTransform.DOMoveY(_startPosition.y + moveDistance, animationDuration).SetEase(Ease.OutQuad));

            sequence.Join(canvasGroup.DOFade(0f, animationDuration * 0.8f).SetDelay(animationDuration * 0.2f)
                .SetEase(Ease.InQuad));

            sequence.OnComplete(() => { ReturnToPool(); });

            _currentTween = sequence;
        }

        private void ReturnToPool()
        {
            if (_currentTween != null)
            {
                _currentTween.Kill();
                _currentTween = null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
            
            GameMain.Instance.RemovePopup(this);
            GameEntry.PoolManager.DeSpawnSync(this);
        }

        private void OnDestroy()
        {
            if (_currentTween != null)
            {
                _currentTween.Kill();
            }
        }
    }
}