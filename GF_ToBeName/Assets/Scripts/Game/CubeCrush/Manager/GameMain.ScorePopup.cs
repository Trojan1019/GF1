using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public partial class GameMain
    {
        [Header("ScorePopUp配置")] public Transform popupParent;
        public const int perLineScore = 50;
        
        private List<ScorePopup> _activePopups = new List<ScorePopup>();
        public void ShowPlacementScore(int placementScore, Vector3 worldPosition)
        {
            ShowScorePopup(placementScore, worldPosition);
        }

        public void ShowClearScore(int placementScore, int linesCleared, Vector3 worldPosition)
        {
            int totalScore = placementScore + linesCleared * perLineScore;
            ShowScorePopup(totalScore, worldPosition);
        }

        private void ShowScorePopup(int score, Vector3 worldPosition)
        {
            ScorePopup popup = GameEntry.PoolManager.SpawnSync<ScorePopup>(31003);
            popup.transform.SetParent(popupParent);
            popup.Init(score, Vector3.zero);
            _activePopups.Add(popup);
            //_lastPopupX = popupPosition.x;
        }
        
        public void RemovePopup(ScorePopup popup)
        {
            if (_activePopups.Contains(popup))
            {
                _activePopups.Remove(popup);
            }
        }

        public void ClearAllPopups()
        {
            for (int i = _activePopups.Count - 1; i >= 0; i--)
            {
                if (_activePopups[i] != null)
                {
                    if (GameEntry.PoolManager != null)
                    {
                        GameEntry.PoolManager.DeSpawnSync(_activePopups[i]);
                    }
                }
            }

            _activePopups.Clear();
        }
    }
}