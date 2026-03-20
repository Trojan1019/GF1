using UnityEngine;

namespace NewSideGame
{
    public partial class GameMain
    {
        public enum CubeCrushMode
        {
            Classic,
            StageSurvival,
        }

        [Header("CubeCrush Mode")]
        [SerializeField] public CubeCrushMode mode = CubeCrushMode.Classic;

        [Header("Stage Survival (Stage Blueprint)")]
        // 当没有存档时，默认从 stageStartIndex 开始挑战
        [SerializeField] public int stageStartIndex = 1;

        [Header("Stage Database")]
        [Tooltip("关卡数据总表。请用编辑器创建/维护 StageDatabase，然后在这里拖拽引用。")]
        [SerializeField] public CubeCrushStageDatabase stageDatabase;

        // UIHomeForm 会在进入关卡场景前写入这个 bool，GameMain Awake 时读取以切换模式。
        private const string ModeStageSurvivalKey = "CubeCrush.Mode.StageSurvival";

        private void AwakeStageSurvivalModeFromSetting()
        {
            // 如果 Setting 不存在（极少见），就保持 Inspector 配置。
            try
            {
                bool stage = GameEntry.Setting.GetBool(ModeStageSurvivalKey, false);
                mode = stage ? CubeCrushMode.StageSurvival : CubeCrushMode.Classic;
            }
            catch
            {
                // 忽略设置读取失败，保留 Inspector 默认值。
            }
        }

        public bool IsStageSurvival => mode == CubeCrushMode.StageSurvival;

        public int StageCount => stageDatabase != null && stageDatabase.stages != null
            ? stageDatabase.stages.Count
            : 0;

        public CubeCrushStage GetStageConfig(int stageIndex)
        {
            if (stageDatabase == null || stageDatabase.stages == null || stageDatabase.stages.Count == 0)
                return null;

            CubeCrushStage last = null;
            foreach (var s in stageDatabase.stages)
            {
                if (s == null) continue;
                last = s;
                if (s.stageIndex == stageIndex) return s;
            }

            //兜底：找不到具体关卡时，取最后一个（避免空引用直接结束）
            return last;
        }

        public int GetStageTargetLocalScore(int stageIndex)
        {
            var cfg = GetStageConfig(stageIndex);
            return cfg != null ? cfg.targetScoreLocal : 0;
        }
    }
}

