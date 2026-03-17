using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace NewSideGame
{
    public class UIHomeForm : UGuiForm
    {
        [HideInInspector] public CasualCurrencyGroup m_CasualCurrency;
        [SerializeField] private Button GamePlayBtn;
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button settingBtn;
        [SerializeField] private TextMeshProUGUI score;

        private void OnEnable()
        {
            GamePlayBtn.onClick.AddListener(OnClick_PuzzleBtn);
            settingBtn.onClick.AddListener(OnClick_SettingBtn);

            EventManager.Instance.AddEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }

        private void OnDisable()
        {
            GamePlayBtn.onClick.RemoveAllListeners();
            settingBtn.onClick.RemoveAllListeners();

            EventManager.Instance.RemoveEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }


        private void OnClick_PuzzleBtn()
        {
            GamePlayBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            SceneHelper.LoadGameScene(() => { });
        }

        private void OnClick_SettingBtn()
        {
            settingBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.UI.OpenUIForm(UIFormType.SettingDialog);
        }

        private void RefreshHomePanel(params object[] objs)
        {
            UpdateScore();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            UpdateScore();
        }

        private void UpdateScore()
        {
            if (score != null)
            {
                // 获取并显示最高分
                int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
                score.text = string.Format("BestScore:{0:N0}", bestScore); // 使用千位分隔符格式化
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }
    }
}