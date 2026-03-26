using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewSideGame;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Spine.Unity;

public abstract class BaseDialog : UGuiForm
{
    [SerializeField] protected Image maskBg;
    [SerializeField] private ScrollRect[] scrollRects;
    private SkeletonGraphic[] skeletonGraphics;

    private GraphicRaycaster graphicRaycaster;
    protected float maskBgAlpha = 0.9f;

    private bool isInit;

    private bool hasClickedClose;

    private Sequence sequence;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        if (maskBg == null)
        {
            var imgMask = transform.Find("Mask");
            if (imgMask)
                maskBg = imgMask.GetComponent<Image>();
        }
        if (maskBg)
        {
            maskBgAlpha = maskBg.color.a;
        }
        if (!isInit)
        {
            scrollRects = GetComponentsInChildren<ScrollRect>();
            for (int i = 0; i < scrollRects.Length; i++)
            {
                scrollRects[i].enabled = false;
            }
        }
        if (maskBg != null)
        {
            UGUIEventListener.Get(maskBg.gameObject).onClick = OnClickMask;
        }
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        skeletonGraphics = GetComponentsInChildren<SkeletonGraphic>(true);
        isInit = true;
    }
    public virtual void OnClickMask(GameObject obj)
    {
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        UIAnimationFinish = false;

        if (m_Params == null || !m_Params.HasKey("Anim") || m_Params.GetBoolParams("Anim"))
        {
            if (!ReferenceEquals(sequence, null) && sequence.active) sequence.Kill();
            sequence = DOTween.Sequence();

            m_CanvasGroup.alpha = 0;
            if (skeletonGraphics != null && skeletonGraphics.Length > 0)
            {
                foreach (var ske in skeletonGraphics)
                {
                    // ske.material = CanvasSkeletonMaterial;
                    // ske.timeScale = 0f;
                }
            }
            transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            bool isInit = false;

            sequence.Append(transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack).OnComplete(OnCompleteScale));
            sequence.Join(m_CanvasGroup.DOFade(1f, 0.6f).SetEase(Ease.InSine).OnUpdate(() =>
            {
                if (!isInit && m_CanvasGroup.alpha >= 0.3f && skeletonGraphics != null && skeletonGraphics.Length > 0)
                {
                    isInit = true;
                    foreach (var ske in skeletonGraphics)
                    {
                        // ske.timeScale = 1f;
                    }
                }
            }).OnComplete(() =>
            {
                if (skeletonGraphics != null && skeletonGraphics.Length > 0)
                {
                    foreach (var ske in skeletonGraphics)
                    {
                        // ske.material = DefaultSkeletonMaterial;
                    }
                }
            }));
        }
        else
        {
            OnCompleteScale();
        }
        graphicRaycaster.enabled = true;
    }
    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);

        hasClickedClose = false;

        if (!ReferenceEquals(sequence, null) && sequence.active) sequence.Kill();
    }

    public override void Close()
    {
        if (hasClickedClose) return;
        hasClickedClose = true;
        graphicRaycaster.enabled = false;

        if (!ReferenceEquals(sequence, null) && sequence.active) sequence.Kill();
        sequence = DOTween.Sequence();

        m_CanvasGroup.alpha = 0.6f;
        if (skeletonGraphics != null && skeletonGraphics.Length > 0)
        {
            foreach (var ske in skeletonGraphics)
            {
                // ske.material = CanvasSkeletonMaterial;
            }
        }
        transform.localScale = Vector3.one;
        sequence.Append(m_CanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear)).OnComplete(() =>
        {
            base.Close();
        });
    }

    public void CloseWithAnim(bool isAnim)
    {
        if (isAnim)
        {
            Close();
        }
        else
        {
            base.Close();
        }
    }

    protected override void OnOpenCallback() { }

    protected virtual void OnCompleteScale()
    {
        if (scrollRects != null && scrollRects.Length > 0)
        {
            for (int i = 0; i < scrollRects.Length; i++)
            {
                scrollRects[i].enabled = true;
            }
            scrollRects = null;
        }

        UIAnimationFinish = true;
        if (m_Params != null) m_Params.OnOpenCallback?.Invoke(this);
    }
}
