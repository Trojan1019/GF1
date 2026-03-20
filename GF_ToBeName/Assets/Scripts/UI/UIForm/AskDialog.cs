using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CubeCrush.Manager;
using NewSideGame;
using DG.Tweening;

namespace NewSideGame
{
    public class AskDialog : BaseDialog
    {
        [Header("UI Components")] public TextMeshProUGUI MessageText;
        public TextMeshProUGUI TitleText;
        public Button ConfirmButton;
        public Button DenyButton;

        public UGUIParamsDelegate OnClickConfirm;

        private string _titleKey;
        private string _messageKey;

        private void RefreshLocalizedText()
        {
            if (GameEntry.Localization == null) return;

            if (TitleText != null && !string.IsNullOrEmpty(_titleKey))
                TitleText.text = GameEntry.Localization.GetString(_titleKey);

            if (MessageText != null && !string.IsNullOrEmpty(_messageKey))
                MessageText.text = GameEntry.Localization.GetString(_messageKey);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            OnClickConfirm = m_Params.GetDelegage("OnClickConfirm");

            // 现在 Title/Message 传进来的都是 localizationKey，这样语言切换时可以自动刷新
            _titleKey = m_Params.GetStringParams("Title");
            _messageKey = m_Params.GetStringParams("Message");

            EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
            RefreshLocalizedText();
            
            if (ConfirmButton != null)
            {
                ConfirmButton.onClick.RemoveAllListeners();
                ConfirmButton.onClick.AddListener(() =>
                {
                    ConfirmButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);

                    OnClickConfirm?.Invoke();
                    Close();
                });
            }

            if (DenyButton != null)
            {
                DenyButton.onClick.RemoveAllListeners();
                DenyButton.onClick.AddListener(() =>
                {
                    DenyButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    Close();
                });
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
            RefreshLocalizedText();
        }
    }
}