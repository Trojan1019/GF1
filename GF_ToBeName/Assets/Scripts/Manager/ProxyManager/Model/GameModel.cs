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
    public class GameModel : BaseModel
    {
        public bool hasSavedGame;
        public int score;
        public int cols;
        public int rows;
        public int[] gridData;
        public string[] gridColors;
        public string[] spawnShapes;
    }
}