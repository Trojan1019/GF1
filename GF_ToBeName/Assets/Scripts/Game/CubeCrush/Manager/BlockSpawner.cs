using System.Collections.Generic;
using UnityEngine;
using NewSideGame;

namespace NewSideGame
{
    public class BlockSpawner : MonoSingleton<BlockSpawner>
    {
        [Header("Runtime State")] public List<BlockShape> currentShapes = new List<BlockShape>();
        public List<CubeCrushGoalItemType> currentShapeItems = new List<CubeCrushGoalItemType>();

        [Header("Bottom Slots")]
        // 底部同时展示的槽位数
        public int spawnSlots = 3;

        [Header("Stage Spawn Sequence")]
        // spawnSequence 作为“优先顺序前缀”：
        // 只要 spawnCursor 指向的元素仍在 spawnSequence 内，则按顺序生成；
        // 当 spawnSequence 用完后，后续生成仍然随机（符合你的需求）。
        private readonly List<BlockShape> _spawnSequence = new List<BlockShape>();

        // 当 GameMain.availableShapes 为空/全为 null 时，Stage 随机补充可以从 spawnSequence 兜底取值
        private readonly List<BlockShape> _randomFallbackPool = new List<BlockShape>();
        private bool _useSpawnSequence;
        private int _spawnCursor; // 下一次 SpawnBlocks 将从 spawnCursor 开始取 spawnSequence
        private readonly List<CubeCrushGoalItemType> _allowedGoalItems = new List<CubeCrushGoalItemType>();
        private float _itemAttachProbability;

        public int SpawnCursor => _spawnCursor;

        // 避免随机补充导致“整批都死局”，所以尝试重抽几次
        private const int MaxRerollAttempts = 25;

        public void ConfigureClassicSpawn()
        {
            _useSpawnSequence = false;
            _spawnSequence.Clear();
            _randomFallbackPool.Clear();
            _allowedGoalItems.Clear();
            _itemAttachProbability = 0f;
            _spawnCursor = 0;
        }

        public void ConfigureStageSpawn(List<BlockShape> spawnSequence, int spawnCursorStart = 0,
            List<CubeCrushGoalRequirement> goalRequirements = null, float itemAttachProbability = 0f)
        {
            _useSpawnSequence = true;
            _spawnSequence.Clear();
            _randomFallbackPool.Clear();
            _allowedGoalItems.Clear();

            if (spawnSequence != null)
            {
                _spawnSequence.AddRange(spawnSequence);

                // 构建兜底随机池：从 spawnSequence 中收集所有非空形状
                foreach (var s in spawnSequence)
                {
                    if (s == null) continue;
                    if (!_randomFallbackPool.Contains(s))
                        _randomFallbackPool.Add(s);
                }
            }

            if (goalRequirements != null)
            {
                for (int i = 0; i < goalRequirements.Count; i++)
                {
                    var req = goalRequirements[i];
                    if (req == null) continue;
                    if (req.itemType == CubeCrushGoalItemType.None) continue;
                    if (req.requiredCount <= 0) continue;
                    if (!_allowedGoalItems.Contains(req.itemType))
                        _allowedGoalItems.Add(req.itemType);
                }
            }

            _itemAttachProbability = Mathf.Clamp01(itemAttachProbability);
            _spawnCursor = Mathf.Max(0, spawnCursorStart);
        }

        // 用于“从存档恢复时”：恢复当前底部那一批方块 + spawnCursor（下一批从哪一项开始）
        public void RestoreStageSpawn(
            List<BlockShape> spawnSequence,
            int spawnCursorStart,
            List<BlockShape> restoredCurrentVisibleShapes,
            List<CubeCrushGoalItemType> restoredCurrentVisibleItems,
            bool notifyUI = true,
            bool playSpawnSound = false)
        {
            ConfigureStageSpawn(spawnSequence, spawnCursorStart);

            currentShapes.Clear();
            currentShapeItems.Clear();
            if (restoredCurrentVisibleShapes != null)
            {
                currentShapes.AddRange(restoredCurrentVisibleShapes);
            }

            if (restoredCurrentVisibleItems != null)
            {
                currentShapeItems.AddRange(restoredCurrentVisibleItems);
            }

            // 保证 currentShapes.Count == spawnSlots，方便 UI 按槽位计算
            while (currentShapes.Count < spawnSlots) currentShapes.Add(null);
            if (currentShapes.Count > spawnSlots)
                currentShapes.RemoveRange(spawnSlots, currentShapes.Count - spawnSlots);
            while (currentShapeItems.Count < spawnSlots) currentShapeItems.Add(CubeCrushGoalItemType.None);
            if (currentShapeItems.Count > spawnSlots)
                currentShapeItems.RemoveRange(spawnSlots, currentShapeItems.Count - spawnSlots);

            if (notifyUI) NotifySpawnUpdate(playSpawnSound);
        }

        public void SpawnBlocks()
        {
            currentShapes.Clear();
            currentShapeItems.Clear();

            if (_useSpawnSequence)
            {
                bool[] rerollableSlots = new bool[spawnSlots];

                for (int i = 0; i < spawnSlots; i++)
                {
                    int seqIndex = _spawnCursor + i;
                    BlockShape shape = null;
                    bool fixedNonNull = false;

                    // 先按顺序用完 spawnSequence
                    if (seqIndex >= 0 && _spawnSequence.Count > 0 && seqIndex < _spawnSequence.Count)
                    {
                        shape = _spawnSequence[seqIndex];
                        fixedNonNull = shape != null;
                    }

                    // spawnSequence 用完或该位为空：后续随机
                    if (shape == null)
                    {
                        shape = GetRandomAvailableShapeOrNull();
                    }

                    currentShapes.Add(shape);
                    currentShapeItems.Add(GenerateRandomGoalItem());

                    // 仅允许重抽“非固定前缀”的槽位（前缀非空就尽量保持）
                    rerollableSlots[i] = !fixedNonNull;
                }

                _spawnCursor += spawnSlots;
                EnsureAtLeastOneValidPlacement(rerollableSlots);
            }
            else
            {
                // Classic: 随机从可用形状取
                bool[] rerollableSlots = new bool[spawnSlots];
                for (int i = 0; i < spawnSlots; i++) rerollableSlots[i] = true;

                for (int i = 0; i < spawnSlots; i++)
                {
                    currentShapes.Add(GetRandomAvailableShapeOrNull());
                    currentShapeItems.Add(GenerateRandomGoalItem());
                }

                EnsureAtLeastOneValidPlacement(rerollableSlots);
            }

            NotifySpawnUpdate();
        }

        private void EnsureAtLeastOneValidPlacement(bool[] rerollableSlots)
        {
            // 防御：网格未初始化时不做过滤
            if (GridManager.Instance == null || GridManager.Instance.grid == null) return;

            if (IsAnyShapePlaceable(currentShapes)) return;

            int attempts = 0;
            while (attempts < MaxRerollAttempts)
            {
                attempts++;

                for (int i = 0; i < currentShapes.Count && i < spawnSlots; i++)
                {
                    if (rerollableSlots == null || i >= rerollableSlots.Length || !rerollableSlots[i])
                        continue;

                    currentShapes[i] = GetRandomAvailableShapeOrNull();
                    currentShapeItems[i] = GenerateRandomGoalItem();
                }

                if (IsAnyShapePlaceable(currentShapes)) return;
            }

            // 如果仍不满足，就放宽限制：允许整批槽位都重抽（以满足“至少有一块能落子”体验）。
            attempts = 0;
            while (attempts < MaxRerollAttempts)
            {
                attempts++;

                for (int i = 0; i < currentShapes.Count && i < spawnSlots; i++)
                {
                    currentShapes[i] = GetRandomAvailableShapeOrNull();
                    currentShapeItems[i] = GenerateRandomGoalItem();
                }

                if (IsAnyShapePlaceable(currentShapes)) return;
            }

            // 如果仍不满足，就保留最后一次抽取结果；
            // 后续 GameOver 逻辑会兜底触发失败流程。
        }

        private bool IsAnyShapePlaceable(List<BlockShape> shapes)
        {
            if (shapes == null || shapes.Count == 0) return false;
            if (GridManager.Instance == null) return false;
            if (GridManager.Instance.grid == null) return false;

            var grid = GridManager.Instance;

            for (int i = 0; i < shapes.Count; i++)
            {
                var shape = shapes[i];
                if (shape == null) continue;

                for (int x = 0; x < grid.cols; x++)
                {
                    for (int y = 0; y < grid.rows; y++)
                    {
                        if (grid.CanPlace(shape, new Vector2Int(x, y)))
                            return true;
                    }
                }
            }

            return false;
        }

        private BlockShape GetRandomAvailableShapeOrNull()
        {
            // 1) 优先用 GameMain.availableShapes（全局可用形状池）
            if (GameMain.Instance != null && GameMain.Instance.availableShapes != null)
            {
                // 过滤掉空引用，避免 Random.Range 的候选里出现 null
                // 注：这里为可读性直接生成列表；如果你后面性能要优化，再缓存非空列表。
                List<BlockShape> nonNull = new List<BlockShape>();
                foreach (var s in GameMain.Instance.availableShapes)
                {
                    if (s != null) nonNull.Add(s);
                }

                if (nonNull.Count > 0)
                    return nonNull[Random.Range(0, nonNull.Count)];
            }

            // 2) 如果全局池取不到（空/全为 null），stage 随机兜底从 spawnSequence 中取
            if (_randomFallbackPool.Count > 0)
            {
                return _randomFallbackPool[Random.Range(0, _randomFallbackPool.Count)];
            }

            return null;
        }

        public bool UseBlock(int index)
        {
            if (index < 0 || index >= currentShapes.Count)
            {
                return false;
            }

            currentShapes[index] = null; // Mark as used
            if (index < currentShapeItems.Count)
            {
                currentShapeItems[index] = CubeCrushGoalItemType.None;
            }

            NotifySpawnUpdate(false);
            return true;
        }

        public CubeCrushGoalItemType GetItemAt(int index)
        {
            if (index < 0 || index >= currentShapeItems.Count) return CubeCrushGoalItemType.None;
            return currentShapeItems[index];
        }

        public bool AreAllBlocksUsed()
        {
            foreach (var shape in currentShapes)
            {
                if (shape != null) return false;
            }

            return true;
        }

        private void NotifySpawnUpdate(bool spawn = true)
        {
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushSpawnUpdated, spawn);
        }

        private CubeCrushGoalItemType GenerateRandomGoalItem()
        {
            if (_allowedGoalItems.Count == 0) return CubeCrushGoalItemType.None;
            if (Random.value > _itemAttachProbability) return CubeCrushGoalItemType.None;
            return _allowedGoalItems[Random.Range(0, _allowedGoalItems.Count)];
        }
    }
}