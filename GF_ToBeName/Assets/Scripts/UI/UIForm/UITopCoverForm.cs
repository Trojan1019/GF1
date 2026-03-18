using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace NewSideGame
{
    public class UITopCoverForm : UGuiForm
    {
        private UnityEngine.UI.VerticalLayoutGroup m_LeftBar => GetRef<UnityEngine.UI.VerticalLayoutGroup>("LeftBar");
        private UnityEngine.UI.VerticalLayoutGroup m_RightBar => GetRef<UnityEngine.UI.VerticalLayoutGroup>("RightBar");
        public SettingGMButton m_DebugBtn;

        private float lastestShowTime;

        private GameObject m_HomeRightBar;

        private static UITopCoverForm m_Instance;

        public static UITopCoverForm Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameEntry.UI.OpenUIForm(UIFormType.UITopCoverForm);
                }

                return m_Instance;
            }
            set { m_Instance = value; }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_Instance = this;
            m_DebugBtn.gameObject.SetActive(false);

#if UNITY_EDITOR
            m_DebugBtn.gameObject.SetActive(true);
#endif
        }

        public GameObject rectangle;

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            m_Instance = null;
        }
    }
}