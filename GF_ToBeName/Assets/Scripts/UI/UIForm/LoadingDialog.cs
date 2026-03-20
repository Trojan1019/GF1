
using UnityEngine;
using NewSideGame;
using DG.Tweening;

public class LoadingDialog : UGuiForm
{
    private Transform m_Loading => GetRef<Transform>("Loading");
    private TMPro.TextMeshProUGUI m_Text => GetRef<TMPro.TextMeshProUGUI>("Text");
    private Timer timer;
    private bool m_IsPurchase;

    private void ApplyLocalizedText()
    {
        if (m_Text == null) return;
        m_Text.text = m_IsPurchase ? GameEntry.Localization.GetString("43") : GameEntry.Localization.GetString("44");
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);

        m_IsPurchase = m_Params != null && m_Params.GetBoolParams("isPurchase");
        ApplyLocalizedText();

        EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);

        m_Loading.rotation = Quaternion.identity;
        m_Loading.DORotate(new Vector3(0, 0, -360), 5f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);

        timer = this.AttachTimer(m_Params.GetFloatParams("Duration"), () =>
        {
            Close();
        });
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
        }

        base.OnClose(isShutdown, userData);

        Timer.Cancel(timer);
        timer = null;
    }

    private void OnLanguageChanged(object[] args)
    {
        ApplyLocalizedText();
    }
}

