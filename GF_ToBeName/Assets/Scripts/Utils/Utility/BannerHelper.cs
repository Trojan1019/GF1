// //------------------------------------------------------------
// // File : BannerHelper.cs
// // Email: yang.li@kingboat.io
// // Desc : 
// //------------------------------------------------------------
// using UnityEngine;
// using NewSideGame;
//
// public static class BannerHelper
// {
//     public static float GetBannerHeight()
//     {
//         // var heightDp = McSdkUtils.GetAdaptiveBannerHeight();//获取dp
//         // var density = McSdkUtils.GetScreenDensity();
//         // var scaleFactor = GameEntry.UI.MainCanvas.scaleFactor;
//         // var heightPx = heightDp * density / scaleFactor;
//         // Debug.Log($"==> lyly GetBannerHeight heightDp: {heightDp}, density: {density}, heightPx: {heightPx}, scaleFactor: {scaleFactor}");
//         // return heightPx;
//     }
//
//     public static Vector2 GetBannerPosition()
//     {
//         RectTransform rect = GameEntry.UI.MainCanvas.GetComponent<RectTransform>();
//         Camera camera = GameEntry.UI.UICamera;
//         Rect safeArea = Screen.safeArea;
//         Vector2 bannerScreenPosition = new Vector2(safeArea.x + safeArea.width / 2, safeArea.y);
//
//         // 将横幅位置从屏幕坐标转为UI坐标
//         Vector2 bannerUiPosition;
//         RectTransformUtility.ScreenPointToLocalPointInRectangle(
//             rect,
//             bannerScreenPosition,
//             camera,
//             out bannerUiPosition
//         );
//         float bannerHeight = GetBannerHeight();
//         bannerUiPosition = new Vector2(bannerUiPosition.x, bannerUiPosition.y + bannerHeight / 2);
//
//         Debug.Log($"==> lyly Banner Screen Position: {bannerScreenPosition}, UI Position: {bannerUiPosition}");
//
//         bannerUiPosition = new Vector2(bannerUiPosition.x, bannerUiPosition.y);
//         return bannerUiPosition;
//     }
// }