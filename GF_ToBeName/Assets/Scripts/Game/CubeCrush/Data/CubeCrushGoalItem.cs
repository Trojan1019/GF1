using System;
using UnityEngine;

namespace NewSideGame
{
    public enum CubeCrushGoalItemType
    {
        None = 0,
        Glove = 1,
        Star = 2,
        Gem = 3,
    }

    [Serializable]
    public class CubeCrushGoalRequirement
    {
        public CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None;
        public int requiredCount = 1;
    }

    [Serializable]
    public class CubeCrushGoalProgress
    {
        public CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None;
        public int requiredCount;
        public int remainingCount;
    }
}
