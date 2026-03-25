using UnityEngine;
using DG.Tweening;
using TMPro;
using NewSideGame;

namespace NewSideGame
{
    public class ScorePopup : ActivatablePoolPrefabBase
    {
        private enum PopupAnimStyle
        {
            Score = 0,
            Combo = 1,
        }

        [Header("组件引用")] public TextMeshProUGUI scoreText;
        public CanvasGroup canvasGroup;
        public RectTransform rectTransform;

        [Header("动画配置")]
        [Tooltip("加分弹字上飘距离（UI 像素坐标）")]
        public float scoreMoveDistance = 90f;

        [Tooltip("连击弹字上飘距离（UI 像素坐标）")]
        public float comboMoveDistance = 70f;

        [Tooltip("兼容旧配置：未填写新字段时使用的上飘距离")]
        public float moveDistance = 10f;
        public float animationDuration = 2.5f;
        public float scaleUpAmount = 1.3f;
        public float scaleUpDuration = 0.3f;

        [Header("颜色配置")] public Color normalColor = Color.yellow;
        public Color bigScoreColor = Color.magenta;
        public int bigScoreThreshold = 500;

        private Vector3 _startPosition;
        private Vector2 _startAnchoredPosition;
        private Tween _currentTween;
        private float _lifeDuration;
        private PopupAnimStyle _animStyle;

        public void Init(int score, Vector3 localPos)
        {
            if (scoreText == null) return;

            scoreText.text = $"+{score}";
            scoreText.color = score >= bigScoreThreshold ? bigScoreColor : normalColor;
            _lifeDuration = animationDuration;
            _animStyle = PopupAnimStyle.Score;

            rectTransform.localPosition = localPos;
            rectTransform.anchoredPosition = new Vector2(localPos.x, localPos.y);
            _startPosition = localPos;
            _startAnchoredPosition = rectTransform.anchoredPosition;

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            PlayAnimation();
        }

        public void InitCustom(string text, Vector3 localPos, float duration)
        {
            if (scoreText == null) return;
            scoreText.text = text;
            scoreText.color = normalColor;
            _lifeDuration = Mathf.Clamp(duration, 0.8f, 1.2f);
            _animStyle = PopupAnimStyle.Combo;

            rectTransform.localPosition = localPos;
            rectTransform.anchoredPosition = new Vector2(localPos.x, localPos.y);
            _startPosition = localPos;
            _startAnchoredPosition = rectTransform.anchoredPosition;
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

            // UI 使用 anchoredPosition 做“向上飘”，避免世界坐标 tween 在不同父节点下表现不一致。
            rectTransform.anchoredPosition = _startAnchoredPosition;

            if (_animStyle == PopupAnimStyle.Score)
            {
                // 保持原本“加分文本”的动画手感：先放大再回弹，同时上飘+淡出
                sequence.Append(rectTransform.DOScale(scaleUpAmount, scaleUpDuration).SetEase(Ease.OutBack));
                sequence.Append(rectTransform.DOScale(1f, scaleUpDuration * 0.5f).SetEase(Ease.InQuad));

                float dist = scoreMoveDistance > 0.001f ? scoreMoveDistance : moveDistance;
                // 上飘从一开始就执行，避免看起来“没动”
                sequence.Insert(0f,
                    rectTransform.DOAnchorPosY(_startAnchoredPosition.y + dist, _lifeDuration).SetEase(Ease.OutQuad));

                sequence.Insert(0f,
                    canvasGroup.DOFade(0f, _lifeDuration * 0.8f)
                        .SetDelay(_lifeDuration * 0.2f)
                        .SetEase(Ease.InQuad));
            }
            else
            {
                // 连击文本：更明显的弹动 + 上飘消散
                sequence.Append(rectTransform.DOPunchScale(new Vector3(0.18f, 0.18f, 0f), 0.22f, 8, 1f)
                    .SetEase(Ease.OutQuad));
                sequence.Append(rectTransform.DOScale(1f, 0.08f).SetEase(Ease.OutQuad));

                float dist = comboMoveDistance > 0.001f ? comboMoveDistance : moveDistance;
                sequence.Insert(0f,
                    rectTransform.DOAnchorPosY(_startAnchoredPosition.y + dist, _lifeDuration).SetEase(Ease.OutQuad));

                sequence.Insert(0f,
                    canvasGroup.DOFade(0f, _lifeDuration * 0.8f)
                        .SetDelay(_lifeDuration * 0.2f)
                        .SetEase(Ease.InQuad));
            }

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