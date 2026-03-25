using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    [Serializable]
    public class CubeCrushPrefilledCell
    {
        public int x;
        public int y;
        public int prefilledBlockId;
        public Color color = Color.white;
    }

    [Serializable]
    public class CubeCrushInitialItemCell
    {
        public int x;
        public int y;
        public CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None;
        public int prefilledBlockId;
    }

    /// <summary>
    /// 单关卡数据：预置已填充格 + 本关通关分数 + 底部固定生成序列。
    /// 建议把所有关卡 ScriptableObject 放到 Assets/Game/.../Stages/ 下，
    /// 并由 StageDatabase（同目录）引用，保证能被打进 AssetBundle。
    /// </summary>
    [CreateAssetMenu(fileName = "Stage", menuName = "CubeCrush/Stage")]
    public class CubeCrushStage : ScriptableObject
    {
        public int stageIndex = 1; // 1-based
        public int targetScoreLocal = 1000;
        [Range(0f, 1f)] public float itemAttachProbability = 0.2f;

        public List<CubeCrushPrefilledCell> prefilledCells = new List<CubeCrushPrefilledCell>();
        public List<CubeCrushInitialItemCell> initialItems = new List<CubeCrushInitialItemCell>();

        // 底部方块固定序列：按顺序取，序列连续往后走（由 BlockSpawner 的 spawnCursor 决定）
        public List<NewSideGame.BlockShape> spawnSequence = new List<NewSideGame.BlockShape>();
        public List<CubeCrushGoalRequirement> goalRequirements = new List<CubeCrushGoalRequirement>();
    }
}

