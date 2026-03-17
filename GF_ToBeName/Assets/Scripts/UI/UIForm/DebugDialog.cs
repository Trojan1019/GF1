using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

namespace NewSideGame
{
    public partial class DebugDialog : UGuiForm
    {
        public TextMeshProUGUI paramsInfo;
        public TMP_InputField orderInputField;
        public GameObject mainITF;
        public RectTransform[] fitters;
        public Button m_buttonTemp;
        public RectTransform m_buttonRoot;
        public SlotList<Button> m_allButton = new SlotList<Button>();
        private Dictionary<int, object> merge_sprites = new Dictionary<int, object>();

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_allButton.Init(m_buttonTemp, m_buttonRoot, true);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            ForceReflashLayout();
            ControlInterface(0);
            UpdateButtonGroup();
            RefreshInformation();
            ForceReflashLayout();

            orderInputField.text = string.Empty;

            merge_sprites.Clear();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            foreach (var item in merge_sprites)
            {
                GameEntry.Resource.UnloadAsset(item.Value);
            }
        }

        private void RefreshInformation()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string Theme = TestConfig.IsReviewVersion ? "A" : "B";
            sb.AppendFormat($"AB: {Theme}<br>");
            sb.AppendFormat($"Cur Time: {TimeUtils.GetTimeStamp()}<br>");
            sb.AppendFormat($"InterstitialInterval: {Constant.Setting.interstitialInterval}<br>");
            sb.AppendFormat($"InterstitialColdStart: {Constant.Setting.interstitialColdstart}<br>");
            sb.AppendFormat($"current country: {ProxyManager.UserProxy.userModel.countryCode}<br>");
            paramsInfo.text = sb.ToString();
        }


        #region 常用按钮功能区

        private void UpdateButtonGroup()
        {
            m_allButton.SetCount(ResidentCMD.Count);
            int index = 0;

            foreach (var item in ResidentCMD)
            {
                Button btn = m_allButton.GetSlot(index);
                btn.name = item.Key;
                TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = item.Key;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => { item.Value.Invoke(orderInputField.text); });
                index++;
            }
        }

        #endregion

        #region 命令功能区

        //自定义命令解析 ‘:’ 分隔
        public void ParseCommandWithParm()
        {
            string command = orderInputField.text;
            var commands = command.Split(':');
            string methodName = "";
            string param = "";
            string param2 = "";
            string param3 = "";

            if (commands.Length > 0)
            {
                methodName = commands[0];
            }

            if (commands.Length > 1)
                param = commands[1];
            if (commands.Length > 2)
                param2 = commands[2];
            if (commands.Length > 3)
                param3 = commands[3];

            Debug.Log($"输入指令==方法名{methodName}==参数{param}");
        }

        #endregion


        public void ForceReflashLayout()
        {
            int len = fitters.Length;
            for (int i = 0; i < len; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(fitters[i]);
            }
        }

        public void OnClickClose()
        {
            Close();
        }

        public void ClickToMainInterface()
        {
            ControlInterface(0);
        }

        private void ControlInterface(int i)
        {
            mainITF.SetActive(i == 0);
        }
    }

    public partial class DebugDialog
    {
        // 常用
        private readonly Dictionary<string, Action<string>> ResidentCMD = new Dictionary<string, Action<string>>()
        {
            {
                "Add coins:", (cmdString) =>
                {
                    if (!int.TryParse(cmdString, out int itemCount))
                    {
                        itemCount = 100;
                    }

                    ProxyManager.UserProxy.AddItem((int)ItemType.Coin, itemCount);
                }
            },
           
            {
                "Hide GM", (cmdString) =>
                {
                    UITopCoverForm.Instance.m_DebugBtn.gameObject.SetActive(false);
                    GameEntry.UI.CloseUIForm(UIFormType.DebugDialog);
                }
            }
        };
    }
}