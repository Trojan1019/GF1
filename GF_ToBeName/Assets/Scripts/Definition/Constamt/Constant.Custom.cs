using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public static partial class Constant
    {
        public static class Setting
        {
            #region Setting

            public const string Language = "Setting.Language";
            public const string MusicMuted = "Setting.MusicMuted";
            public const string SoundMuted = "Setting.SoundMuted";
            public const string VibrationMuted = "Setting.VibrationMuted";

            #endregion

            #region Proxy

            public const string UserProxy = "Proxy.UserProxy";
            public const string GameProxy = "Proxy.PuzzleProxy";
            public const string ActivityProxy = "Proxy.ActivityProxy";

            #endregion

            public const int interstitialColdstart = 120;
            public const int interstitialInterval = 120;
            public const int showPackInterval = 180;
        }

        public static class Scene
        {
            public static string Home = "Home";
            public static string Gameplay = "GamePlay";
        }

        public static class Event
        {
            public const int LanguageChangeSuccess = 1; //多语言切换成功
            public const int UserCurrencyChange = 2; //货币栏变动
            public const int OnChooseVipItem = 3; //切换选中的VipItem
            public const int OnRefreshShopDialog = 4; //刷新商店
            public const int OnRefreshHomePanel = 5; //刷新主页
            public const int RefreshScore = 6; //
            public const int GameOver = 7; //
            
            public const int CubeCrushGridUpdated = 101;
            public const int CubeCrushSpawnUpdated = 102;
            public const int CubeCrushGameStart = 103;
            public const int CubeCrushLinesCleared = 104;
            public const int GameOverFillAnimation = 105;
        }

        public static class Config
        {
            public const string URL_Privacy_Policy = "https://superlight-engine.com/privacy";
            public const string URL_User_License = "https://superlight-engine.com/userLicense";

            public const string DownloadCoinCost = "DownloadCoinCost"; //下载图片金币花费
            public const string PropCoinCost = "PropCoinCost"; //道具金币花费

            public const string SpecialLvCost = "SpecialLvCost"; //特殊关卡花费
            public const string VipLvCost = "VipLvCost"; //Vip关卡花费

            public const string BalloonCoin = "BalloonCoin";//漂浮气球金币

            public const string AdFbEnableTrack = "Ad.FB.EnableTrack";
        }

        public static class Layer
        {
            public const string DefaultLayerName = "Default";
            public static readonly int DefaultLayerId = LayerMask.NameToLayer(DefaultLayerName);

            public const string UILayerName = "UI";
            public static readonly int UILayerId = LayerMask.NameToLayer(UILayerName);
        }
    }
}