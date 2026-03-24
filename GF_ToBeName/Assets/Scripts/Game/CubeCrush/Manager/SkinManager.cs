using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class SkinManager : MonoBehaviour
    {
        public static SkinManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private SkinDatabase skinDatabase;
        [SerializeField] private string rewardedAdUnitId = Constant.Ad.tt_shop_icon_rewards;

        [Header("Scene References")]
        [SerializeField] private Image bgImage;
        [SerializeField] private Image boardImage;
        [SerializeField] private Transform skinPrefabRoot;

        private readonly Dictionary<int, int> _adProgress = new Dictionary<int, int>();
        private readonly Dictionary<int, bool> _unlocked = new Dictionary<int, bool>();
        private readonly List<GameObject> _spawnedSkinPrefabs = new List<GameObject>();

        private int _currentSkinId;
        private bool _initialized;

        private const string KeyCurrentSkinId = "Skin.CurrentSkinId";
        private const string KeyUnlockPrefix = "Skin.Unlock.";
        private const string KeyAdProgressPrefix = "Skin.AdProgress.";

        private void Awake()
        {
            Instance = this;
            InitializeIfNeeded();
        }

        public void InitializeIfNeeded()
        {
            if (_initialized) return;
            _initialized = true;

            if (skinDatabase == null || skinDatabase.skins == null || skinDatabase.skins.Count == 0)
            {
                Debug.LogWarning("[SkinManager] SkinDatabase is empty.");
                return;
            }

            for (int i = 0; i < skinDatabase.skins.Count; i++)
            {
                var cfg = skinDatabase.skins[i];
                if (cfg == null) continue;

                bool defaultUnlocked = (i == 0) || cfg.unlocked || cfg.adWatchRequiredCount <= 0;
                bool unlocked = PlayerPrefs.GetInt(KeyUnlockPrefix + cfg.skinId, defaultUnlocked ? 1 : 0) == 1;
                int adProgress = PlayerPrefs.GetInt(KeyAdProgressPrefix + cfg.skinId, 0);

                _unlocked[cfg.skinId] = unlocked;
                _adProgress[cfg.skinId] = Mathf.Max(0, adProgress);
            }

            int fallbackSkinId = skinDatabase.skins[0] != null ? skinDatabase.skins[0].skinId : 0;
            _currentSkinId = PlayerPrefs.GetInt(KeyCurrentSkinId, fallbackSkinId);
            if (!_unlocked.ContainsKey(_currentSkinId))
            {
                _currentSkinId = fallbackSkinId;
            }
            if (!_unlocked[_currentSkinId])
            {
                _unlocked[_currentSkinId] = true;
                SaveUnlock(_currentSkinId, true);
            }

            ApplySkin(_currentSkinId, true);
        }

        public List<SkinConfig> GetSkinConfigs()
        {
            return skinDatabase != null ? skinDatabase.skins : null;
        }

        public SkinConfig GetConfig(int skinId)
        {
            if (skinDatabase == null || skinDatabase.skins == null) return null;
            for (int i = 0; i < skinDatabase.skins.Count; i++)
            {
                var cfg = skinDatabase.skins[i];
                if (cfg == null) continue;
                if (cfg.skinId == skinId) return cfg;
            }
            return null;
        }

        public string GetDisplayName(SkinConfig cfg)
        {
            if (cfg == null) return string.Empty;
            if (!string.IsNullOrEmpty(cfg.skinNameKey))
            {
                string localized = GameEntry.Localization.GetString(cfg.skinNameKey);
                if (!string.IsNullOrEmpty(localized) && !localized.Contains("<NoKey>"))
                    return localized;
            }
            return cfg.skinName;
        }

        public int CurrentSkinId => _currentSkinId;

        public SkinState GetState(int skinId)
        {
            if (_currentSkinId == skinId) return SkinState.Using;
            if (IsUnlocked(skinId)) return SkinState.Unlocked;
            return SkinState.Locked;
        }

        public bool IsUnlocked(int skinId)
        {
            bool unlocked;
            if (_unlocked.TryGetValue(skinId, out unlocked))
                return unlocked;
            return false;
        }

        public int GetRemainingAdCount(int skinId)
        {
            var cfg = GetConfig(skinId);
            if (cfg == null) return 0;
            int watched = GetWatchedAdCount(skinId);
            return Mathf.Max(0, cfg.adWatchRequiredCount - watched);
        }

        public int GetWatchedAdCount(int skinId)
        {
            int count;
            if (_adProgress.TryGetValue(skinId, out count))
                return count;
            return 0;
        }

        public bool TrySelectSkin(int skinId)
        {
            EventManager.Instance.NotifyEvent(Constant.Event.SkinSelectedEvent, skinId);
            if (!IsUnlocked(skinId))
                return false;

            ApplySkin(skinId, false);
            return true;
        }

        public void TryUnlockByAd(int skinId, Action<bool> callback)
        {
            var cfg = GetConfig(skinId);
            if (cfg == null)
            {
                callback?.Invoke(false);
                return;
            }

            if (IsUnlocked(skinId))
            {
                callback?.Invoke(true);
                return;
            }

            bool shown = AdManager.Instance.ShowRewardedAd(
                rewardedAdUnitId,
                rewardType =>
                {
                    // 防作弊：仅在 SDK 真实回调 rewarded 时计数
                    if (string.IsNullOrEmpty(rewardType))
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    int watched = GetWatchedAdCount(skinId) + 1;
                    _adProgress[skinId] = watched;
                    SaveAdProgress(skinId, watched);

                    int remain = Mathf.Max(0, cfg.adWatchRequiredCount - watched);
                    if (remain <= 0)
                    {
                        _unlocked[skinId] = true;
                        SaveUnlock(skinId, true);
                        EventManager.Instance.NotifyEvent(Constant.Event.SkinUnlockedEvent, skinId, watched);
                    }

                    callback?.Invoke(remain <= 0);
                },
                () =>
                {
                    callback?.Invoke(false);
                });

            if (!shown)
            {
                callback?.Invoke(false);
            }
        }

        public void ApplySkin(int skinId, bool force)
        {
            if (!force && skinId == _currentSkinId) return;
            var cfg = GetConfig(skinId);
            if (cfg == null) return;

            int oldSkinId = _currentSkinId;
            _currentSkinId = skinId;
            PlayerPrefs.SetInt(KeyCurrentSkinId, _currentSkinId);
            PlayerPrefs.Save();

            if (bgImage != null) bgImage.sprite = cfg.bgSprite;
            if (boardImage != null) boardImage.sprite = cfg.boardSprite;

            ClearRelatedPrefabs();
            if (cfg.relatedPrefabResourcePaths != null)
            {
                for (int i = 0; i < cfg.relatedPrefabResourcePaths.Count; i++)
                {
                    string path = cfg.relatedPrefabResourcePaths[i];
                    if (string.IsNullOrEmpty(path)) continue;
                    var prefab = Resources.Load<GameObject>(path);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[SkinManager] Related prefab not found in Resources: {path}");
                        continue;
                    }

                    Transform parent = skinPrefabRoot != null ? skinPrefabRoot : transform;
                    var go = Instantiate(prefab, parent, false);
                    _spawnedSkinPrefabs.Add(go);
                }
            }

            EventManager.Instance.NotifyEvent(Constant.Event.SkinAppliedEvent, _currentSkinId, oldSkinId);
        }

        public bool TryUnlockFirstRewardSkin()
        {
            if (skinDatabase == null || skinDatabase.skins == null || skinDatabase.skins.Count < 2) return false;
            var rewardSkin = skinDatabase.skins[1];
            if (rewardSkin == null) return false;
            _unlocked[rewardSkin.skinId] = true;
            SaveUnlock(rewardSkin.skinId, true);
            EventManager.Instance.NotifyEvent(Constant.Event.SkinUnlockedEvent, rewardSkin.skinId,
                GetWatchedAdCount(rewardSkin.skinId));
            return true;
        }

        private void ClearRelatedPrefabs()
        {
            for (int i = 0; i < _spawnedSkinPrefabs.Count; i++)
            {
                if (_spawnedSkinPrefabs[i] != null)
                    Destroy(_spawnedSkinPrefabs[i]);
            }
            _spawnedSkinPrefabs.Clear();
        }

        private void SaveUnlock(int skinId, bool unlocked)
        {
            PlayerPrefs.SetInt(KeyUnlockPrefix + skinId, unlocked ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SaveAdProgress(int skinId, int watched)
        {
            PlayerPrefs.SetInt(KeyAdProgressPrefix + skinId, watched);
            PlayerPrefs.Save();
        }
    }
}

