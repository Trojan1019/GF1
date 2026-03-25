using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public partial class GameMain
    {
        [Header("ScorePopUp配置")] public Transform popupParent;
        [Tooltip("用于生成连击提示的父节点：与分数文本分离。为空时回退到 popupParent。")]
        public Transform comboPopupParent;
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

        public void ShowComboPopup(int comboCount, Vector3 worldPosition)
        {
            ScorePopup popup = GameEntry.PoolManager.SpawnSync<ScorePopup>(31003);
            Transform parent = comboPopupParent != null ? comboPopupParent : popupParent;
            popup.transform.SetParent(parent, false);
            popup.InitCustom(GameEntry.Localization.GetString("21", comboCount), Vector3.zero, 1.0f);
            _activePopups.Add(popup);
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