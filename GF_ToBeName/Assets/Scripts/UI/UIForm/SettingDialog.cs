using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using GameFramework;

namespace NewSideGame
{
    public class SettingDialog : BaseDialog
    {
        #region SerializeField

        private TMPro.TextMeshProUGUI m_TitleText => GetRef<TMPro.TextMeshProUGUI>("TitleText");
        private TMPro.TextMeshProUGUI m_VersionText => GetRef<TMPro.TextMeshProUGUI>("VersionText");

        private UnityEngine.UI.Button m_BackBtn => GetRef<UnityEngine.UI.Button>("BackBtn");
        private UnityEngine.UI.Button m_CloseBtn => GetRef<UnityEngine.UI.Button>("CloseBtn");

        [Header("Dynamic Sprites")] public Sprite[] musicSprites; // 0:正常 1:暂停
        public Sprite[] soundSprites;
        public Sprite[] vibrateSprites;

        public UnityEngine.UI.Image m_MusicImage;
        public UnityEngine.UI.Image m_SoundImage;
        public UnityEngine.UI.Image m_VibrateImage;

        #endregion

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
            base.OnClose(isShutdown, userData);
        }
    }
}