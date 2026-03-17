//------------------------------------------------------------
// File : AspectSafeAreaHelper.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using System;
using NewSideGame;

public class AspectSafeAreaHelper : MonoBehaviour
{
    // public static readonly float anchorMaxY = 0.95f;
    // public static readonly float anchorMinY = 0.05f;
    // private void Awake()
    // {
    //     Refresh();
    // }

    // private void Refresh()
    // {
    //     RectTransform rectTransform = GetComponent<RectTransform>();
    //     Rect safeArea = Screen.safeArea;
    //     float screenWidth = Screen.width;
    //     float screenHeight = Screen.height;
    //
    //     var heightDp = McSdkUtils.GetAdaptiveBannerHeight();
    //     var density = McSdkUtils.GetScreenDensity();
    //     var bannerHeight = heightDp * density;
    //     float bannerHeightNormalized = bannerHeight / screenHeight;
    //
    //     Vector2 anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
    //     Vector2 anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
    //     anchorMin = new Vector2(anchorMin.x, anchorMin.y + bannerHeightNormalized);
    //
    //     Vector2 resultAnchorMin = new Vector2(anchorMin.x, anchorMin.y <= anchorMinY ? anchorMinY : anchorMin.y);
    //     Vector2 resultAnchorMax = new Vector2(anchorMax.x, anchorMax.y >= anchorMaxY ? anchorMaxY : anchorMax.y);
    //     rectTransform.anchorMin = new Vector2(resultAnchorMin.x, resultAnchorMin.y);
    //     rectTransform.anchorMax = new Vector2(resultAnchorMax.x, resultAnchorMax.y);
    //     rectTransform.anchoredPosition = Vector2.zero;
    // }
}