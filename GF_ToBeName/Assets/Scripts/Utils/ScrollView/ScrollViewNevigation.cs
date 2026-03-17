using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class ScrollViewNevigation : MonoBehaviour
{

    private ScrollRect scrollRect;
    private RectTransform viewport;
    public RectTransform content;

    public void Init()
    {
        if (scrollRect == null)
        {
            scrollRect = this.GetComponent<ScrollRect>();
        }

        if (viewport == null)
        {
            viewport = this.transform.Find("Viewport").GetComponent<RectTransform>();
        }

        if (content == null)
        {
            content = this.transform.Find("Viewport/Content").GetComponent<RectTransform>();
        }
    }

    public void Nevigate(float offset, float scrollInterval, Action complete = null)
    {
        var rTf = content.GetComponent<RectTransform>();
        rTf.DOAnchorPosY(rTf.anchoredPosition.y + offset, scrollInterval)
            .OnComplete(() => complete?.Invoke());
    }

    // pivotType : 1居中，2置顶
    public void Nevigate(RectTransform item, int pivotType = 1, Action complete = null)
    {
        Vector3 itemCurrentLocalPostion = scrollRect.GetComponent<RectTransform>().InverseTransformVector(ConvertLocalPosToWorldPos(item, pivotType));
        Vector3 itemTargetLocalPos = scrollRect.GetComponent<RectTransform>().InverseTransformVector(ConvertLocalPosToWorldPos(viewport, pivotType));

        Vector3 diff = itemTargetLocalPos - itemCurrentLocalPostion;
        diff.z = 0.0f;

        var newNormalizedPosition = new Vector2(
            diff.x / (content.GetComponent<RectTransform>().rect.width - viewport.rect.width),
            diff.y / (content.GetComponent<RectTransform>().rect.height - viewport.rect.height));

        newNormalizedPosition = scrollRect.GetComponent<ScrollRect>().normalizedPosition - newNormalizedPosition;

        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

        DOTween.To(() => scrollRect.GetComponent<ScrollRect>().normalizedPosition, x => scrollRect.GetComponent<ScrollRect>().normalizedPosition = x, newNormalizedPosition, 0.6f)
            .OnComplete(() => complete?.Invoke());
    }

    public  void NevigateImmediate(RectTransform item, int pivotType = 1)
    { 
        Vector3 itemCurrentLocalPostion = scrollRect.GetComponent<RectTransform>().InverseTransformVector(ConvertLocalPosToWorldPos(item, pivotType));
        Vector3 itemTargetLocalPos = scrollRect.GetComponent<RectTransform>().InverseTransformVector(ConvertLocalPosToWorldPos(viewport, pivotType));

        Vector3 diff = itemTargetLocalPos - itemCurrentLocalPostion;
        diff.z = 0.0f;

        var newNormalizedPosition = new Vector2(
            diff.x / (content.GetComponent<RectTransform>().rect.width - viewport.rect.width),
            diff.y / (content.GetComponent<RectTransform>().rect.height - viewport.rect.height));

        newNormalizedPosition = scrollRect.GetComponent<ScrollRect>().normalizedPosition - newNormalizedPosition;

        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

        scrollRect.GetComponent<ScrollRect>().normalizedPosition = newNormalizedPosition ;

    }

    // pivotType : 1居中，2置顶
    private Vector3 ConvertLocalPosToWorldPos(RectTransform target, int pivotType = 1)
    {
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            pivotType == 2 ? (0.5f * target.rect.size.y) : ((0.5f - target.pivot.y) * target.rect.size.y),
            0f);

        var localPosition = target.localPosition + (pivotType != 0 ? pivotOffset : new Vector3(0, target.sizeDelta.y / 2, 0));

        return target.parent.TransformPoint(localPosition);
    }
}