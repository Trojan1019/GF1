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
            EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushGameStart);
        }

        public void OnBlockPlaced(int spawnIndex, BlockShape shape, Vector2Int pos)
        {
            if (isGameOver) return;

            if (GridManager.Instance.PlaceBlock(shape, pos, out List<int> clearedRows, out List<int> clearedCols, out List<ClearedCellInfo> clearedCells))
            {
                BlockSpawner.Instance.UseBlock(spawnIndex);

                int clearedLines = clearedRows.Count + clearedCols.Count;
                
                // 连击计分机制：消除1行/列获得50分，同时消除2行/列获得100分，以此类推
                int points = shape.cells.Count;
                if (clearedLines > 0)
                {
                    points += clearedLines * 50; 
                }
                score += points;

                // 播放音效
                //GameEntry.Sound.PlaySound(placeSoundId);
                if (clearedLines > 0)
                {
                    GameEntry.Sound.PlaySound(Constant.SoundId.Remove);
                    // 触发消除动画和震动
                    EventManager.Instance.NotifyEvent(Constant.Event.CubeCrushLinesCleared, clearedCells);
#if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
#endif
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
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            
            // 更新并保存最高分
            if (ProxyManager.UserProxy != null)
            {
                ProxyManager.UserProxy.UpdateBestScore(score);
            }
            
            //GameEntry.Sound.PlaySound(Constant.SoundId.Click);
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            
            // 触发填满网格动画
            EventManager.Instance.NotifyEvent(Constant.Event.GameOverFillAnimation);
        }

        public void Restart()
        {
            StartGame();
        }
    }
}
