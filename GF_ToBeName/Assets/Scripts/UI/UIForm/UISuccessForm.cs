using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CubeCrush.Manager;
using NewSideGame;
using DG.Tweening;

namespace NewSideGame
{
    public class UISuccessForm : UGuiForm
    {
        [Header("UI Components")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI bestScoreText;
        public Button restartButton;
        public Button backButton;

        private int displayScore = 0;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => {
                    restartButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameLoopManager.Instance.Restart();
                    Close();
                });
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => {
                    backButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    SceneHelper.LoadHomeScene(() => {});
                    Close();
                });
            }

            UpdateScore(GameLoopManager.Instance.score);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }
        
        private void UpdateScore(int finalScore)
        {
            int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
            
            if (bestScoreText != null)
            {
                bestScoreText.text = $"Best: {bestScore}";
            }

            if (scoreText != null)
            {
                displayScore = 0;
                scoreText.text = $"Score: {displayScore}";
                
                // 以每秒500分的速度滚动
                float duration = finalScore > 0 ? Mathf.Max(0.5f, finalScore / 500f) : 0.5f;
                
                DOTween.To(() => displayScore, x => {
                    displayScore = x;
                    scoreText.text = $"Score: {displayScore}";
                }, finalScore, duration).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => {
                    // 破纪录闪烁动画
                    if (finalScore >= bestScore && finalScore > 0 && bestScoreText != null)
                    {
                        bestScoreText.text = $"Best: {finalScore}";
                        bestScoreText.transform.DOScale(1.2f, 0.3f).SetLoops(4, LoopType.Yoyo).SetUpdate(true);
                        bestScoreText.DOColor(Color.yellow, 0.3f).SetLoops(4, LoopType.Yoyo).SetUpdate(true);
                    }
                });
            }
        }
    }
}