//------------------------------------------------------------
// File : SplashPage.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class SplashPage : UGuiForm
    {
        public static SplashPage splashPage;

        public static void OpenUI()
        {
            if (splashPage == null)
            {
                splashPage = GameEntry.UI.OpenUIInResources<SplashPage>("Prefabs/SplashPage", Constant.UIGroup.Loading, null);
            }
        }

        public static void CloseUI()
        {
            if (splashPage != null)
            {
                UnityEngine.Object.Destroy(splashPage.gameObject);
                splashPage = null;
            }
        }

        public static void OpenLaunchPage()
        {
            if (splashPage != null)
            {
                UGUIParams uguiParams = UGUIParams.Create(CloseUI);
                GameEntry.UI.OpenUI(UIFormType.LaunchPage, LaunchPage.AssetPath, Constant.UIGroup.Loading, Constant.AssetPriority.UIFormAsset, false, uguiParams);
            }
        }
    }
}