using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using GameFramework;
using GameFramework.Event;
using GameFramework.Localization;
using TMPro;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NewSideGame
{
    public class SettingDialog : BaseDialog
    {
        #region SerializeField

        private TMPro.TextMeshProUGUI m_TitleText => GetRef<TMPro.TextMeshProUGUI>("TitleText");
        private TMPro.TextMeshProUGUI m_VersionText => GetRef<TMPro.TextMeshProUGUI>("VersionText");

        private UnityEngine.UI.Button m_BackBtn => GetRef<UnityEngine.UI.Button>("BackBtn");
        private UnityEngine.UI.Button m_CloseBtn => GetRef<UnityEngine.UI.Button>("CloseBtn");
        private UnityEngine.UI.Button m_PrivacyPolicyBtn => GetRef<UnityEngine.UI.Button>("PrivacyPolicyBtn");
        private UnityEngine.UI.Button m_UserLicenseBtn => GetRef<UnityEngine.UI.Button>("UserLicenseBtn");
        private TMP_Dropdown m_LanguageDropdown => GetRef<TMP_Dropdown>("LanguageDropdown");

        [Header("Dynamic Sprites")] public Sprite[] musicSprites; // 0:正常 1:暂停
        public Sprite[] soundSprites;
        public Sprite[] vibrateSprites;

        public UnityEngine.UI.Image m_MusicImage;
        public UnityEngine.UI.Image m_SoundImage;
        public UnityEngine.UI.Image m_VibrateImage;

        #endregion


        private bool _isApplyingLanguage;
        private Language _pendingLanguage;
        private int _lastLanguageIndex = 0;

        #region SerializeField

        private void OnClick_CloseBtn()
        {
            m_CloseBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            Close();
        }

        private void OnClick_BackBtn()
        {
            m_BackBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            Close();
            SceneHelper.LoadHomeScene();
        }

        private void OnClick_SoundBtn()
        {
            m_SoundImage.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            bool set = !GameEntry.Setting.GetBool(Constant.Setting.SoundMuted);
            GameEntry.Setting.SetBool(Constant.Setting.SoundMuted, set);
            GameEntry.Sound.Mute("Sound", set);
            m_SoundImage.sprite = set ? soundSprites[0] : soundSprites[1];
        }

        private void OnClick_MusicBtn()
        {
            m_MusicImage.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            bool set = !GameEntry.Setting.GetBool(Constant.Setting.MusicMuted);
            GameEntry.Setting.SetBool(Constant.Setting.MusicMuted, set);
            GameEntry.Sound.Mute("Music", GameEntry.Setting.GetBool(Constant.Setting.MusicMuted));
            m_MusicImage.sprite = set ? musicSprites[0] : musicSprites[1];
        }

        private void OnClick_VibrateBtn()
        {
            m_VibrateImage.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);

            bool set = !GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted);
            GameEntry.Setting.SetBool(Constant.Setting.VibrationMuted, set);

            m_VibrateImage.sprite = set ? vibrateSprites[0] : vibrateSprites[1];
        }

        private void OnClick_PrivacyPolicyBtn()
        {
            if (m_PrivacyPolicyBtn != null)
            {
                m_PrivacyPolicyBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            }
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);
            
            Application.OpenURL(Constant.Config.URL_Privacy_Policy);
        }

        private void OnClick_UserLicenseBtn()
        {
            if (m_UserLicenseBtn != null)
            {
                m_UserLicenseBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            }
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);
            
            Application.OpenURL(Constant.Config.URL_User_License);
        }

        #endregion


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_VersionText.text = GameEntry.Localization.GetString("67", Application.version);
            m_BackBtn.gameObject.SetActive(SceneHelper.IsPuzzleScene());

            UpdateDynamicSprites();
            InitLanguageDropdown();
            RegisterLanguageReloadCallbacks();
        }

        /// <summary>
        /// 根据配置状态更新设置面板的图标
        /// </summary>
        private void UpdateDynamicSprites()
        {
            // 更新 Sound Sprite
            if (m_SoundImage != null && soundSprites != null && soundSprites.Length >= 2)
            {
                bool isSoundMuted = false;
                try
                {
                    isSoundMuted = GameEntry.Setting.GetBool(Constant.Setting.SoundMuted);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SettingDialog] Failed to get SoundMuted setting, using default (false). Error: {e.Message}");
                }
                m_SoundImage.sprite = isSoundMuted ? soundSprites[0] : soundSprites[1];
                m_SoundImage.DOFade(1f, 0.3f).From(0f).SetUpdate(true);
            }
            else
            {
                Debug.LogError("[SettingDialog] m_SoundImage or soundSprites is not properly configured.");
            }

            // 更新 Music Sprite
            if (m_MusicImage != null && musicSprites != null && musicSprites.Length >= 2)
            {
                bool isMusicMuted = false;
                try
                {
                    isMusicMuted = GameEntry.Setting.GetBool(Constant.Setting.MusicMuted);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SettingDialog] Failed to get MusicMuted setting, using default (false). Error: {e.Message}");
                }
                m_MusicImage.sprite = isMusicMuted ? musicSprites[0] : musicSprites[1];
                m_MusicImage.DOFade(1f, 0.3f).From(0f).SetUpdate(true);
            }

            // 更新 Vibrate Sprite
            if (m_VibrateImage != null && vibrateSprites != null && vibrateSprites.Length >= 2)
            {
                bool isVibrateMuted = false;
                try
                {
                    isVibrateMuted = GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SettingDialog] Failed to get VibrationMuted setting, using default (false). Error: {e.Message}");
                }
                m_VibrateImage.sprite = isVibrateMuted ? vibrateSprites[0] : vibrateSprites[1];
                m_VibrateImage.DOFade(1f, 0.3f).From(0f).SetUpdate(true);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            UnregisterLanguageReloadCallbacks();

            if (m_LanguageDropdown != null)
            {
                m_LanguageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownValueChanged);
            }

            base.OnClose(isShutdown, userData);
        }

        private void RegisterLanguageReloadCallbacks()
        {
            // 使用 GameFramework 事件系统监听字典加载结果（ProcedurePreload 同样如此）
            GameEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
        }

        private void UnregisterLanguageReloadCallbacks()
        {
            GameEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
        }

        private void InitLanguageDropdown()
        {
            if (m_LanguageDropdown == null) return;

            var supported = LocalizationLanguageHelper.SupportedLanguages;
            List<string> options = new List<string>(supported.Count);

            for (int i = 0; i < supported.Count; i++)
            {
                options.Add(supported[i].ToString());
            }

            m_LanguageDropdown.ClearOptions();
            m_LanguageDropdown.AddOptions(options);

            int currentIndex = 0;
            for (int i = 0; i < supported.Count; i++)
            {
                if (supported[i] == GameEntry.Localization.Language)
                {
                    currentIndex = i;
                    break;
                }
            }

            m_LanguageDropdown.SetValueWithoutNotify(currentIndex);
            _lastLanguageIndex = currentIndex;
            m_LanguageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownValueChanged);
            m_LanguageDropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
        }

        private void OnLanguageDropdownValueChanged(int index)
        {
            if (_isApplyingLanguage) return;

            var supported = LocalizationLanguageHelper.SupportedLanguages;
            if (index < 0 || index >= supported.Count) return;

            Language selected = supported[index];
            if (selected == GameEntry.Localization.Language) return;

            // 方案1：语言不实时生效，改为“需要重启”弹窗确认。
            _isApplyingLanguage = true;

            int oldIndex = _lastLanguageIndex;
            m_LanguageDropdown.SetValueWithoutNotify(index); // 保持用户选择态；如果拒绝会在 OnClickDeny 回滚

            string restartMessage = "Language change will take effect after restarting the game. Restart now?";

            UGUIParams uguiParams = UGUIParams.Create()
                .AddValue("Title", "10") // 通用“Confirm/确认”文本
                .AddValue("Message", restartMessage) // 这里允许纯文本（AskDialog 已支持兜底）
                .AddDelegage("OnClickConfirm", (s) =>
                {
                    GameEntry.Setting.SetString(Constant.Setting.Language, selected.ToString());
                    GameEntry.Setting.Save();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                })
                .AddDelegage("OnClickDeny", (s) =>
                {
                    if (m_LanguageDropdown != null)
                        m_LanguageDropdown.SetValueWithoutNotify(oldIndex);
                    _isApplyingLanguage = false;
                });

            GameEntry.UI.OpenUIForm(UIFormType.AskDialog, uguiParams);
        }
        
        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            var ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this) return;

            _isApplyingLanguage = false;
            if (m_VersionText != null)
                m_VersionText.text =$"Version：{ Application.version}";
            EventManager.Instance.NotifyEvent(Constant.Event.LanguageChangeSuccess);
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            var ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this) return;

            _pendingLanguage = Language.English;
            _isApplyingLanguage = false;

            string path = LocalizationLanguageHelper.GetAssetPath(Language.English);
            GameEntry.Localization.Language = Language.English;
            GameEntry.Localization.ReadData(path, this);
        }
    }
}