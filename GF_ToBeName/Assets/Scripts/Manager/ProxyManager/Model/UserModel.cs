//------------------------------------------------------------
// File : .cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityGameFramework.Runtime;
using Newtonsoft.Json;

namespace NewSideGame
{
    [System.Serializable]
    public class UserModel : BaseModel
    {
        public Dictionary<string, int> Items = new Dictionary<string, int>() { };
        public string countryCode;
        public long lastLoginTime;
        
        public List<string> transactionIDs = new List<string>();

        //弹窗flag
        public int dialogShowFlag;

        public Dictionary<int, int> shopFreeMap = new Dictionary<int, int>();

    }
}