using System.Collections.Generic;
using UnityEngine;
using CubeCrush.Data;

namespace NewSideGame
{
    public class BlockSpawner : MonoSingleton<BlockSpawner>
    {
        [Header("Runtime State")] public List<BlockShape> currentShapes = new List<BlockShape>();

        [Header("Bottom Slots")]
        // 底部同时展示的槽位数
        public int spawnSlots = 3;

        [Header("Stage Spawn Sequence")]
        // spawnSequence 作为“优先顺序前缀”：
        // 只要 spawnCursor 指向的元素仍在 spawnSequence 内，则按顺序生成；
        // 当 spawnSequence 用完后，后续生成仍然随机（符合你的需求）。

        private readonly List<BlockShape> _spawnSequence = new List<BlockShape>();
        private bool _useSpawnSequence;
        private int _spawnCursor; // 下一次 SpawnBlocks 将从 spawnCursor 开始取 spawnSequence

        public int SpawnCursor => _spawnCursor;

        public void ConfigureClassicSpawn()
        {
            _useSpawnSequence = false;
            _spawnSequence.Clear();
            _spawnCursor = 0;
        }

        public void ConfigureStageSpawn(List<BlockShape> spawnSequence, int spawnCursorStart = 0)
        {
            _useSpawnSequence = true;
            _spawnSequence.Clear();

            if (spawnSequence != null)
            {
                _spawnSequence.AddRange(spawnSequence);
            }

            _spawnCursor = Mathf.Max(0, spawnCursorStart);
        }

        // 用于“从存档恢复时”：恢复当前底部那一批方块 + spawnCursor（下一批从哪一项开始）
        public void RestoreStageSpawn(
            List<BlockShape> spawnSequence,
            int spawnCursorStart,
            List<BlockShape> restoredCurrentVisibleShapes,
            bool notifyUI = true,
            bool playSpawnSound = false)
        {
            ConfigureStageSpawn(spawnSequence, spawnCursorStart);

            currentShapes.Clear();
            if (restoredCurrentVisibleShapes != null)
            {
                currentShapes.AddRange(restoredCurrentVisibleShapes);
            }

            // 保证 currentShapes.Count == spawnSlots，方便 UI 按槽位计算
            while (currentShapes.Count < spawnSlots) currentShapes.Add(null);
            if (currentShapes.Count > spawnSlots) currentShapes.RemoveRange(spawnSlots, currentShapes.Count - spawnSlots);

            if (notifyUI) NotifySpawnUpdate(playSpawnSound);
        }

        public void SpawnBlocks()
        {
            currentShapes.Clear();

            if (_useSpawnSequence)
            {
                for (int i = 0; i < spawnSlots; i++)
                {
                    int seqIndex = _spawnCursor + i;
                    BlockShape shape = null;

                    // 先按顺序用完 spawnSequence
                    if (seqIndex >= 0 && _spawnSequence.Count > 0 && seqIndex < _spawnSequence.Count)
                    {
                        shape = _spawnSequence[seqIndex];
                    }

                    // spawnSequence 用完或该位为空：后续随机
                    if (shape == null)
                    {
                        shape = GetRandomAvailableShapeOrNull();
                    }

                    currentShapes.Add(shape);
                }

                _spawnCursor += spawnSlots;
            }
            else
            {
                // Classic: 随机从可用形状取
                int count = GameMain.Instance.availableShapes.Count;
                for (int i = 0; i < spawnSlots; i++)
                {
                    if (count > 0)
                    {
                        currentShapes.Add(GameMain.Instance.availableShapes[Random.Range(0, count)]);
                    }
                    else
                    {
                        currentShapes.Add(null);
                    }
                }
            }

            NotifySpawnUpdate(true);
        }

        private BlockShape GetRandomAvailableShapeOrNull()
        {
            if (GameMain.Instance == null || GameMain.Instance.availableShapes == null)
                return null;

            // 过滤掉空引用，避免 Random.Range 的候选里出现 null
            // 注：这里为可读性直接生成列表；如果你后面性能要优化，再缓存非空列表。
            List<BlockShape> nonNull = new List<BlockShape>();
            foreach (var s in GameMain.Instance.availableShapes)
            {
                if (s != null) nonNull.Add(s);
            }

            if (nonNull.Count == 0) return null;
            return nonNull[Random.Range(0, nonNull.Count)];
        }

        public bool UseBlock(int index)
        {
            if (index < 0 || index >= currentShapes.Count) return false;
            currentShapes[index] = null; // Mark as used
            NotifySpawnUpdate(false);
            return true;
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
    }
}