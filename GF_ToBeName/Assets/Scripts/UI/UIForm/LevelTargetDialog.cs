using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class LevelTargetDialog : BaseDialog
    {
        [Header("UI Components")] [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField] private TextMeshProUGUI conditionsText;
        [SerializeField] private Button closeBtn;

        [Header("Auto Close")] [SerializeField]
        private float autoCloseSeconds = 2.8f;

        private bool _isClosed;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _isClosed = false;

            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() =>
                {
                    if (_isClosed) return;
                    _isClosed = true;
                    Close();
                });
            }

            int stageIndex = m_Params != null ? m_Params.GetIntParams("StageIndex") : 0;
            Refresh(stageIndex);

            // 目标面板自动关闭，避免遮挡操作
            StartCoroutine(AutoCloseCoroutine());
        }

        private System.Collections.IEnumerator AutoCloseCoroutine()
        {
            yield return new WaitForSeconds(autoCloseSeconds);
            if (_isClosed) yield break;
            _isClosed = true;
            Close();
        }

        private void Refresh(int stageIndex)
        {
            if (GameEntry.Localization != null && titleText != null)
            {
                string localized = GameEntry.Localization.GetString("86");
                titleText.text = (string.IsNullOrEmpty(localized) || localized.Contains("<NoKey>")) ? "Level Target" : localized;
                titleText.transform.DOKill();
                titleText.transform.localScale = Vector3.one;
                titleText.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0), 0.25f, 6, 1).SetUpdate(true);
            }

            CubeCrushStage cfg = null;
            if (GameMain.Instance != null)
                cfg = GameMain.Instance.GetStageConfig(stageIndex);

            if (conditionsText == null) return;

            if (cfg == null)
            {
                conditionsText.text = string.Empty;
                return;
            }

            StringBuilder sb = new StringBuilder();

            // 分数要求
            string scoreDescFmt = GameEntry.Localization.GetString("87");
            if (string.IsNullOrEmpty(scoreDescFmt) || scoreDescFmt.Contains("<NoKey>"))
                scoreDescFmt = "Score >= {0}";
            sb.AppendLine(string.Format(scoreDescFmt, cfg.targetScoreLocal));

            // 道具收集要求
            if (cfg.goalRequirements != null && cfg.goalRequirements.Count > 0)
            {
                string itemHeader = GameEntry.Localization.GetString("56");
                if (string.IsNullOrEmpty(itemHeader) || itemHeader.Contains("<NoKey>"))
                    itemHeader = "Collect items:";
                sb.AppendLine(itemHeader);

                for (int i = 0; i < cfg.goalRequirements.Count; i++)
                {
                    var req = cfg.goalRequirements[i];
                    if (req == null) continue;
                    if (req.itemType == CubeCrushGoalItemType.None) continue;

                    string itemName = GetItemName(req.itemType);
                    string itemLineFmt = GameEntry.Localization.GetString("57");
                    if (string.IsNullOrEmpty(itemLineFmt) || itemLineFmt.Contains("<NoKey>"))
                        itemLineFmt = "{0} x {1}";
                    sb.AppendLine("- " + string.Format(itemLineFmt, itemName, req.requiredCount));
                }
            }

            // 其它可选条件：当前 StageConfig 未提供时不展示
            conditionsText.text = sb.ToString().TrimEnd();
        }

        private string GetItemName(CubeCrushGoalItemType type)
        {
            string key = null;
            switch (type)
            {
                case CubeCrushGoalItemType.Glove:
                    key = "58";
                    break;
                case CubeCrushGoalItemType.Star:
                    key = "59";
                    break;
                case CubeCrushGoalItemType.Gem:
                    key = "60";
                    break;
            }

            if (!string.IsNullOrEmpty(key))
            {
                string localized = GameEntry.Localization.GetString(key);
                if (!string.IsNullOrEmpty(localized) && !localized.Contains("<NoKey>"))
                    return localized;
            }

            return type.ToString();
        }
    }
}

