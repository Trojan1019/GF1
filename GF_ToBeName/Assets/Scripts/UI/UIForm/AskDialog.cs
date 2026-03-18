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

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            string TitleStr = m_Params.GetStringParams("Title");
            string MessageStr = m_Params.GetStringParams("Message");
            OnClickConfirm = m_Params.GetDelegage("OnClickConfirm");

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
            base.OnClose(isShutdown, userData);
        }
    }
}