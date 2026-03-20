using UnityEngine;
using System.Collections.Generic;
using CubeCrush.Data;
using NewSideGame;

namespace CubeCrush.Manager
{
    public class GameLoopManager : MonoSingleton<GameLoopManager>
    {
        public int score;
        public bool isGameOver { get; private set; }

        private int stageIndex = 1; // 1-based
        private int stageStartTotalScore = 0; // 进入当前关时的累计总分
        private int stageTargetLocalScore = 0; // 本关目标分（本关累计分）
        private int highestStageCleared = 0;

        private bool isStageClearPending = false; // 通关 UI 暂停状态：禁止交互

        public int StageIndex => stageIndex;
        public int StageStartScore => stageStartTotalScore; // 兼容旧 UI 字段名
        public int StageTargetDelta => stageTargetLocalScore; // 兼容旧 UI 字段名
        public int MaxStageReached => highestStageCleared;
        public bool IsStageClearPending => isStageClearPending;

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

                RestoreStageGridAndSpawns(model);

                EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
                EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
                EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

                if (isStageClearPending)
                {
                    GameEntry.UI.OpenUIForm(UIFormType.UISuccessForm);
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

            GridManager.Instance.InitializeGrid();

            var cfg = GameMain.Instance.GetStageConfig(stageIndex);
            BlockSpawner.Instance.ConfigureStageSpawn(cfg != null ? cfg.spawnSequence : null, 0);
            BlockSpawner.Instance.SpawnBlocks();

            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
            ApplyStagePrefill(cfg);
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

            // 新开也先落一次存档，便于中途返回“继续”
            SaveStageState(isStageClearPending: false);
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
                }
            }

            // Spawn restore
            var cfg = GameMain.Instance.GetStageConfig(stageIndex);
            List<BlockShape> spawnSequence = cfg != null ? cfg.spawnSequence : null;

            var nameToShape = new Dictionary<string, BlockShape>();
            foreach (var s in GameMain.Instance.availableShapes)
            {
                if (s == null) continue;
                nameToShape[s.name] = s;
            }

            List<BlockShape> restoredCurrentShapes = new List<BlockShape>();
            if (model.spawnShapes != null)
            {
                for (int i = 0; i < model.spawnShapes.Length; i++)
                {
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
            }
        }

        public void OnBlockPlaced(int spawnIndex, BlockShape shape, Vector2Int pos)
        {
            if (isGameOver) return;
            if (isStageClearPending) return;

            if (GridManager.Instance.PlaceBlock(shape, pos, out List<int> clearedRows, out List<int> clearedCols,
                    out List<ClearedCellInfo> clearedCells))
            {
                BlockSpawner.Instance.UseBlock(spawnIndex);

                int clearedLines = clearedRows.Count + clearedCols.Count;

                int points = shape.cells.Count;
                if (clearedLines > 0)
                {
                    points += clearedLines * GameMain.perLineScore;
                }

                score += points;

                if (GameMain.Instance != null)
                {
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
                }

                if (clearedLines > 0)
                {
                    GameEntry.Sound.PlaySound(Constant.SoundId.Remove);
                    EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushLinesCleared, clearedCells);
#if UNITY_ANDROID || UNITY_IOS
                    if (clearedLines >= 2 && !GameEntry.Setting.GetBool(Constant.Setting.VibrationMuted))
                    {
                        Handheld.Vibrate();
                    }
#endif
                }

                // Stage clear check（通关 UI 手动推进下一关）
                if (GameMain.Instance != null && GameMain.Instance.IsStageSurvival)
                {
                    int stageLocalScore = score - stageStartTotalScore;
                    if (!isStageClearPending && stageLocalScore >= stageTargetLocalScore)
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
            GameEntry.UI.OpenUIForm(UIFormType.UISuccessForm);
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
                isStageClearPending: isStageClearPending);
        }

        public void NextStage()
        {
            if (!isStageClearPending) return;

            isStageClearPending = false;
            stageIndex += 1;
            stageStartTotalScore = score;
            stageTargetLocalScore = GameMain.Instance.GetStageTargetLocalScore(stageIndex);

            var cfg = GameMain.Instance.GetStageConfig(stageIndex);

            // 重新初始化场面（清空网格 + 应用下一关预置 + 重置底部序列）
            GridManager.Instance.InitializeGrid();

            BlockSpawner.Instance.ConfigureStageSpawn(cfg != null ? cfg.spawnSequence : null, 0);
            BlockSpawner.Instance.SpawnBlocks();

            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
            ApplyStagePrefill(cfg);
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGridUpdated);
            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

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
            StartGame(false);
        }
    }
}