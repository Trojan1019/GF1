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

        private UnityEngine.UI.Button m_RestoreBtn => GetRef<UnityEngine.UI.Button>("RestoreBtn");
        private UnityEngine.UI.Button m_CloseBtn => GetRef<UnityEngine.UI.Button>("CloseBtn");

        private UIGray m_SoundBtnGray => GetRef<UIGray>("SoundBtn");
        private UIGray m_MusicBtnGray => GetRef<UIGray>("MusicBtn");
        private UIGray m_VibrateBtnGray => GetRef<UIGray>("VibrateBtn");

        #endregion

        #region SerializeField

        private void OnClick_CloseBtn()
        {
            Close();
        }

        private void OnClick_SoundBtn()
        {
            bool set = !GameEntry.Setting.GetBool(Constant.Setting.SoundMuted);
            GameEntry.Setting.SetBool(Constant.Setting.SoundMuted, set);
            GameEntry.Sound.Mute("Sound", set);

            m_SoundBtnGray.IsGray = set;
        }

        private void OnClick_MusicBtn()
        {
            bool set = !GameEntry.Setting.GetBool(Constant.Setting.MusicMuted);
            GameEntry.Setting.SetBool(Constant.Setting.MusicMuted, set);
            GameEntry.Sound.Mute("Music", GameEntry.Setting.GetBool(Constant.Setting.MusicMuted));

            m_MusicBtnGray.IsGray = set;
        }

        private void OnClick_VibrateBtn()
        {
            bool set = !GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted);
            GameEntry.Setting.SetBool(Constant.Setting.VibrationMuted, set);

            m_VibrateBtnGray.IsGray = set;
        }

        #endregion


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
//#if UNITY_IOS
//            m_RestoreBtn.gameObject.SetActive(true);
//#else
//            m_RestoreBtn.gameObject.SetActive(false);
//#endif
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_VersionText.text = GameEntry.Localization.GetString("67", Application.version);

            m_MusicBtnGray.IsGray = GameEntry.Setting.GetBool(Constant.Setting.MusicMuted);
            m_VibrateBtnGray.IsGray = GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted);
            m_SoundBtnGray.IsGray = GameEntry.Setting.GetBool(Constant.Setting.SoundMuted);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }
    }
}