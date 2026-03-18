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
            Close();
        }

        private void OnClick_BackBtn()
        {
            m_BackBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);

            Close();
            SceneHelper.LoadHomeScene();
        }

        private void OnClick_SoundBtn()
        {
            m_SoundImage.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);

            bool set = !GameEntry.Setting.GetBool(Constant.Setting.SoundMuted);
            GameEntry.Setting.SetBool(Constant.Setting.SoundMuted, set);
            GameEntry.Sound.Mute("Sound", set);
            m_SoundImage.sprite = set ? soundSprites[0] : soundSprites[1];
        }

        private void OnClick_MusicBtn()
        {
            m_MusicImage.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);

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

            m_SoundImage.sprite = GameEntry.Setting.GetBool(Constant.Setting.SoundMuted)
                ? soundSprites[0]
                : soundSprites[1];
            m_MusicImage.sprite = GameEntry.Setting.GetBool(Constant.Setting.MusicMuted)
                ? musicSprites[0]
                : musicSprites[1];
            m_VibrateImage.sprite = GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted)
                ? vibrateSprites[0]
                : vibrateSprites[1];

            UpdateDynamicSprites();
        }

        /// <summary>
        /// 根据游戏状态更新设置面板的图标
        /// </summary>
        private void UpdateDynamicSprites()
        {
            // 默认状态索引 0，可以根据当前的 Procedure 或游戏状态进行判断
            int stateIndex = 0;

            // 判断是否在游戏中
            if (GameEntry.Procedure.CurrentProcedure is ProcedureGamePlay)
            {
                // 可以进一步通过 GameLoopManager 判断是否暂停或结束
                // 这里假设 1 表示游戏中
                stateIndex = 1;
            }

            // 更新 Sprite，并实现平滑过渡
            if (musicSprites != null && musicSprites.Length > stateIndex && m_MusicImage != null)
            {
                m_MusicImage.sprite = musicSprites[stateIndex];
                m_MusicImage.DOFade(1f, 0.3f).From(0f);
            }

            if (soundSprites != null && soundSprites.Length > stateIndex && m_SoundImage != null)
            {
                m_SoundImage.sprite = soundSprites[stateIndex];
                m_SoundImage.DOFade(1f, 0.3f).From(0f);
            }

            if (vibrateSprites != null && vibrateSprites.Length > stateIndex && m_VibrateImage != null)
            {
                m_VibrateImage.sprite = vibrateSprites[stateIndex];
                m_VibrateImage.DOFade(1f, 0.3f).From(0f);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }
    }
}