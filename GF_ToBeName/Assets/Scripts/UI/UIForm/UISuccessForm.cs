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
        [Header("UI Components")] public TextMeshProUGUI scoreText;
        public TextMeshProUGUI bestScoreText;
        public Button restartButton;
        public Button backButton;

        private int displayScore = 0;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);

            bool isStageClear = GameLoopManager.Instance != null && GameLoopManager.Instance.IsStageClearPending;
            // 获取并显示最高分
            int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
            bestScoreText.text =
                string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), bestScore); // 使用千位分隔符格式化

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    restartButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    if (isStageClear)
                    {
                        GameLoopManager.Instance.NextStage();
                    }
                    else
                    {
                        GameLoopManager.Instance.Restart();
                    }

                    Close();
                });
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() =>
                {
                    backButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    SceneHelper.LoadHomeScene(() => { });
                    Close();
                });
            }

            if (GameLoopManager.Instance != null)
            {
                // 通关 UI 展示本关累计分；游戏结束展示总分
                int totalScore = GameLoopManager.Instance.score;
                int animationScore = isStageClear ? totalScore - GameLoopManager.Instance.StageStartScore : totalScore;
                UpdateScore(animationScore, totalScore, isStageClear ? GameLoopManager.Instance.StageTargetDelta : 0,
                    isStageClear);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RemoveEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
            }
            base.OnClose(isShutdown, userData);
        }

        private void OnLanguageChanged(object[] args)
        {
            // 只刷新文本，不重置分数滚动动画；displayScore 会随着 DOTween 过程实时更新
            if (GameLoopManager.Instance == null) return;

            bool isStageClear = GameLoopManager.Instance.IsStageClearPending;
            int stageTarget = isStageClear ? GameLoopManager.Instance.StageTargetDelta : 0;

            // bestScoreText 始终显示“记录分”（totalScore）
            int recordScore = GameLoopManager.Instance.score;
            if (bestScoreText != null)
            {
                bestScoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), recordScore);
            }

            if (scoreText == null) return;

            string scoreLabel = GameEntry.Localization.GetString("5"); // 分数
            string targetLabel = GameEntry.Localization.GetString("23"); // 目标分数

            if (isStageClear)
            {
                scoreText.text = string.Format("{0}:{1:N0}\n{2}:{3:N0}",
                    scoreLabel,
                    displayScore,
                    targetLabel,
                    stageTarget);
            }
            else
            {
                scoreText.text = string.Format("{0}:{1:N0}", scoreLabel, displayScore);
            }
        }

        private void UpdateScore(int animationScore, int recordScore, int stageTarget, bool isStageClear)
        {
            // 获取并显示最高分
            int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
            bestScoreText.text =
                string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), bestScore); // 使用千位分隔符格式化


            if (scoreText != null)
            {
                displayScore = 0;
                bool isStageSurvival = GameMain.Instance != null && GameMain.Instance.IsStageSurvival;
                int maxStage = isStageSurvival ? GameLoopManager.Instance.MaxStageReached : 1;
                //string maxStageLabel = string.Format(GameEntry.Localization.GetString("32"), maxStage); // 第 {0} 关

                if (isStageClear)
                {
                    scoreText.text = string.Format("{0}:{1:N0}\n{2}:{3:N0}",
                        GameEntry.Localization.GetString("5"), // 分数
                        displayScore,
                        GameEntry.Localization.GetString("23"), // 目标分数
                        stageTarget);
                }
                else
                {
                    scoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("5"), displayScore);
                }

                bestScoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), recordScore);
                // 以每秒500分的速度滚动
                float duration = animationScore > 0 ? Mathf.Max(0.5f, animationScore / 500f) : 0.5f;

                DOTween.To(() => displayScore, x =>
                {
                    displayScore = x;
                    if (isStageClear)
                    {
                        scoreText.text = string.Format("{0}:{1:N0}\n{2}:{3:N0}",
                            GameEntry.Localization.GetString("5"), // 分数
                            displayScore,
                            GameEntry.Localization.GetString("23"), // 目标分数
                            stageTarget);
                    }
                    else
                    {
                        scoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("5"), displayScore);
                    }
                }, animationScore, duration).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
                {
                    // 破纪录闪烁动画
                    if (recordScore >= bestScore && recordScore > 0 && bestScoreText != null)
                    {
                        bestScoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"),
                            recordScore);
                        bestScoreText.transform.DOScale(1.2f, 0.3f).SetLoops(4, LoopType.Yoyo).SetUpdate(true);
                        bestScoreText.DOColor(Color.yellow, 0.3f).SetLoops(4, LoopType.Yoyo).SetUpdate(true);
                    }
                });
            }
        }
    }
}