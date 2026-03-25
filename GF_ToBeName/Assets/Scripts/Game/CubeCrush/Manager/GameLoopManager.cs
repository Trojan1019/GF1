using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NewSideGame
{
    public class GameLoopManager : MonoSingleton<GameLoopManager>
    {
        public int score;
        public bool isGameOver { get; private set; }

        private int stageIndex = 1; // 1-based
        private int stageStartTotalScore = 0; // 进入当前关时的累计总分
        private int stageTargetLocalScore = 0; // 本关目标分（本关累计分）
        private int highestStageCleared = 0;
        private readonly List<CubeCrushGoalProgress> stageGoals = new List<CubeCrushGoalProgress>();

        private bool isStageClearPending = false; // 通关 UI 暂停状态：禁止交互
        private bool hasUsedReviveAd = false;
        private int comboCount;
        private int comboBreakStepsLeft;
        private float comboPitchCurrent = 1f;
        private Coroutine comboShowCoroutine;
        private const int ComboBreakStepWindow = 2;
        private const float ComboDisplayDelaySeconds = 0.2f;

        public int StageIndex => stageIndex;
        public int StageStartScore => stageStartTotalScore; // 兼容旧 UI 字段名
        public int StageTargetDelta => stageTargetLocalScore; // 兼容旧 UI 字段名
        public int MaxStageReached => highestStageCleared;
        public bool IsStageClearPending => isStageClearPending;
        public List<CubeCrushGoalProgress> StageGoals => stageGoals;

        // 用于“保存节流”，避免每次落子都触发 Setting.Save()
        private float _lastSaveTime;
        private const float SaveIntervalSeconds = 2.0f;

        public void StartGame()
        {
            bool loadSaved = GameEntry.Setting.GetBool("LoadSavedGame", false);
            StartGame(loadSaved);
        }

        public void StartGame(bool loadSavedGame)
        {
            score = 0;
            isGameOver = false;
            isStageClearPending = false;
            hasUsedReviveAd = false;
            comboCount = 0;
            comboBreakStepsLeft = 0;
            comboPitchCurrent = 1f;
            if (comboShowCoroutine != null)
            {
                StopCoroutine(comboShowCoroutine);
                comboShowCoroutine = null;
            }

            if (GameMain.Instance != null && GameMain.Instance.IsStageSurvival)
            {
                StartStageMode(loadSavedGame);
            }
            else
            {
                StartClassicMode();
            }
        }

        private void StartClassicMode()
        {
            var model = ProxyManager.GameProxy != null ? ProxyManager.GameProxy.GameModel : null;
            highestStageCleared = model != null ? Mathf.Max(0, model.highestStageCleared) : 0;
            stageIndex = 1;
            stageStartTotalScore = 0;
            stageTargetLocalScore = 0;
            stageGoals.Clear();

            BlockSpawner.Instance.ConfigureClassicSpawn();
            GridManager.Instance.InitializeGrid();
            BlockSpawner.Instance.SpawnBlocks();

            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
        }

        private void StartStageMode(bool loadSavedGame)
        {
            var gameProxy = ProxyManager.GameProxy;
            var model = gameProxy != null ? gameProxy.GameModel : null;
            int savedHighest = model != null ? model.highestStageCleared : 0;

            // 1) 尝试从存档恢复
            if (loadSavedGame && model != null && model.hasSavedGame && model.stageModeEnabled)
            {
                score = model.score;
                stageIndex = Mathf.Max(1, model.currentStageIndex);
                highestStageCleared = Mathf.Max(0, model.highestStageCleared);
                stageStartTotalScore = model.stageStartTotalScore;
                stageTargetLocalScore = GameMain.Instance.GetStageTargetLocalScore(stageIndex);
                isStageClearPending = model.isStageClearPending;
                RestoreStageGoals(model);
                if (stageGoals.Count == 0)
                {
                    InitStageGoalsFromConfig(GameMain.Instance.GetStageConfig(stageIndex));
                }

                RestoreStageGridAndSpawns(model);

                EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
                EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
                EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

                if (isStageClearPending)
                {
                    GameEntry.UI.OpenUIForm(UIFormType.StageClearNextLevelDialog);
                }
                else
                {
                    OpenLevelTargetDialog(stageIndex);
                }

                return;
            }

            // 2) 新开挑战：从（最高通关关卡 + 1）开始
            highestStageCleared = Mathf.Max(0, savedHighest);
            score = 0;
            stageStartTotalScore = 0;
            stageIndex = Mathf.Max(GameMain.Instance.stageStartIndex, highestStageCleared + 1);

            // 兜底：如果 stage 列表为空，targetScoreLocal 会返回 0，此时会直接触发通关 UI
            stageTargetLocalScore = GameMain.Instance.GetStageTargetLocalScore(stageIndex);
            InitStageGoalsFromConfig(GameMain.Instance.GetStageConfig(stageIndex));

            GridManager.Instance.InitializeGrid();

            var stageCfg = GameMain.Instance.GetStageConfig(stageIndex);
            ValidateStageInitialItems(stageCfg);
            BlockSpawner.Instance.ConfigureStageSpawn(
                stageCfg != null ? stageCfg.spawnSequence : null,
                0,
                stageCfg != null ? stageCfg.goalRequirements : null,
                stageCfg != null ? stageCfg.itemAttachProbability : 0f);
            BlockSpawner.Instance.SpawnBlocks();

            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
            ApplyStagePrefill(stageCfg);
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

            OpenLevelTargetDialog(stageIndex);

            // 新开也先落一次存档，便于中途返回“继续”
            SaveStageState(isStageClearPending: false);
        }

        private void OpenLevelTargetDialog(int stageIdx)
        {
            if (isStageClearPending) return;
            GameEntry.UI.OpenUIForm(UIFormType.LevelTargetDialog,
                UGUIParams.Create().AddValue("StageIndex", stageIdx));
        }

        private void RestoreStageGridAndSpawns(GameModel model)
        {
            // Grid restore
            GridManager.Instance.cols = model.cols;
            GridManager.Instance.rows = model.rows;
            GridManager.Instance.InitializeGrid();

            int cols = GridManager.Instance.cols;
            int rows = GridManager.Instance.rows;
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int index = y * cols + x;
                    GridManager.Instance.grid[x, y] = model.gridData != null && index < model.gridData.Length ? model.gridData[index] : 0;

                    Color parsed = Color.clear;
                    if (model.gridColors != null && index < model.gridColors.Length)
                    {
                        var str = model.gridColors[index];
                        if (!string.IsNullOrEmpty(str))
                        {
                            ColorUtility.TryParseHtmlString("#" + str, out parsed);
                        }
                    }

                    GridManager.Instance.gridColors[x, y] = parsed;
                    GridManager.Instance.gridGoalItems[x, y] = model.gridGoalItems != null && index < model.gridGoalItems.Length ? model.gridGoalItems[index] : 0;
                }
            }

            // 兼容旧存档：如果没有保存 gridGoalItems，则尝试从 cfg.initialItems 重建。
            if (model.gridGoalItems == null)
            {
                var legacyStageCfg = GameMain.Instance.GetStageConfig(stageIndex);
                if (legacyStageCfg != null && legacyStageCfg.initialItems != null)
                {
                    for (int i = 0; i < legacyStageCfg.initialItems.Count; i++)
                    {
                        var item = legacyStageCfg.initialItems[i];
                        if (item == null) continue;
                        if (item.itemType == CubeCrushGoalItemType.None) continue;
                        if (item.x < 0 || item.x >= GridManager.Instance.cols) continue;
                        if (item.y < 0 || item.y >= GridManager.Instance.rows) continue;
                        if (GridManager.Instance.grid[item.x, item.y] != 1) continue;
                        GridManager.Instance.gridGoalItems[item.x, item.y] = (int)item.itemType;
                    }
                }
            }

            // Spawn restore
            var spawnStageCfg = GameMain.Instance.GetStageConfig(stageIndex);
            List<BlockShape> spawnSequence = spawnStageCfg != null ? spawnStageCfg.spawnSequence : null;

            var nameToShape = new Dictionary<string, BlockShape>();
            foreach (var s in GameMain.Instance.availableShapes)
            {
                if (s == null) continue;
                nameToShape[s.name] = s;
            }

            List<BlockShape> restoredCurrentShapes = new List<BlockShape>();
            List<CubeCrushGoalItemType> restoredCurrentItems = new List<CubeCrushGoalItemType>();
            if (model.spawnShapes != null)
            {
                for (int i = 0; i < model.spawnShapes.Length; i++)
                {
                    CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None;
                    if (model.spawnItemTypes != null && i < model.spawnItemTypes.Length)
                        itemType = (CubeCrushGoalItemType)model.spawnItemTypes[i];
                    restoredCurrentItems.Add(itemType);

                    string shapeName = model.spawnShapes[i];
                    if (string.IsNullOrEmpty(shapeName))
                    {
                        restoredCurrentShapes.Add(null);
                        continue;
                    }

                    if (nameToShape.TryGetValue(shapeName, out var shape))
                    {
                        restoredCurrentShapes.Add(shape);
                    }
                    else
                    {
                        restoredCurrentShapes.Add(null);
                    }
                }
            }

            BlockSpawner.Instance.RestoreStageSpawn(
                spawnSequence,
                model.spawnCursor,
                restoredCurrentShapes,
                restoredCurrentItems,
                notifyUI: false);
        }

        private void ApplyStagePrefill(CubeCrushStage cfg)
        {
            if (cfg == null) return;
            if (cfg.prefilledCells == null) return;

            foreach (var cell in cfg.prefilledCells)
            {
                if (cell == null) continue;
                if (cell.x < 0 || cell.x >= GridManager.Instance.cols) continue;
                if (cell.y < 0 || cell.y >= GridManager.Instance.rows) continue;

                GridManager.Instance.grid[cell.x, cell.y] = 1;
                GridManager.Instance.gridColors[cell.x, cell.y] = cell.color;
                GridManager.Instance.gridGoalItems[cell.x, cell.y] = (int)CubeCrushGoalItemType.None;
            }

            // 初始道具：仅允许放在预填方块格子上（编辑器已做校验，这里再防御一次）
            if (cfg.initialItems != null)
            {
                for (int i = 0; i < cfg.initialItems.Count; i++)
                {
                    var item = cfg.initialItems[i];
                    if (item == null) continue;
                    if (item.itemType == CubeCrushGoalItemType.None) continue;
                    if (item.x < 0 || item.x >= GridManager.Instance.cols) continue;
                    if (item.y < 0 || item.y >= GridManager.Instance.rows) continue;

                    // 必须是已预填的格子，否则忽略
                    if (GridManager.Instance.grid[item.x, item.y] != 1) continue;
                    GridManager.Instance.gridGoalItems[item.x, item.y] = (int)item.itemType;
                }
            }
        }

        public void OnBlockPlaced(int spawnIndex, BlockShape shape, Vector2Int pos)
        {
            if (isGameOver) return;
            if (isStageClearPending) return;

            CubeCrushGoalItemType placedItemType = BlockSpawner.Instance.GetItemAt(spawnIndex);
            if (GridManager.Instance.PlaceBlock(shape, pos, placedItemType, out List<int> clearedRows, out List<int> clearedCols,
                    out List<ClearedCellInfo> clearedCells))
            {
                // 放置完成后占用该槽位（道具不在这里“消耗/收集”，而是在棋盘格被消除时收集）
                BlockSpawner.Instance.UseBlock(spawnIndex);

                int clearedLines = clearedRows.Count + clearedCols.Count;

                int points = shape.cells.Count;
                if (clearedLines > 0)
                {
                    points += clearedLines * GameMain.perLineScore;
                }

                score += points;

                Vector3 placementWorldPos = GameMain.Instance.GridToWorld(pos);
                int placementScore = shape.cells.Count;

                if (clearedLines > 0)
                {
                    GameMain.Instance.ShowClearScore(placementScore, clearedLines, placementWorldPos);
                }
                else
                {
                    GameMain.Instance.ShowPlacementScore(placementScore, placementWorldPos);
                }

                if (clearedLines > 0)
                {
                    GameEntry.Sound.PlaySound(Constant.SoundId.Remove);
                    // 传入 rows/cols，便于按“从左到右/从上到下”逐格播放消除特效
                    EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushLinesCleared, clearedCells, clearedRows, clearedCols);
#if UNITY_ANDROID || UNITY_IOS
                    if (clearedLines >= 2 && !GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted))
                    {
                        Handheld.Vibrate();
                    }
#endif
                    // 在真正被清除的格子上收集道具（保持 icon 直到消除发生）
                    if (clearedCells != null)
                    {
                        for (int i = 0; i < clearedCells.Count; i++)
                        {
                            var info = clearedCells[i];
                            if (info == null) continue;
                            if (info.itemType == CubeCrushGoalItemType.None) continue;
                            CollectGoalItem(info.itemType);
                        }
                    }

                    comboCount = Mathf.Max(1, comboCount + 1);
                    comboBreakStepsLeft = ComboBreakStepWindow;
                    if (comboCount >= 2)
                    {
                        if (comboShowCoroutine != null)
                        {
                            StopCoroutine(comboShowCoroutine);
                        }
                        comboShowCoroutine = StartCoroutine(ShowComboAfterDelay(placementWorldPos, comboCount));
                    }
                }
                else if (comboCount > 0)
                {
                    comboBreakStepsLeft -= 1;
                    if (comboBreakStepsLeft <= 0)
                    {
                        comboCount = 0;
                        comboPitchCurrent = 1f;
                        if (comboShowCoroutine != null)
                        {
                            StopCoroutine(comboShowCoroutine);
                            comboShowCoroutine = null;
                        }
                    }
                }

                // Stage clear check（通关 UI 手动推进下一关）
                if (GameMain.Instance.IsStageSurvival)
                {
                    int stageLocalScore = score - stageStartTotalScore;
                    if (!isStageClearPending && stageLocalScore >= stageTargetLocalScore && AreAllGoalsCompleted())
                    {
                        TriggerStageClear();
                        return;
                    }
                }

                EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

                if (BlockSpawner.Instance.AreAllBlocksUsed())
                {
                    BlockSpawner.Instance.SpawnBlocks();
                }

                if (GridManager.Instance.CheckGameOver(BlockSpawner.Instance.currentShapes))
                {
                    GameOver();
                }
                else
                {
                    // 自动保存（支持“继续”）
                    TryAutoSave();
                }
            }
        }

        private void TriggerStageClear()
        {
            isStageClearPending = true;
            highestStageCleared = Mathf.Max(highestStageCleared, stageIndex);

            // 更新存档：处于“通关UI暂停状态”
            SaveStageState(isStageClearPending: true);

            if (GameMain.Instance != null)
            {
                GameMain.Instance.OnStageClearPause();
            }

            // 暂停后不再显示新提示（需要在 GameMain.Update 里配合判断）
            GameEntry.UI.OpenUIForm(UIFormType.StageClearNextLevelDialog);
        }

        private void TryAutoSave()
        {
            if (GameMain.Instance == null || !GameMain.Instance.IsStageSurvival) return;
            if (ProxyManager.GameProxy == null || ProxyManager.GameProxy.GameModel == null) return;

            if (Time.time - _lastSaveTime < SaveIntervalSeconds) return;
            _lastSaveTime = Time.time;

            SaveStageState(isStageClearPending: false);
        }

        private void SaveStageState(bool isStageClearPending)
        {
            if (ProxyManager.GameProxy == null) return;

            var model = ProxyManager.GameProxy.GameModel;
            if (model == null) return;

            var grid = GridManager.Instance.grid;
            var colors = GridManager.Instance.gridColors;

            // 直接保存当前底部那批可见方块（含 null，占位用于恢复“已用掉哪些槽位”）
            var shapes = BlockSpawner.Instance.currentShapes;

            ProxyManager.GameProxy.SaveStageState(
                score,
                GridManager.Instance.cols,
                GridManager.Instance.rows,
                grid,
                colors,
                shapes,
                stageModeEnabled: true,
                currentStageIndex: stageIndex,
                highestStageCleared: highestStageCleared,
                stageStartTotalScore: stageStartTotalScore,
                spawnCursor: BlockSpawner.Instance.SpawnCursor,
                isStageClearPending: isStageClearPending,
                goals: stageGoals,
                spawnItems: BlockSpawner.Instance.currentShapeItems);
        }

        public void NextStage()
        {
            if (!isStageClearPending) return;

            isStageClearPending = false;
            stageIndex += 1;
            stageStartTotalScore = score;
            stageTargetLocalScore = GameMain.Instance.GetStageTargetLocalScore(stageIndex);
            InitStageGoalsFromConfig(GameMain.Instance.GetStageConfig(stageIndex));

            var nextStageCfg = GameMain.Instance.GetStageConfig(stageIndex);
            ValidateStageInitialItems(nextStageCfg);

            // 重新初始化场面（清空网格 + 应用下一关预置 + 重置底部序列）
            GridManager.Instance.InitializeGrid();

            BlockSpawner.Instance.ConfigureStageSpawn(
                nextStageCfg != null ? nextStageCfg.spawnSequence : null,
                0,
                nextStageCfg != null ? nextStageCfg.goalRequirements : null,
                nextStageCfg != null ? nextStageCfg.itemAttachProbability : 0f);
            BlockSpawner.Instance.SpawnBlocks();

            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
            ApplyStagePrefill(nextStageCfg);
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

            OpenLevelTargetDialog(stageIndex);

            SaveStageState(isStageClearPending: false);
        }

        private void GameOver()
        {
            isGameOver = true;

            // 更新并保存最高分
            if (ProxyManager.UserProxy != null)
            {
                ProxyManager.UserProxy.UpdateBestScore(score);
            }

            // 阶段模式下：失败不保留“继续”，只保留最高通关关卡
            if (GameMain.Instance != null && GameMain.Instance.IsStageSurvival)
            {
                if (ProxyManager.GameProxy != null && ProxyManager.GameProxy.GameModel != null)
                {
                    ProxyManager.GameProxy.GameModel.highestStageCleared = highestStageCleared;
                    ProxyManager.GameProxy.Save();
                }
                ProxyManager.GameProxy.ClearSavedGame();
            }

#if UNITY_ANDROID || UNITY_IOS
            if (!GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted))
            {
                Handheld.Vibrate();
            }
#endif

            // 触发填满网格动画
            EventManager.Instance.NotifyEvent(Constant.Event.GameOverFillAnimation);
        }

        public void Restart()
        {
            // Restart 视为“重新开始挑战”，不从存档恢复
            isStageClearPending = false;
            isGameOver = false;
            hasUsedReviveAd = false;
            comboCount = 0;
            comboBreakStepsLeft = 0;
            comboPitchCurrent = 1f;
            StartGame(false);
        }

        public bool ReviveByAdClearBoard()
        {
            if (!isGameOver) return false;
            if (hasUsedReviveAd) return false;

            hasUsedReviveAd = true;
            isGameOver = false;

            GridManager.Instance.ClearAllBlocks(out List<ClearedCellInfo> clearedCells, out List<int> clearedRows, out List<int> clearedCols);
            if (clearedCells.Count > 0)
            {
                EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushLinesCleared, clearedCells, clearedRows, clearedCols);
                GameEntry.Sound.PlaySound(Constant.SoundId.Remove);
            }

            if (BlockSpawner.Instance.AreAllBlocksUsed())
            {
                BlockSpawner.Instance.SpawnBlocks();
            }

            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);

            return true;
        }

        private IEnumerator ShowComboAfterDelay(Vector3 worldPos, int comboAtSchedule)
        {
            yield return new WaitForSeconds(ComboDisplayDelaySeconds);
            if (comboCount != comboAtSchedule) yield break;

            float targetPitch = GetComboPitch(comboAtSchedule);
            comboPitchCurrent = Mathf.Lerp(comboPitchCurrent, targetPitch, 0.65f);
            GameEntry.Sound.PlaySoundByAssetID((int)ResourceIdentificationType.Bubble60007, pitch: comboPitchCurrent, volume: 1f);
            GameMain.Instance.ShowComboPopup(comboAtSchedule, worldPos);
            comboShowCoroutine = null;
        }

        private float GetComboPitch(int combo)
        {
            int c = Mathf.Clamp(combo, 1, 5);
            switch (c)
            {
                case 1: return 1.0f;
                case 2: return 1.2f;
                case 3: return 1.4f;
                case 4: return 1.6f;
                default: return 2.0f;
            }
        }

        private void InitStageGoalsFromConfig(CubeCrushStage cfg)
        {
            stageGoals.Clear();
            if (cfg == null || cfg.goalRequirements == null) return;
            for (int i = 0; i < cfg.goalRequirements.Count; i++)
            {
                var req = cfg.goalRequirements[i];
                if (req == null || req.itemType == CubeCrushGoalItemType.None || req.requiredCount <= 0) continue;
                stageGoals.Add(new CubeCrushGoalProgress
                {
                    itemType = req.itemType,
                    requiredCount = req.requiredCount,
                    remainingCount = req.requiredCount
                });
            }
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGoalProgressChanged);
        }

        private void RestoreStageGoals(GameModel model)
        {
            stageGoals.Clear();
            if (model == null || model.goalItemTypes == null || model.goalRequiredCounts == null || model.goalRemainingCounts == null)
                return;

            int count = Mathf.Min(model.goalItemTypes.Length, Mathf.Min(model.goalRequiredCounts.Length, model.goalRemainingCounts.Length));
            for (int i = 0; i < count; i++)
            {
                stageGoals.Add(new CubeCrushGoalProgress
                {
                    itemType = (CubeCrushGoalItemType)model.goalItemTypes[i],
                    requiredCount = model.goalRequiredCounts[i],
                    remainingCount = model.goalRemainingCounts[i]
                });
            }
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGoalProgressChanged);
        }

        private void CollectGoalItem(CubeCrushGoalItemType itemType)
        {
            if (itemType == CubeCrushGoalItemType.None) return;
            for (int i = 0; i < stageGoals.Count; i++)
            {
                if (stageGoals[i].itemType != itemType) continue;
                stageGoals[i].remainingCount = Mathf.Max(0, stageGoals[i].remainingCount - 1);
                EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGoalProgressChanged);
                GameEntry.Sound.PlaySound(Constant.SoundId.Collect);
                return;
            }
        }

        private bool AreAllGoalsCompleted()
        {
            if (stageGoals.Count == 0) return true;
            for (int i = 0; i < stageGoals.Count; i++)
            {
                if (stageGoals[i].remainingCount > 0) return false;
            }
            return true;
        }

        private void ValidateStageInitialItems(CubeCrushStage cfg)
        {
            if (cfg == null || cfg.initialItems == null || cfg.prefilledCells == null) return;
            HashSet<Vector2Int> prefilled = new HashSet<Vector2Int>();
            for (int i = 0; i < cfg.prefilledCells.Count; i++)
            {
                var c = cfg.prefilledCells[i];
                if (c == null) continue;
                prefilled.Add(new Vector2Int(c.x, c.y));
            }

            for (int i = 0; i < cfg.initialItems.Count; i++)
            {
                var item = cfg.initialItems[i];
                if (item == null) continue;
                bool validPos = prefilled.Contains(new Vector2Int(item.x, item.y));
                if (!validPos)
                {
                    Debug.LogWarning(
                        $"[StageValidation] Stage {cfg.stageIndex} initial item at ({item.x},{item.y}) has no prefilled block.");
                }
            }
        }
    }
}