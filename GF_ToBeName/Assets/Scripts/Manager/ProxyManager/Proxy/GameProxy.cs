using System;
using System.Collections;
using System.Collections.Generic;
using NewSideGame;
using Newtonsoft.Json;
using UnityEngine;

namespace NewSideGame
{
    public partial class GameProxy : BaseProxy<GameModel>
    {
        public GameModel GameModel
        {
            get { return (GameModel)this.Data; }
        }

        public GameProxy(string proxyName, object data = null) : base(proxyName, data)
        {
        }

        public override void OnRegister()
        {
        }

        public void SaveGameState(int score, int cols, int rows, int[,] grid, Color[,] colors, List<CubeCrush.Data.BlockShape> shapes)
        {
            GameModel.hasSavedGame = true;
            GameModel.score = score;
            GameModel.cols = cols;
            GameModel.rows = rows;
            
            int total = cols * rows;
            GameModel.gridData = new int[total];
            GameModel.gridColors = new string[total];
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int index = y * cols + x;
                    GameModel.gridData[index] = grid[x, y];
                    GameModel.gridColors[index] = ColorUtility.ToHtmlStringRGBA(colors[x, y]);
                }
            }

            GameModel.spawnShapes = new string[shapes.Count];
            for (int i = 0; i < shapes.Count; i++)
            {
                GameModel.spawnShapes[i] = shapes[i] != null ? shapes[i].name : "";
            }

            Save();
        }

        public void SaveStageState(
            int score,
            int cols,
            int rows,
            int[,] grid,
            Color[,] colors,
            List<CubeCrush.Data.BlockShape> currentVisibleSpawnShapes,
            bool stageModeEnabled,
            int currentStageIndex,
            int highestStageCleared,
            int stageStartTotalScore,
            int spawnCursor,
            bool isStageClearPending)
        {
            GameModel.hasSavedGame = true;
            GameModel.stageModeEnabled = stageModeEnabled;
            GameModel.currentStageIndex = currentStageIndex;
            GameModel.highestStageCleared = highestStageCleared;
            GameModel.stageStartTotalScore = stageStartTotalScore;
            GameModel.spawnCursor = spawnCursor;
            GameModel.isStageClearPending = isStageClearPending;

            GameModel.score = score;
            GameModel.cols = cols;
            GameModel.rows = rows;

            int total = cols * rows;
            GameModel.gridData = new int[total];
            GameModel.gridColors = new string[total];
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int index = y * cols + x;
                    GameModel.gridData[index] = grid[x, y];
                    GameModel.gridColors[index] = ColorUtility.ToHtmlStringRGBA(colors[x, y]);
                }
            }

            GameModel.spawnShapes = new string[currentVisibleSpawnShapes.Count];
            for (int i = 0; i < currentVisibleSpawnShapes.Count; i++)
            {
                var shape = currentVisibleSpawnShapes[i];
                GameModel.spawnShapes[i] = shape != null ? shape.name : "";
            }

            Save();
        }

        public void ClearSavedGame()
        {
            GameModel.hasSavedGame = false;
            GameModel.gridData = null;
            GameModel.gridColors = null;
            GameModel.spawnShapes = null;
            GameModel.stageModeEnabled = false;
            GameModel.currentStageIndex = 0;
            GameModel.stageStartTotalScore = 0;
            GameModel.spawnCursor = 0;
            GameModel.isStageClearPending = false;
            Save();
        }
    }
}