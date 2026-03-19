//------------------------------------------------------------
// File : AdConfig.cs
// Desc : 广告配置类
//------------------------------------------------------------

using System;
using UnityEngine;

namespace NewSideGame
{
    [Serializable]
    public class AdConfig
    {
        [Header("测试模式")]
        public bool isTestMode = true;
        
        [Header("Android 配置")]
        public string androidAppId = "ca-app-pub-3940256099942544~3347511713";
        
        [Header("iOS 配置")]
        public string iosAppId = "ca-app-pub-3940256099942544~1458002511";
        
        [Header("广告展示频率配置")]
        public float interstitialCooldownSeconds = 120f;
        public int maxInterstitialsPerSession = 5;
        
        [Header("预加载配置")]
        public bool preloadBannerOnStart = true;
        public bool preloadRewardedOnStart = true;
        public bool preloadInterstitialOnStart = true;
    }
}
