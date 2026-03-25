using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class StageClearNextLevelDialog : UGuiForm
    {
        [SerializeField] private Button nextLevelBtn;
        [SerializeField] private TextMeshProUGUI nextLevelText;
        [SerializeField] private TextMeshProUGUI titleText;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Bind();
            RefreshTitle();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            Unbind();
            base.OnClose(isShutdown, userData);
        }

        private void Bind()
        {
            if (nextLevelBtn == null) return;
            nextLevelBtn.onClick.RemoveAllListeners();
            nextLevelBtn.onClick.AddListener(OnClickNext);
        }

        private void Unbind()
        {
            if (nextLevelBtn == null) return;
            nextLevelBtn.onClick.RemoveAllListeners();
        }

        private void RefreshTitle()
        {
            if (titleText == null) return;
            string localized = GameEntry.Localization.GetString("61");
            if (string.IsNullOrEmpty(localized) || localized.Contains("<NoKey>")) localized = "Stage Complete";
            titleText.text = localized;

            if (nextLevelText != null)
            {
                string nextLocalized = GameEntry.Localization.GetString("30");
                if (string.IsNullOrEmpty(nextLocalized) || nextLocalized.Contains("<NoKey>")) nextLocalized = "Next Level";
                nextLevelText.text = nextLocalized;
            }
        }

        private void OnClickNext()
        {
            if (GameLoopManager.Instance == null) return;
            // 先关闭再切下一关，避免 UI 重叠
            Close();
            GameLoopManager.Instance.NextStage();
        }
    }
}

