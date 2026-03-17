using System.Collections.Generic;
//------------------------------------------------------------
// File : GameManager.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using UnityEngine;

namespace NewSideGame
{
    public class GameManager : MonoSingleton<GameManager>
    {
        public long pastTimeMillTime;
        public bool useDebugTime;
        private float _timeTick = 0f;

        private void Awake()
        {
            pastTimeMillTime = TimeUtils.GetTimeStampOfMilliseconds();
            var exceptionHelper = gameObject.GetOrAddComponent<ExceptionHelper>();
        }

        public int GetTimeStampOfSeconds()
        {
            return (int)(pastTimeMillTime / 1000);
        }

        private void Update()
        {
            pastTimeMillTime += (int)(Time.unscaledDeltaTime * 1000);

            if (_timeTick > 1f)
            {
                _timeTick = 0f;
                UpdatePerSecond();
            }
            else
            {
                _timeTick += Time.deltaTime;
            }

            if (!GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted, true))
                VibrateManager.Instance.Update();
        }

        private void UpdatePerSecond()
        {
            //if (LaunchPage.launchPage == null && ProxyManager.UserProxy.CanShowPack() && SceneHelper.IsHomeScene())
            //{
            //    GameFramework.UI.IUIGroup group = GameEntry.UI.GetUIGroup("Dialog");
            //    if (group.GetAllUIForms().Length < 1)
            //    {
            //        List<UIFormType> packList = GetPackList();
            //        GameEntry.UI.OpenUI(packList[ProxyManager.UserProxy.userModel.lastShowIndex % packList.Count]);
            //        ProxyManager.UserProxy.UpdateShowPackTime();
            //    }
            //}
        }

        public void StartGame()
        {
            GameEntry.UI.OpenUIForm(UIFormType.UITopCoverForm);
            GameEntry.Sound.PlayMusic(1001);
        }
    }
}