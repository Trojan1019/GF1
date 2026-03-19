//------------------------------------------------------------
// File : AdManager.cs
// Desc : 广告管理器 - 统一广告管理接口
//------------------------------------------------------------

using System;
using UnityEngine;

namespace NewSideGame
{
    public class AdManager : MonoSingleton<AdManager>
    {
        [Header("配置")] public AdConfig config;

        private IAdAdapter _adapter;
        private DateTime _lastInterstitialTime;
        private int _interstitialCountThisSession;

        public bool IsInitialized { get; private set; }
        public bool IsAdRemoved { get; private set; }

        public event Action<string> OnAdLoaded;
        public event Action<string, string> OnAdFailedToLoad;
        public event Action<string> OnAdOpened;
        public event Action<string> OnAdClosed;
        public event Action<string, string> OnAdRewarded;
        public event Action<string> OnAdImpressionRecorded;
        
        private void Start()
        {
            if (config == null)
            {
                config = new AdConfig();
            }

            _lastInterstitialTime = DateTime.MinValue;
            _interstitialCountThisSession = 0;
            
            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            try
            {
                IsAdRemoved = CheckAdRemovedStatus();

                _adapter = new AdMobAdapter();
                _adapter.Initialize(config);

                _adapter.OnAdLoaded += HandleAdLoaded;
                _adapter.OnAdFailedToLoad += HandleAdFailedToLoad;
                _adapter.OnAdOpened += HandleAdOpened;
                _adapter.OnAdClosed += HandleAdClosed;
                _adapter.OnAdRewarded += HandleAdRewarded;
                _adapter.OnAdImpressionRecorded += HandleAdImpressionRecorded;

                IsInitialized = true;
                Debug.Log("[AdManager] 初始化成功");

                if (!IsAdRemoved)
                {
                    PreloadAds();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdManager] 初始化失败: {e.Message}");
            }
        }

        private bool CheckAdRemovedStatus()
        {
            if (ProxyManager.Instance != null && ProxyManager.UserProxy != null)
            {
                return ProxyManager.UserProxy.userModel.isAdRemove;
            }

            return false;
        }

        private void PreloadAds()
        {
            if (config.preloadBannerOnStart)
            {
                LoadBanner(config.isTestMode ? Constant.Ad.admob_test_banner : Constant.Ad.admob_banner_home);
            }

            if (config.preloadRewardedOnStart)
            {
                LoadRewardedAd(config.isTestMode
                    ? Constant.Ad.admob_test_rewarded
                    : Constant.Ad.admob_rewarded_double_score);
            }

            if (config.preloadInterstitialOnStart)
            {
                LoadInterstitial(config.isTestMode
                    ? Constant.Ad.admob_test_interstitial
                    : Constant.Ad.admob_interstitial_game_over);
            }
        }

        #region Banner 广告

        public void LoadBanner(string adUnitId)
        {
            if (IsAdRemoved) return;
            if (!IsInitialized)
            {
                Debug.LogWarning("[AdManager] 未初始化");
                return;
            }

            _adapter.LoadBanner(adUnitId);
        }

        public void ShowBanner(string adUnitId)
        {
            if (IsAdRemoved) return;
            if (!IsInitialized)
            {
                Debug.LogWarning("[AdManager] 未初始化");
                return;
            }

            _adapter.ShowBanner(adUnitId);
        }

        public void HideBanner()
        {
            if (!IsInitialized) return;
            _adapter.HideBanner();
        }

        public void DestroyBanner()
        {
            if (!IsInitialized) return;
            _adapter.DestroyBanner();
        }

        #endregion

        #region 插屏广告

        public void LoadInterstitial(string adUnitId)
        {
            if (IsAdRemoved) return;
            if (!IsInitialized)
            {
                Debug.LogWarning("[AdManager] 未初始化");
                return;
            }

            _adapter.LoadInterstitial(adUnitId);
        }

        public bool ShowInterstitial(string adUnitId)
        {
            if (IsAdRemoved) return false;
            if (!IsInitialized)
            {
                Debug.LogWarning("[AdManager] 未初始化");
                return false;
            }

            if (!CanShowInterstitial())
            {
                Debug.Log("[AdManager] 插屏广告展示频率限制");
                return false;
            }

            if (_adapter.ShowInterstitial(adUnitId))
            {
                _lastInterstitialTime = DateTime.Now;
                _interstitialCountThisSession++;
                return true;
            }

            return false;
        }

        private bool CanShowInterstitial()
        {
            if (_interstitialCountThisSession >= config.maxInterstitialsPerSession)
                return false;

            TimeSpan timeSinceLast = DateTime.Now - _lastInterstitialTime;
            return timeSinceLast.TotalSeconds >= config.interstitialCooldownSeconds;
        }

        #endregion

        #region 激励广告

        public void LoadRewardedAd(string adUnitId)
        {
            if (IsAdRemoved) return;
            if (!IsInitialized)
            {
                Debug.LogWarning("[AdManager] 未初始化");
                return;
            }

            _adapter.LoadRewardedAd(adUnitId);
        }

        public bool ShowRewardedAd(string adUnitId, Action<string> onRewarded = null, Action onFailed = null)
        {
            if (IsAdRemoved)
            {
                onRewarded?.Invoke("ad_removed");
                return true;
            }

            if (!IsInitialized)
            {
                Debug.LogWarning("[AdManager] 未初始化");
                onFailed?.Invoke();
                return false;
            }

            return _adapter.ShowRewardedAd(adUnitId, onRewarded, onFailed);
        }

        #endregion

        #region 事件处理

        private void HandleAdLoaded(string adUnitId)
        {
            OnAdLoaded?.Invoke(adUnitId);
            EventManager.Instance?.NotifyEvent(Constant.Event.AdLoaded, adUnitId);
        }

        private void HandleAdFailedToLoad(string adUnitId, string error)
        {
            OnAdFailedToLoad?.Invoke(adUnitId, error);
            EventManager.Instance?.NotifyEvent(Constant.Event.AdFailedToLoad, adUnitId, error);
        }

        private void HandleAdOpened(string adUnitId)
        {
            OnAdOpened?.Invoke(adUnitId);
            EventManager.Instance?.NotifyEvent(Constant.Event.AdOpened, adUnitId);
        }

        private void HandleAdClosed(string adUnitId)
        {
            OnAdClosed?.Invoke(adUnitId);
            EventManager.Instance?.NotifyEvent(Constant.Event.AdClosed, adUnitId);
        }

        private void HandleAdRewarded(string adUnitId, string rewardType)
        {
            OnAdRewarded?.Invoke(adUnitId, rewardType);
            EventManager.Instance?.NotifyEvent(Constant.Event.AdRewarded, adUnitId, rewardType);
        }

        private void HandleAdImpressionRecorded(string adUnitId)
        {
            OnAdImpressionRecorded?.Invoke(adUnitId);
            EventManager.Instance?.NotifyEvent(Constant.Event.AdImpressionRecorded, adUnitId);
        }

        #endregion

        public void SetAdRemoved(bool removed)
        {
            IsAdRemoved = removed;
            if (removed)
            {
                DestroyBanner();
            }
        }
    }

    public interface IAdAdapter
    {
        void Initialize(AdConfig config);

        void LoadBanner(string adUnitId);
        void ShowBanner(string adUnitId);
        void HideBanner();
        void DestroyBanner();

        void LoadInterstitial(string adUnitId);
        bool ShowInterstitial(string adUnitId);

        void LoadRewardedAd(string adUnitId);
        bool ShowRewardedAd(string adUnitId, Action<string> onRewarded, Action onFailed);

        event Action<string> OnAdLoaded;
        event Action<string, string> OnAdFailedToLoad;
        event Action<string> OnAdOpened;
        event Action<string> OnAdClosed;
        event Action<string, string> OnAdRewarded;
        event Action<string> OnAdImpressionRecorded;
    }
}