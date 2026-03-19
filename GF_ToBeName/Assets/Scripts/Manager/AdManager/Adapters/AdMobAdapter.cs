//------------------------------------------------------------
// File : AdMobAdapter.cs
// Desc : AdMob 适配器 - AdMob SDK 具体实现
//------------------------------------------------------------

using System;
using UnityEngine;

namespace NewSideGame
{
    public class AdMobAdapter : IAdAdapter
    {
        private AdConfig _config;
        private bool _isInitialized;
        
        public event Action<string> OnAdLoaded;
        public event Action<string, string> OnAdFailedToLoad;
        public event Action<string> OnAdOpened;
        public event Action<string> OnAdClosed;
        public event Action<string, string> OnAdRewarded;
        public event Action<string> OnAdImpressionRecorded;
        
#if UNITY_ADS || GOOGLE_MOBILE_ADS
        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        
        private string _currentBannerAdUnitId;
        private string _currentInterstitialAdUnitId;
        private string _currentRewardedAdUnitId;
        
        private Action<string> _pendingRewardedCallback;
        private Action _pendingRewardedFailedCallback;
#endif
        
        public void Initialize(AdConfig config)
        {
            _config = config;
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            try
            {
                MobileAds.Initialize(initStatus =>
                {
                    Debug.Log("[AdMobAdapter] 初始化成功");
                    _isInitialized = true;
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdMobAdapter] 初始化失败: {e.Message}");
            }
#else
            Debug.Log("[AdMobAdapter] AdMob SDK 未导入，请先导入 Google Mobile Ads Unity 插件");
            _isInitialized = true;
#endif
        }
        
        #region Banner 广告
        
        public void LoadBanner(string adUnitId)
        {
            string actualAdUnitId = GetAdUnitId(adUnitId);
            Debug.Log($"[AdMobAdapter] 加载 Banner: {actualAdUnitId}");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (!_isInitialized) return;
            
            DestroyBanner();
            
            _currentBannerAdUnitId = actualAdUnitId;
            
            _bannerView = new BannerView(actualAdUnitId, AdSize.Banner, AdPosition.Bottom);
            
            _bannerView.OnBannerAdLoaded += () => HandleBannerLoaded(actualAdUnitId);
            _bannerView.OnBannerAdLoadFailed += (error) => HandleBannerFailed(actualAdUnitId, error.GetMessage());
            _bannerView.OnAdOpening += () => HandleAdOpened(actualAdUnitId);
            _bannerView.OnAdClosed += () => HandleAdClosed(actualAdUnitId);
            _bannerView.OnAdPaid += (adValue) => HandleAdImpression(actualAdUnitId);
            
            AdRequest request = new AdRequest();
            _bannerView.LoadAd(request);
#else
            OnAdLoaded?.Invoke(actualAdUnitId);
#endif
        }
        
        public void ShowBanner(string adUnitId)
        {
            Debug.Log($"[AdMobAdapter] 显示 Banner");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (_bannerView != null)
            {
                _bannerView.Show();
            }
#endif
        }
        
        public void HideBanner()
        {
            Debug.Log($"[AdMobAdapter] 隐藏 Banner");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (_bannerView != null)
            {
                _bannerView.Hide();
            }
#endif
        }
        
        public void DestroyBanner()
        {
            Debug.Log($"[AdMobAdapter] 销毁 Banner");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
#endif
        }
        
        #endregion
        
        #region 插屏广告
        
        public void LoadInterstitial(string adUnitId)
        {
            string actualAdUnitId = GetAdUnitId(adUnitId);
            Debug.Log($"[AdMobAdapter] 加载插屏广告: {actualAdUnitId}");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (!_isInitialized) return;
            
            _currentInterstitialAdUnitId = actualAdUnitId;
            
            InterstitialAd.Load(actualAdUnitId, new AdRequest(), (interstitialAd, error) =>
            {
                if (error != null)
                {
                    HandleInterstitialFailed(actualAdUnitId, error.GetMessage());
                    return;
                }
                
                _interstitialAd = interstitialAd;
                
                _interstitialAd.OnAdFullScreenContentOpened += () => HandleAdOpened(actualAdUnitId);
                _interstitialAd.OnAdFullScreenContentClosed += () => HandleAdClosed(actualAdUnitId);
                _interstitialAd.OnAdPaid += (adValue) => HandleAdImpression(actualAdUnitId);
                
                HandleInterstitialLoaded(actualAdUnitId);
            });
#else
            OnAdLoaded?.Invoke(actualAdUnitId);
#endif
        }
        
        public bool ShowInterstitial(string adUnitId)
        {
            Debug.Log($"[AdMobAdapter] 显示插屏广告");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
                return true;
            }
            
            Debug.LogWarning("[AdMobAdapter] 插屏广告未准备好");
            return false;
#else
            OnAdOpened?.Invoke(adUnitId);
            OnAdClosed?.Invoke(adUnitId);
            return true;
#endif
        }
        
        #endregion
        
        #region 激励广告
        
        public void LoadRewardedAd(string adUnitId)
        {
            string actualAdUnitId = GetAdUnitId(adUnitId);
            Debug.Log($"[AdMobAdapter] 加载激励广告: {actualAdUnitId}");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (!_isInitialized) return;
            
            _currentRewardedAdUnitId = actualAdUnitId;
            
            RewardedAd.Load(actualAdUnitId, new AdRequest(), (rewardedAd, error) =>
            {
                if (error != null)
                {
                    HandleRewardedFailed(actualAdUnitId, error.GetMessage());
                    return;
                }
                
                _rewardedAd = rewardedAd;
                
                _rewardedAd.OnAdFullScreenContentOpened += () => HandleAdOpened(actualAdUnitId);
                _rewardedAd.OnAdFullScreenContentClosed += () => HandleAdClosed(actualAdUnitId);
                _rewardedAd.OnAdPaid += (adValue) => HandleAdImpression(actualAdUnitId);
                
                HandleRewardedLoaded(actualAdUnitId);
            });
#else
            OnAdLoaded?.Invoke(actualAdUnitId);
#endif
        }
        
        public bool ShowRewardedAd(string adUnitId, Action<string> onRewarded, Action onFailed)
        {
            Debug.Log($"[AdMobAdapter] 显示激励广告");
            
#if UNITY_ADS || GOOGLE_MOBILE_ADS
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _pendingRewardedCallback = onRewarded;
                _pendingRewardedFailedCallback = onFailed;
                
                _rewardedAd.Show(reward =>
                {
                    string rewardType = reward.Type;
                    HandleRewarded(adUnitId, rewardType);
                    _pendingRewardedCallback?.Invoke(rewardType);
                    _pendingRewardedCallback = null;
                });
                
                return true;
            }
            
            Debug.LogWarning("[AdMobAdapter] 激励广告未准备好");
            onFailed?.Invoke();
            return false;
#else
            OnAdOpened?.Invoke(adUnitId);
            OnAdRewarded?.Invoke(adUnitId, "reward");
            OnAdClosed?.Invoke(adUnitId);
            onRewarded?.Invoke("reward");
            return true;
#endif
        }
        
        #endregion
        
        #region 事件处理
        
        private string GetAdUnitId(string adUnitId)
        {
            if (_config.isTestMode)
            {
                if (adUnitId.Contains("banner")) return Constant.Ad.admob_test_banner;
                if (adUnitId.Contains("interstitial")) return Constant.Ad.admob_test_interstitial;
                if (adUnitId.Contains("rewarded")) return Constant.Ad.admob_test_rewarded;
            }
            return adUnitId;
        }
        
#if UNITY_ADS || GOOGLE_MOBILE_ADS
        private void HandleBannerLoaded(string adUnitId)
        {
            OnAdLoaded?.Invoke(adUnitId);
        }
        
        private void HandleBannerFailed(string adUnitId, string error)
        {
            OnAdFailedToLoad?.Invoke(adUnitId, error);
        }
        
        private void HandleInterstitialLoaded(string adUnitId)
        {
            OnAdLoaded?.Invoke(adUnitId);
        }
        
        private void HandleInterstitialFailed(string adUnitId, string error)
        {
            OnAdFailedToLoad?.Invoke(adUnitId, error);
        }
        
        private void HandleRewardedLoaded(string adUnitId)
        {
            OnAdLoaded?.Invoke(adUnitId);
        }
        
        private void HandleRewardedFailed(string adUnitId, string error)
        {
            OnAdFailedToLoad?.Invoke(adUnitId, error);
            _pendingRewardedFailedCallback?.Invoke();
            _pendingRewardedFailedCallback = null;
        }
        
        private void HandleRewarded(string adUnitId, string rewardType)
        {
            OnAdRewarded?.Invoke(adUnitId, rewardType);
        }
#endif
        
        private void HandleAdOpened(string adUnitId)
        {
            OnAdOpened?.Invoke(adUnitId);
        }
        
        private void HandleAdClosed(string adUnitId)
        {
            OnAdClosed?.Invoke(adUnitId);
        }
        
        private void HandleAdImpression(string adUnitId)
        {
            OnAdImpressionRecorded?.Invoke(adUnitId);
        }
        
        #endregion
    }
}
