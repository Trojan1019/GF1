using System;
using UnityEngine;
using System.Collections.Generic;
using UnityGameFramework.Runtime;
using Newtonsoft.Json;

namespace NewSideGame
{
    [System.Serializable]
    public class ActivityModel : BaseModel
    {
        public SignInModel signInModel;
        public TreasureFestModel TreasureFestModel;
        public SpinAdModel spinAdModel;
    }

    public class SignInModel
    {
        public bool Enabled;
        public int ActiveTime;
        public int CurrentDay;
        public int Round;
        public int[] ClaimMask;
        public int PurchaseMask;
        public int EndTime;
    }
    public class TreasureFestModel
    {
        public bool Enabled;
        public int TreasurePackIndex; //Treasure 礼包领取索引
        public int ActiveTime;
    }
    public class SpinAdModel
    {
        public bool Enabled;
        public int spinCoin;
        public int free;
        public int ad;
    }

}