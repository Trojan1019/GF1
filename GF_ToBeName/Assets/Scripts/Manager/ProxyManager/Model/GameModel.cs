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

        // Stage progress
        public bool stageModeEnabled;
        public int currentStageIndex; // 当前进行到第几关（1-based）
        public int highestStageCleared; // 玩家已通关到的最高关卡（1-based；未通关可为0）
        public int stageStartTotalScore; // 进入该 stage 时的累计总分，用于计算本关累计分
        public int spawnCursor; // 底部固定序列的游标（下一批 spawn 从哪一项开始）
        public bool isStageClearPending; // 是否处于“通关UI暂停状态”（需要恢复交互暂停）
    }
}