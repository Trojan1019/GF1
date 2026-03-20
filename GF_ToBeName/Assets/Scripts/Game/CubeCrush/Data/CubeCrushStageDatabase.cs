using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    /// <summary>
    /// 关卡总表：运行时由 GameMain（StageSurvival）引用该对象，从而拿到所有 Stage 资源引用。
    /// </summary>
    [CreateAssetMenu(fileName = "StageDatabase", menuName = "CubeCrush/Stage Database")]
    public class CubeCrushStageDatabase : ScriptableObject
    {
        public List<CubeCrushStage> stages = new List<CubeCrushStage>();
    }
}

