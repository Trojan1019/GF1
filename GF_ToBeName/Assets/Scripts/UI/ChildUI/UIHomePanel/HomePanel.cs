//------------------------------------------------------------
// File : HomePanel.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace NewSideGame
{
    public class HomePanel : BasePanel, IUGUIChild
    {
        [FormerlySerializedAs("puzzleBtn")] [SerializeField] private Button GamePlayBtn;
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button settingBtn;

        //[SerializeField] private TextMeshProUGUI level;

        private void OnEnable()
        {
            GamePlayBtn.onClick.AddListener(OnClick_PuzzleBtn);
            shopBtn.onClick.AddListener(OnClick_ShopBtn);
            settingBtn.onClick.AddListener(OnClick_SettingBtn);

            EventManager.Instance.AddEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }

        private void OnDisable()
        {
            GamePlayBtn.onClick.RemoveListener(OnClick_PuzzleBtn);
            shopBtn.onClick.RemoveListener(OnClick_ShopBtn);
            settingBtn.onClick.RemoveListener(OnClick_SettingBtn);

            EventManager.Instance.RemoveEventListener(Constant.Event.OnRefreshHomePanel, RefreshHomePanel);
        }


        private void OnClick_PuzzleBtn()
        {
            SceneHelper.LoadGameScene(() => { });
        }

        private void OnClick_ShopBtn()
        {
            GameEntry.UI.OpenUIForm(UIFormType.ShopDialog);
        }
        
        private void OnClick_SettingBtn()
        {
            GameEntry.UI.OpenUIForm(UIFormType.SettingDialog);
        }

        public void OnInit()
        {
        }

        public void OnOpen()
        {
            RefreshHomePanel();
        }


        private void RefreshHomePanel(params object[] args)
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void OnClose()
        {
        }

        public void OnSecondUpdate(float time)
        {
        }
    }
}