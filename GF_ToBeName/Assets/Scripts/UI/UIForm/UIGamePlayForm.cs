using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CubeCrush.Manager;
using NewSideGame;
using DG.Tweening;

namespace NewSideGame
{
    public class UIGamePlayForm : UGuiForm
    {
        [Header("UI Components")] public TextMeshProUGUI scoreText;
        public TextMeshProUGUI bestScoreText;
        public Button restartButton;
        public Button settingButton;

        private int displayScore = 0;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            EventManager.Instance.AddEventListener(Constant.Event.GameOver, OnGameOver);
            EventManager.Instance.AddEventListener(Constant.Event.RefreshScore, OnRefreshScore);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    // 按钮点击弹性反馈
                    restartButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameLoopManager.Instance.Restart();
                });
            }

            if (settingButton != null)
            {
                settingButton.onClick.RemoveAllListeners();
                settingButton.onClick.AddListener(() =>
                {
                    // 按钮点击弹性反馈
                    settingButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameEntry.UI.OpenUIForm(UIFormType.SettingDialog);
                });
            }

            displayScore = GameLoopManager.Instance.score;
            if (scoreText != null) scoreText.text = $"Score: {displayScore}";
            if (bestScoreText != null)
            {
                // 获取并显示最高分
                int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
                bestScoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), bestScore); // 使用千位分隔符格式化
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RemoveEventListener(Constant.Event.GameOver, OnGameOver);
                EventManager.Instance.RemoveEventListener(Constant.Event.RefreshScore, OnRefreshScore);
            }

            base.OnClose(isShutdown, userData);
        }

        private void OnGameOver(params object[] args)
        {
            // 在 GameMain 中等待填满动画后，会再次触发 GameOver（或者通过直接打开）
            // 注意：GameLoopManager 中触发了 GameOverFillAnimation，然后 GameMain 处理完会再次触发 GameOver 事件
            // 为了防止重复打开，需要做个状态判断或者只在真正结束时打开
            GameEntry.UI.OpenUIForm(UIFormType.UISuccessForm);
        }

        private void OnRefreshScore(params object[] args)
        {
            UpdateScore(GameLoopManager.Instance.score);
        }

        private void UpdateScore(int newScore)
        {
            if (scoreText != null)
            {
                // 分数滚动动画
                DOTween.To(() => displayScore, x =>
                {
                    displayScore = x;
                    scoreText.text = $"Score: {displayScore}";
                }, newScore, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
            }
        }
    }
}