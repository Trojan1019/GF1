using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class GameFailRetryDialog : BaseDialog
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI adBtnText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button adReviveButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject loadingNode;

        private bool _isWatchingAd;
        private bool _pendingReviveAfterClose;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            RefreshTexts();
            BindButtons();
            SetLoading(false);
        }

        private void RefreshTexts()
        {
            if (titleText != null)
            {
                string txt = GameEntry.Localization.GetString("76");
                if (string.IsNullOrEmpty(txt) || txt.Contains("<NoKey>")) txt = "Failed";
                titleText.text = txt;
            }

            if (descText != null)
            {
                string txt = GameEntry.Localization.GetString("77");
                if (string.IsNullOrEmpty(txt) || txt.Contains("<NoKey>")) txt = "Try again or watch an ad to revive.";
                descText.text = txt;
            }

            if (adBtnText != null)
            {
                string txt = GameEntry.Localization.GetString("78");
                if (string.IsNullOrEmpty(txt) || txt.Contains("<NoKey>")) txt = "Watch Ad Revive";
                adBtnText.text = txt;
            }
        }

        private void BindButtons()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() =>
                {
                    if (_isWatchingAd) return;
                    retryButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 10, 1f).SetUpdate(true);
                    GameLoopManager.Instance.Restart();
                    Close();
                });
            }

            if (adReviveButton != null)
            {
                adReviveButton.onClick.RemoveAllListeners();
                adReviveButton.onClick.AddListener(OnClickAdRevive);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() =>
                {
                    if (_isWatchingAd) return;
                    SceneHelper.LoadHomeScene(() => { });
                    Close();
                });
            }
        }

        private void OnClickAdRevive()
        {
            if (_isWatchingAd) return;
            _isWatchingAd = true;
            SetLoading(true);

            bool shown = AdManager.Instance.ShowRewardedAd(
                Constant.Ad.tt_get_booster_1_rewarded,
                rewardType =>
                {
                    _isWatchingAd = false;
                    SetLoading(false);

                    // 确保面板关闭后再触发清台特效
                    _pendingReviveAfterClose = true;
                    Close();
                },
                () =>
                {
                    _isWatchingAd = false;
                    SetLoading(false);
                    string msg = GameEntry.Localization.GetString("79");
                    if (string.IsNullOrEmpty(msg) || msg.Contains("<NoKey>")) msg = "Ad unavailable. Restarting...";
                    UIManager.Toast(msg);
                    GameLoopManager.Instance.Restart();
                    Close();
                });

            if (!shown)
            {
                _isWatchingAd = false;
                SetLoading(false);
                string msg = GameEntry.Localization.GetString("79");
                if (string.IsNullOrEmpty(msg) || msg.Contains("<NoKey>")) msg = "Ad unavailable. Restarting...";
                UIManager.Toast(msg);
                GameLoopManager.Instance.Restart();
                Close();
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            if (_pendingReviveAfterClose)
            {
                _pendingReviveAfterClose = false;
                bool ok = GameLoopManager.Instance.ReviveByAdClearBoard();
                if (!ok)
                {
                    string msg = GameEntry.Localization.GetString("80");
                    if (string.IsNullOrEmpty(msg) || msg.Contains("<NoKey>")) msg = "Revive failed.";
                    UIManager.Toast(msg);
                }
            }
        }

        private void SetLoading(bool show)
        {
            if (loadingNode != null)
                loadingNode.SetActive(show);
            if (adReviveButton != null)
                adReviveButton.interactable = !show;
            if (retryButton != null)
                retryButton.interactable = !show;
        }
    }
}

