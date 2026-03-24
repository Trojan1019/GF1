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
        [SerializeField] private Button stageSurvivalBtn; // 关卡生存（Stage Survival）按钮
        [SerializeField] private Button continueBtn; // 继续游戏按钮
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button settingBtn;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI stageProgressText; // 可选：显示已通关最高关卡

        private void OnEnable()
        {
            if (continueBtn != null) continueBtn.onClick.AddListener(OnClick_ContinueBtn);
            if (newGameBtn != null) newGameBtn.onClick.AddListener(OnClick_NewGameBtn);
            if (stageSurvivalBtn != null) stageSurvivalBtn.onClick.AddListener(OnClick_StageSurvivalBtn);
            if (settingBtn != null) settingBtn.onClick.AddListener(OnClick_SettingBtn);

            EventManager.Instance.AddEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }

        private void OnDisable()
        {
            if (continueBtn != null) continueBtn.onClick.RemoveAllListeners();
            if (newGameBtn != null) newGameBtn.onClick.RemoveAllListeners();
            if (stageSurvivalBtn != null) stageSurvivalBtn.onClick.RemoveAllListeners();
            if (settingBtn != null) settingBtn.onClick.RemoveAllListeners();

            EventManager.Instance.RemoveEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }

        private void OnClick_ContinueBtn()
        {
            continueBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            // 设置一个标志位告诉 GamePlay 场景去读取存档
            GameEntry.Setting.SetBool("LoadSavedGame", true);
            bool stageMode = ProxyManager.GameProxy.GameModel.stageModeEnabled;
            GameEntry.Setting.SetBool(Constant.Setting.ModeStageSurvivalKey, stageMode);
            SceneHelper.LoadGameScene(() => { });
        }

        private void OnClick_NewGameBtn()
        {
            newGameBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            if (ProxyManager.GameProxy != null && ProxyManager.GameProxy.GameModel.hasSavedGame)
            {
                // 有存档时，弹窗确认
                UGUIParams uguiParams = UGUIParams.Create().AddValue("Title", "10")
                    .AddValue("Message", "53")
                    .AddDelegage("OnClickConfirm", (s) =>
                    {
                        ProxyManager.GameProxy.ClearSavedGame();
                        GameEntry.Setting.SetBool("LoadSavedGame", false);
                        GameEntry.Setting.SetBool(Constant.Setting.ModeStageSurvivalKey, false);
                        SceneHelper.LoadGameScene(() => { });
                    });
                GameEntry.UI.OpenUIForm(UIFormType.AskDialog, uguiParams);
            }
            else
            {
                GameEntry.Setting.SetBool("LoadSavedGame", false);
                GameEntry.Setting.SetBool(Constant.Setting.ModeStageSurvivalKey, false);
                SceneHelper.LoadGameScene(() => { });
            }
        }

        private void OnClick_StageSurvivalBtn()
        {
            if (stageSurvivalBtn == null) return;

            stageSurvivalBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);

            // 进入关卡生存模式时，仍沿用“新游戏”的逻辑（如果有存档先清空确认）
            if (ProxyManager.GameProxy != null && ProxyManager.GameProxy.GameModel.hasSavedGame)
            {
                UGUIParams uguiParams = UGUIParams.Create().AddValue("Title", "10")
                    .AddValue("Message", "14")
                    .AddDelegage("OnClickConfirm", (s) =>
                    {
                        ProxyManager.GameProxy.ClearSavedGame();
                        GameEntry.Setting.SetBool("LoadSavedGame", false);
                        GameEntry.Setting.SetBool(Constant.Setting.ModeStageSurvivalKey, true);
                        SceneHelper.LoadGameScene(() => { });
                    });
                GameEntry.UI.OpenUIForm(UIFormType.AskDialog, uguiParams);
            }
            else
            {
                GameEntry.Setting.SetBool("LoadSavedGame", false);
                GameEntry.Setting.SetBool(Constant.Setting.ModeStageSurvivalKey, true);
                SceneHelper.LoadGameScene(() => { });
            }
        }

        private void OnClick_SettingBtn()
        {
            settingBtn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
            GameEntry.Sound.PlaySound(Constant.SoundId.Click);
            GameEntry.UI.OpenUIForm(UIFormType.SettingDialog);
        }

        private void RefreshHomePanel(params object[] objs)
        {
            UpdateScore();
        }

        private void OnLanguageChanged(object[] args)
        {
            UpdateScore();
            UpdateStageProgress();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 用 UI 生命周期注册语言刷新，避免被其他弹窗临时 disable 后丢失实时刷新。
            AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);

            UpdateScore();

            bool hasSave = ProxyManager.GameProxy != null && ProxyManager.GameProxy.GameModel.hasSavedGame;
            if (continueBtn != null) continueBtn.gameObject.SetActive(hasSave);

            UpdateStageProgress();
        }

        private void UpdateScore()
        {
            // 获取并显示最高分
            int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
            score.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), bestScore); // 使用千位分隔符格式化
        }

        private void UpdateStageProgress()
        {
            if (stageProgressText != null)
            {
                int stageCleared = ProxyManager.GameProxy != null
                    ? ProxyManager.GameProxy.GameModel.highestStageCleared
                    : 0;
                int nextStage = Mathf.Max(1, stageCleared + 1);
                stageProgressText.text = string.Format(GameEntry.Localization.GetString("32"), nextStage); // 第 {0} 关
            }

            // 也尝试更新关卡按钮内部的文字（如果你的 prefab 里 Button 下有 TextMeshProUGUI）
            if (stageSurvivalBtn != null)
            {
                var label = stageSurvivalBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    int stageCleared = ProxyManager.GameProxy != null
                        ? ProxyManager.GameProxy.GameModel.highestStageCleared
                        : 0;
                    int nextStage = Mathf.Max(1, stageCleared + 1);
                    label.text = string.Format(GameEntry.Localization.GetString("32"), nextStage);
                }
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