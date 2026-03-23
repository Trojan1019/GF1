using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class LaunchPage : UGuiForm
    {
        public static LaunchPage launchPage;
        public const string AssetPath = "Assets/Game/Prefabs/UI/LaunchPage#10000.prefab";
        [SerializeField] private Slider m_Process;
        [SerializeField] private UnityEngine.RectTransform m_Lipstick;
        [SerializeField] private Image background;
        [HideInInspector] public bool m_IsFullProcess = false;
        public bool IsAnimationFinish => m_process >= maxProcess;

        public bool Ready => true;
        private float m_process = 0;
        private const float maxProcess = 1f;
        private const float stopProcess = 0.95f;
        private const float duration = 8f;
        private float timer = 0f;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            launchPage = this;
            m_Process.value = 0f;
            m_process = timer = 0f;
            m_IsFullProcess = false;
            background.sprite = Resources.Load<Sprite>("Prefabs/HomeBg");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            launchPage = null;
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_IsFullProcess)
            {
                if (m_process >= maxProcess)
                {
                    // Close();
                    return;
                }

                float delta = Time.deltaTime * 0.5f;
                SetProcess(delta);
            }
            else
            {
                timer += Time.deltaTime;
                // if (m_process > stopProcess)
                // {
                //     if (timer > duration && ProxyManager.Instance.IsInit)
                //     {
                //         return;
                //     }
                //
                //     return;
                // }

                float delta = (stopProcess / duration) * Time.deltaTime;
                SetProcess(delta);
            }
        }

        private void SetProcess(float delta)
        {
            m_process += delta;
            m_process = Mathf.Clamp01(m_process);
            m_Process.value = m_process;
            m_Lipstick.anchoredPosition =
                new Vector3(m_Process.value * m_Process.GetComponent<RectTransform>().sizeDelta.x - 8f, 15f);
        }

        public static void SetLaunchFullProcess()
        {
            if (launchPage != null)
            {
                launchPage.m_IsFullProcess = true;
            }
        }

        public static bool IsLaunchAnimationFinish()
        {
            if (launchPage != null)
            {
                return launchPage.IsAnimationFinish;
            }

            return false;
        }

        public static void CloseUI()
        {
            if (launchPage != null)
            {
                GameEntry.UI.CloseUIForm(UIFormType.LaunchPage);
                launchPage = null;
            }
        }
    }
}