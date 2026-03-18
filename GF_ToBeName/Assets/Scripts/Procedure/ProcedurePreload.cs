using System;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace NewSideGame
{
    public class ProcedurePreload : ProcedureBase
    {
        private int _totalResourceCount;
        private int _currentResourceCount;
        private bool isInitializedTA = false;
        private bool isReadyPreload = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            SplashPage.OpenLaunchPage();

            GameEntry.Event.Subscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Subscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            _totalResourceCount = 0;
            _currentResourceCount = 0;

            //InitializeTA();

            LoadDataTable();

            LoadGlobalConfig();

            LoadLocalization();

            LoadResourceIdentification();

            LoadTMPSetting();

            InitializeProxy();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            GameEntry.Event.Unsubscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Unsubscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

//            // 初始化
// KBSDKManager.InitSDK(cloudMsg =>
// {
//     if (cloudMsg.Message.Notification != null)
//     {
//         KBSDK.KBLog.Log(GetType().ToString(), "Firebase Received a new message : "
//             + cloudMsg.Message.Notification.Title + ", " + cloudMsg.Message.Notification.Body);
//     }
// });
// KBSDKManager.Ads.setInitCallback((ok) =>
// {
//     if (ok)
//     {
//         // 开启全局广告加载优化
//         var failToast = LocalizationUtils.GetString(
//             143, "Video is currently unavailable. Please try again later.");
//         KBSDKManager.Ads.EnableWaitForRetry(true, 8f,
//                 showLoading: () => UIManager.ShowLoading(),
//                 closeLoading: () => UIManager.CloseLoading(),
//                 showFailToast: () => UIManager.Toast(failToast));

//         string countryCode = McSdk.GetSdkConfiguration().CountryCode;
//         if (!string.IsNullOrEmpty(countryCode))
//             ProxyManager.UserProxy.SetCountryCode(countryCode);
//     }
//     UnityEngine.Debug.Log("==> lyly KBSDKManager.Ads.setInitCallback ok : " + ok);
// });

// KBIAPManager.Instance.Initilize();
            //DateTimeHelper.Login();

            // #if UNITY_ANDROID
            //             NotificationManager.Instance.Init();
            // #endif
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_totalResourceCount != -1 && _currentResourceCount >= _totalResourceCount)
            {
                _totalResourceCount = -1;
                isReadyPreload = true;
            }

            if (isReadyPreload)
            {
                isReadyPreload = false;
                LaunchPage.SetLaunchFullProcess();
            }

            if (LaunchPage.IsLaunchAnimationFinish())
            {
                // SplashPage.CloseUI();
                procedureOwner.SetData<VarBoolean>(Constant.ProcedureData.ColdBoot, true);
                ChangeState<ProcedureMainGame>(procedureOwner);
            }
        }

        private void LoadGlobalConfig()
        {
            _totalResourceCount++;

            string globalConfigAssetPath = $"Assets/Game/Configs/GlobalConfig.csv";
            GameEntry.Config.ReadData(globalConfigAssetPath, this);
        }

        private void LoadDataTable()
        {
            // 输出结果
            foreach (DataTableType dataTableType in Enum.GetValues(typeof(DataTableType)))
            {
                string dataTableName = dataTableType.ToString();
                string dataTableAssetPath = $"Assets/Game/DataTables/{dataTableName}.bytes";

                HasAssetResult hasAssetResult = GameEntry.Resource.HasAsset(dataTableAssetPath);

                if (hasAssetResult != HasAssetResult.NotExist)
                {
                    _totalResourceCount++;
                    GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetPath, this);
                }
            }
        }

        private void LoadLocalization()
        {
            _totalResourceCount++;

            string locationAssetPath =
                $"Assets/Game/Localization/uiText_{GameEntry.Localization.Language.ToString()}.csv";
            GameEntry.Localization.ReadData(locationAssetPath, this);
        }

        private void LoadResourceIdentification()
        {
            _totalResourceCount++;
            ResourceIdentificationTool.Instance.InitResourceIdentificationInfos(() => { AddCurrentResourceCount(); });
        }


        private void LoadTMPSetting()
        {
            _totalResourceCount++;
            TMPHelper.LoadSetting(() => { AddCurrentResourceCount(); });
        }

        private void AddCurrentResourceCount(int count = 1)
        {
            _currentResourceCount += count;
        }

        private void OnLoadConfigSuccess(object sender, GameEventArgs e)
        {
            LoadConfigSuccessEventArgs ne = (LoadConfigSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            AddCurrentResourceCount();
        }

        private void OnLoadConfigFailure(object sender, GameEventArgs e)
        {
            LoadConfigFailureEventArgs ne = (LoadConfigFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load config '{0}' from '{1}' with error message '{2}'.", ne.ConfigAssetName,
                ne.ConfigAssetName, ne.ErrorMessage);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            AddCurrentResourceCount();
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName,
                ne.DataTableAssetName, ne.ErrorMessage);
        }

        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            AddCurrentResourceCount();
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName,
                ne.DictionaryAssetName, ne.ErrorMessage);
        }

        private void InitializeProxy()
        {
            _totalResourceCount++;
            ProxyManager.Instance.InitProxy(() => _totalResourceCount--);
        }
    }
}