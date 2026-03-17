using System;
using System.Collections;
using System.Collections.Generic;
using NewSideGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LocalizationForUndeal : MonoBehaviour
{
    public string localizationKey;
    public StrReplace[] replaceList;
    public string appendSignal;//后面附加的符号

    private Text text;
    private TextMeshProUGUI TextMeshText;
    private TextMeshPro meshPro;
    private bool hasListenEvent = false;

    public string LocalizationKey
    {
        set
        {
            localizationKey = value;
            RefreshText();
        }
    }
    private void Awake()
    {
        text = GetComponent<Text>();
        TextMeshText = GetComponent<TextMeshProUGUI>();
        meshPro = GetComponent<TextMeshPro>();
    }

    private void RefreshLocalText(object[] args)
    {
        RefreshText();
    }

    private void Start()
    {
        NewSideGame.EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, RefreshLocalText);
        hasListenEvent = true;

        RefreshText();

    }

    private void RefreshText()
    {
        if (text != null)
        {
            var localization = GetLocalization(localizationKey);
            if (localization != null)
            {
                text.text = localization;
            }
        }
        else if (TextMeshText != null)
        {

            var localization = GetLocalization(localizationKey);
            if (localization != null)
            {
                TextMeshText.text = localization;
            }
        }
        else if (meshPro != null)
        {
            var localization = GetLocalization(localizationKey);
            if (meshPro != null)
            {
                meshPro.text = localization;
            }
        }
    }

    private string GetLocalization(string localizationKey)
    {
        if (GameEntry.Localization == null) return null;

        if (string.IsNullOrEmpty(localizationKey))
        {
            UnityGameFramework.Runtime.Log.Error("{0}/{1}", transform.parent.name, transform.name);
            return null;
        }

        var str = GameEntry.Localization.GetString(localizationKey);
        if (replaceList != null && replaceList.Length > 0)
        {
            foreach (var replace in replaceList)
            {
                str = str.Replace(replace.holder, replace.placement);
            }
        }

        if (!string.IsNullOrEmpty(appendSignal))
        {
            str += appendSignal;
        }
        return str;
    }

    private void OnDestroy()
    {
        if (hasListenEvent)
        {
            NewSideGame.EventManager.Instance.RemoveEventListener(Constant.Event.LanguageChangeSuccess, RefreshLocalText);
            hasListenEvent = false;
        }

    }

#if UNITY_EDITOR
    [ContextMenu("RefreshLanguage")]
    public void Refresh()
    {
        RefreshText();
    }

#endif

    [Serializable]
    public class StrReplace
    {
        public string holder;
        public string placement;
    }
}
