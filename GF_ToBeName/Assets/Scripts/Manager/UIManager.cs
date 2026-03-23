using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework;
using System;

namespace NewSideGame
{
    public static class UIManager
    {
        public static void Toast(string toast, Action callback = null)
        {
            GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", toast)
                    .AddValue("ToastCall", callback));
        }

        public static bool IsLoading
        {
            get
            {
                if (GameEntry.UI == null) return false;
                var loading = GameEntry.UI.GetUIForm(UIFormType.LoadingDialog);
                return loading != null && loading.gameObject.activeInHierarchy;
            }
        }

        public static void ShowLoading(bool isPurchase = false, float duraing = float.MaxValue, GameFrameworkAction closeCB = null)
        {
            UGUIParams uguiParams = UGUIParams.Create(closeCB);
            uguiParams.AddValue("Duration", duraing);
            uguiParams.AddValue("isPurchase", isPurchase);

            GameEntry.UI.OpenUIForm(UIFormType.LoadingDialog, uguiParams);
        }

        public static void CloseLoading()
        {
            if (IsLoading)
                GameEntry.UI.CloseUIForm(UIFormType.LoadingDialog);
        }
    }
}