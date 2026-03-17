using UnityEngine;
using System.Collections.Generic;
using BlockBlast.Data;
using NewSideGame;

namespace BlockBlast.Manager
{
    public class GameLoopManager : MonoSingleton<GameLoopManager>
    {
        public int score;
        private bool isGameOver;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            score = 0;
            isGameOver = false;

            GridManager.Instance.InitializeGrid();
            BlockSpawner.Instance.SpawnBlocks();

            EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);
            EventManager.Instance.NotifyEvent(Constant.Event.BlockBlastGameStart);
        }

        public void OnBlockPlaced(int spawnIndex, BlockShape shape, Vector2Int pos)
        {
            if (isGameOver) return;

            if (GridManager.Instance.PlaceBlock(shape, pos, out List<int> clearedRows, out List<int> clearedCols))
            {
                BlockSpawner.Instance.UseBlock(spawnIndex);

                int clearedCount = clearedRows.Count + clearedCols.Count;
                int points = shape.cells.Count + (clearedCount * 10) + (clearedCount > 1 ? clearedCount * 5 : 0);
                score += points;

                EventManager.Instance.NotifyEvent(Constant.Event.RefreshScore);

                if (BlockSpawner.Instance.AreAllBlocksUsed())
                {
                    BlockSpawner.Instance.SpawnBlocks();
                }

                if (GridManager.Instance.CheckGameOver(BlockSpawner.Instance.currentShapes))
                {
                    GameOver();
                }
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            EventManager.Instance.NotifyEvent(Constant.Event.GameOver);
        }

        public void Restart()
        {
            StartGame();
        }
    }
}
