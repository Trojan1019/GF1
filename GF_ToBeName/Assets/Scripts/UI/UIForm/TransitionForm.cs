using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NewSideGame;
using System.Collections;

public class TransitionForm : UGuiForm
{
    public static void Open(GameFramework.GameFrameworkAction onChangeScene)
    {
        UGUIParams uGUIParams = UGUIParams.Create();
        uGUIParams.AddValue("onChangeScene", onChangeScene);
        GameEntry.UI.OpenUIForm(UIFormType.TransitionForm, uGUIParams);
    }
    [SerializeField] private RectTransform rootRectTransform;
    private Image m_Bg => GetRef<Image>("Background");
    private Sequence m_Sequence;
    private readonly float m_Duration = 0.4f;
    private Ease _closeEase = Ease.OutSine;
    private Ease _openEase = Ease.InSine;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        // m_Bg.sprite = Resources.Load<Sprite>("Prefabs/HomeBg");
        m_Bg.GetComponent<AspectFullImage>().Refresh();
        rootRectTransform.pivot = new Vector2(0.5f, 1f);
        rootRectTransform.anchoredPosition = new Vector2(0, GameEntry.UI.MainCanvas.GetComponent<RectTransform>().sizeDelta.y);
        StartCoroutine(ExecuteChangeScene());
    }

    private void ClearSequence()
    {
        if (m_Sequence.IsActive()) m_Sequence.Kill();
        m_Sequence = DOTween.Sequence();
    }

    private IEnumerator ExecuteChangeScene()
    {
        ClearSequence();
        bool tracking = false;
        m_Sequence.Append(rootRectTransform.DOAnchorPosY(0, m_Duration).SetEase(_closeEase));
        m_Sequence.OnComplete(() => tracking = true);

        yield return new WaitUntil(() => tracking);

        if (m_Params.HasKey("onChangeScene"))
        {
            GameFramework.GameFrameworkAction onChangeScene = (GameFramework.GameFrameworkAction)m_Params.GetObjectParams("onChangeScene");
            onChangeScene?.Invoke();
        }

        yield return new WaitUntil(() => GameEntry.Procedure.CurrentProcedure is not ProcedureChangeScene);

        yield return new WaitForSeconds(0.3f);

        ClearSequence();
        tracking = false;
        m_Sequence.Append(rootRectTransform.DOAnchorPosY(GameEntry.UI.MainCanvas.GetComponent<RectTransform>().sizeDelta.y, m_Duration).SetEase(_openEase));
        m_Sequence.OnComplete(() => tracking = true);

        yield return new WaitUntil(() => tracking);
        Close();
    }
}

