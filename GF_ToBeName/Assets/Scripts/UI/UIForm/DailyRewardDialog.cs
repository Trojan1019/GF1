using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class DailyRewardDialog : BaseDialog
    {
        [System.Serializable]
        public class SignDayCell
        {
            public GameObject root;
            public TextMeshProUGUI dayText;
            public Image doneIcon;
            public Image todayIcon;
        }

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI streakText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI rewardListText;
        [SerializeField] private Button signButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private List<SignDayCell> dayCells = new List<SignDayCell>();
        [SerializeField] private Transform signFxRoot;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            BindButtons();
            RefreshUI();
            EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            EventManager.Instance.RemoveEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
            base.OnClose(isShutdown, userData);
        }

        private void BindButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }

            if (signButton != null)
            {
                signButton.onClick.RemoveAllListeners();
                signButton.onClick.AddListener(OnClickSign);
            }
        }

        private void OnClickSign()
        {
            var result = ProxyManager.UserProxy.SignToday();
            if (result.alreadySignedToday)
            {
                string msg = GameEntry.Localization.GetString("20033");
                if (string.IsNullOrEmpty(msg) || msg.Contains("<NoKey>")) msg = "Already signed today.";
                UIManager.Toast(msg);
                return;
            }

            GameEntry.Sound.PlaySound(Constant.SoundId.Collect);
            PlaySignFx();

            if (result.unlockedSkinReward)
            {
                string msg = GameEntry.Localization.GetString("20034");
                if (string.IsNullOrEmpty(msg) || msg.Contains("<NoKey>")) msg = "Skin unlocked by 3-day streak!";
                UIManager.Toast(msg);
            }

            RefreshUI();
        }

        private void PlaySignFx()
        {
            if (signFxRoot == null) return;
            signFxRoot.DOKill();
            signFxRoot.localScale = Vector3.one * 0.9f;
            Sequence seq = DOTween.Sequence();
            seq.Append(signFxRoot.DOScale(1.1f, 0.18f).SetEase(Ease.OutBack).SetUpdate(true));
            seq.Append(signFxRoot.DOScale(1f, 0.18f).SetEase(Ease.OutQuad).SetUpdate(true));
        }

        private void RefreshUI()
        {
            var user = ProxyManager.UserProxy.userModel;

            string title = GameEntry.Localization.GetString("20030");
            if (string.IsNullOrEmpty(title) || title.Contains("<NoKey>")) title = "Daily Sign-In";
            if (titleText != null) titleText.text = title;

            if (streakText != null)
            {
                string fmt = GameEntry.Localization.GetString("20031");
                if (string.IsNullOrEmpty(fmt) || fmt.Contains("<NoKey>")) fmt = "Current streak: {0} days";
                streakText.text = string.Format(fmt, user.dailySignStreakDays);
            }

            int target = 3;
            float progress = Mathf.Clamp01((float)user.dailySignStreakDays / target);
            if (progressSlider != null) progressSlider.value = progress;
            if (progressText != null)
            {
                progressText.text = $"{Mathf.Min(user.dailySignStreakDays, target)}/{target}";
            }

            if (rewardListText != null)
            {
                string txt = GameEntry.Localization.GetString("20032");
                if (string.IsNullOrEmpty(txt) || txt.Contains("<NoKey>")) txt = "Reward: unlock first skin after 3-day streak";
                rewardListText.text = txt;
            }

            // Calendar-like 7 day view
            for (int i = 0; i < dayCells.Count; i++)
            {
                var cell = dayCells[i];
                if (cell == null || cell.root == null) continue;
                cell.root.SetActive(true);
                int day = i + 1;
                if (cell.dayText != null) cell.dayText.text = day.ToString();

                bool signed = day <= user.dailySignStreakDays;
                bool today = day == Mathf.Clamp(user.dailySignStreakDays + (ProxyManager.UserProxy.CanSignToday() ? 1 : 0), 1, 7);
                if (cell.doneIcon != null) cell.doneIcon.gameObject.SetActive(signed);
                if (cell.todayIcon != null) cell.todayIcon.gameObject.SetActive(today && !signed);
            }

            if (signButton != null)
            {
                signButton.interactable = ProxyManager.UserProxy.CanSignToday();
            }
        }

        private void OnLanguageChanged(params object[] args)
        {
            RefreshUI();
        }
    }
}

