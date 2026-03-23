using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace NewSideGame
{
    public class TMPHelper
    {
        private const string TMPSettingAssetPath = "Assets/RawAsset/Fonts/TextMeshPro/TMP Settings.asset";

        public static void LoadSettingsDefault()
        {
        }

        public static void LoadSetting(Action onCompleteAction = null)
        {
            GameEntry.Resource.LoadAssetSync<TMP_Settings>(TMPSettingAssetPath, (resource) =>
            {
                System.Type settingsType = resource.GetType();
                System.Reflection.FieldInfo settingsInstanceInfo = settingsType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
                settingsInstanceInfo?.SetValue(null, resource);

                onCompleteAction?.Invoke();
            });
        }
    }
}
