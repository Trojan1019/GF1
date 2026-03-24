using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NewSideGame
{
    public class SkinDialog : BaseDialog
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private SkinItem itemTemplate;
        [SerializeField] private TextMeshProUGUI titleText;

        private readonly List<SkinItem> _runtimeItems = new List<SkinItem>();

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            BindButtons();
            RefreshTitle();
            RebuildList();

            EventManager.Instance.AddEventListener(Constant.Event.SkinUnlockedEvent, OnSkinEventChanged);
            EventManager.Instance.AddEventListener(Constant.Event.SkinAppliedEvent, OnSkinEventChanged);
            EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            EventManager.Instance.RemoveEventListener(Constant.Event.SkinUnlockedEvent, OnSkinEventChanged);
            EventManager.Instance.RemoveEventListener(Constant.Event.SkinAppliedEvent, OnSkinEventChanged);
            EventManager.Instance.RemoveEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
            base.OnClose(isShutdown, userData);
        }

        private void BindButtons()
        {
            if (closeButton == null) return;
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        private void RefreshTitle()
        {
            if (titleText == null) return;
            string localized = GameEntry.Localization.GetString("20001");
            if (string.IsNullOrEmpty(localized) || localized.Contains("<NoKey>"))
                localized = "Skins";
            titleText.text = localized;
        }

        private void RebuildList()
        {
            if (itemTemplate == null || contentRoot == null) return;
            itemTemplate.gameObject.SetActive(false);

            for (int i = 0; i < _runtimeItems.Count; i++)
            {
                if (_runtimeItems[i] != null)
                    Destroy(_runtimeItems[i].gameObject);
            }
            _runtimeItems.Clear();

            SkinManager.Instance.InitializeIfNeeded();
            var configs = SkinManager.Instance.GetSkinConfigs();
            if (configs == null) return;

            for (int i = 0; i < configs.Count; i++)
            {
                var cfg = configs[i];
                if (cfg == null) continue;
                var item = Instantiate(itemTemplate, contentRoot);
                item.gameObject.SetActive(true);
                _runtimeItems.Add(item);

                int remain = SkinManager.Instance.GetRemainingAdCount(cfg.skinId);
                item.Setup(
                    cfg,
                    SkinManager.Instance.GetDisplayName(cfg),
                    SkinManager.Instance.GetState(cfg.skinId),
                    remain,
                    OnClickSkinItem);
            }

            if (scrollRect != null)
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        private void OnClickSkinItem(int skinId)
        {
            bool selected = SkinManager.Instance.TrySelectSkin(skinId);
            if (selected)
            {
                RebuildList();
                return;
            }

            int remain = SkinManager.Instance.GetRemainingAdCount(skinId);
            string title = "20004";
            string messageFmt = GameEntry.Localization.GetString("20005");
            if (string.IsNullOrEmpty(messageFmt) || messageFmt.Contains("<NoKey>"))
                messageFmt = "Watch {0} ads to unlock this skin?";
            string message = string.Format(messageFmt, remain);

            UGUIParams uguiParams = UGUIParams.Create()
                .AddValue("Title", title)
                .AddValue("Message", message)
                .AddDelegage("OnClickConfirm", args =>
                {
                    SkinManager.Instance.TryUnlockByAd(skinId, success =>
                    {
                        if (success)
                        {
                            SkinManager.Instance.TrySelectSkin(skinId);
                            RebuildList();
                            return;
                        }

                        string toast = GameEntry.Localization.GetString("20006");
                        if (string.IsNullOrEmpty(toast) || toast.Contains("<NoKey>"))
                            toast = "Ad failed or interrupted.";
                        UIManager.Toast(toast);
                        RebuildList();
                    });
                });

            GameEntry.UI.OpenUIForm(UIFormType.AskDialog, uguiParams);
        }

        private void OnSkinEventChanged(params object[] args)
        {
            RebuildList();
        }

        private void OnLanguageChanged(params object[] args)
        {
            RefreshTitle();
            RebuildList();
        }
    }
}

