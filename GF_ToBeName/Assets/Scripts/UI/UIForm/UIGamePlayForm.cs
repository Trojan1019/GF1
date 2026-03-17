using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockBlast.Manager;
using NewSideGame;

namespace NewSideGame
{
    public class UIGamePlayForm : UGuiForm
    {
        [Header("UI Components")]
        public TextMeshProUGUI scoreText;
        public Button restartButton;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            EventManager.Instance.AddEventListener(Constant.Event.GameOver, OnGameOver);
            EventManager.Instance.AddEventListener(Constant.Event.RefreshScore, OnRefreshScore);
            
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => GameLoopManager.Instance.Restart());
            }
            
            // Initial update
            UpdateScore(GameLoopManager.Instance.score);
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
            // Open Game Over Dialog or Show Panel
            // For now, let's open a dialog if available, or just log
            // Assuming UISuccessForm is a placeholder for game over/win
            GameEntry.UI.OpenUIForm(UIFormType.UISuccessForm);
        }

        private void OnRefreshScore(params object[] args)
        {
            UpdateScore(GameLoopManager.Instance.score);
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
    }
}
