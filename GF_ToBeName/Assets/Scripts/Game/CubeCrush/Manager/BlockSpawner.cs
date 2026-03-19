using UnityEngine;
using System.Collections.Generic;
using CubeCrush.Data;
using NewSideGame;
using NewSideGame;

namespace NewSideGame
{
    public class BlockSpawner : MonoSingleton<BlockSpawner>
    {
        [Header("Runtime State")] public List<BlockShape> currentShapes = new List<BlockShape>();

        // Max number of slots at bottom
        public int spawnSlots = 3;

        private void Awake()
        {
        }

        public void SpawnBlocks()
        {
            currentShapes.Clear();
            for (int i = 0; i < spawnSlots; i++)
            {
                int count = GameMain.Instance.availableShapes.Count;
                if (count > 0)
                {
                    currentShapes.Add(GameMain.Instance.availableShapes[Random.Range(0, count)]);
                }
            }

            NotifySpawnUpdate(true);
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
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushSpawnUpdated,spawn);
        }
    }
}