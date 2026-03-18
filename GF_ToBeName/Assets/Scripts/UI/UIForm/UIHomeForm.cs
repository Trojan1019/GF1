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
        [SerializeField] private Button newGameBtn; // 新游戏按钮
        [SerializeField] private Button continueBtn; // 继续游戏按钮
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button settingBtn;
        [SerializeField] private TextMeshProUGUI score;

        private void OnEnable()
        {
            if (continueBtn != null) continueBtn.onClick.AddListener(OnClick_ContinueBtn);
            if (newGameBtn != null) newGameBtn.onClick.AddListener(OnClick_NewGameBtn);
            if (settingBtn != null) settingBtn.onClick.AddListener(OnClick_SettingBtn);

            EventManager.Instance.AddEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }

        private void OnDisable()
        {
            if (continueBtn != null) continueBtn.onClick.RemoveAllListeners();
            if (newGameBtn != null) newGameBtn.onClick.RemoveAllListeners();
            if (settingBtn != null) settingBtn.onClick.RemoveAllListeners();

            EventManager.Instance.RemoveEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }

        private void OnClick_ContinueBtn()
        {
            // 设置一个标志位告诉 GamePlay 场景去读取存档
            GameEntry.Setting.SetBool("LoadSavedGame", true);
            SceneHelper.LoadGameScene(() => { });
        }

        private void OnClick_NewGameBtn()
        {
            if (ProxyManager.GameProxy != null && ProxyManager.GameProxy.GameModel.hasSavedGame)
            {
                // 有存档时，弹窗确认
                UGUIParams uguiParams = UGUIParams.Create().AddValue("Title", GameEntry.Localization.GetString("10"))
                    .AddValue("Message",
                        GameEntry.Localization.GetString("14"))
                    .AddDelegage("OnClickConfirm", (s) =>
                    {
                        ProxyManager.GameProxy.ClearSavedGame();
                        GameEntry.Setting.SetBool("LoadSavedGame", false);
                        SceneHelper.LoadGameScene(() => { });
                    });
                GameEntry.UI.OpenUIForm(UIFormType.AskDialog, uguiParams);
            }
            else
            {
                GameEntry.Setting.SetBool("LoadSavedGame", false);
                SceneHelper.LoadGameScene(() => { });
            }
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

            bool hasSave = ProxyManager.GameProxy != null && ProxyManager.GameProxy.GameModel.hasSavedGame;
            if (continueBtn != null) continueBtn.gameObject.SetActive(hasSave);
        }

        private void UpdateScore()
        {
            if (score != null)
            {
                // 获取并显示最高分
                int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
                score.text = string.Format($"{GameEntry.Localization.GetString("2")}:{0:N0}", bestScore); // 使用千位分隔符格式化
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