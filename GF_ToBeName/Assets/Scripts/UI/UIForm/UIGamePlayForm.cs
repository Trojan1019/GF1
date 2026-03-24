using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

namespace NewSideGame
{
    public class UIGamePlayForm : UGuiForm
    {
        [Header("UI Components")] public TextMeshProUGUI scoreText;
        public TextMeshProUGUI bestScoreText;
        public Button restartButton;
        public Button settingButton;
        public Button skinButton;
        public Button dailySignButton;

        [Header("Goal UI")] [SerializeField] private RectTransform goalPanel;
        [SerializeField] private List<GoalItemUIBind> goalItemBinds = new List<GoalItemUIBind>();
        [SerializeField] private float goalPulseDuration = 0.4f;
        [SerializeField] private float goalPulseScale = 1.2f;
        [SerializeField] private float checkFadeDuration = 0.2f;

        private int displayScore = 0;

        private readonly Dictionary<CubeCrushGoalItemType, bool> _goalCompletedState =
            new Dictionary<CubeCrushGoalItemType, bool>();

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            EventManager.Instance.AddEventListener(Constant.Event.GameOver, OnGameOver);
            EventManager.Instance.AddEventListener(Constant.Event.RefreshScore, OnRefreshScore);
            EventManager.Instance.AddEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
            EventManager.Instance.AddEventListener(Constant.Event.CubeCrushGoalProgressChanged, OnGoalProgressChanged);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    if (GameMain.Instance.isGameOverFillAnimating)
                    {
                        return;
                    }

                    // 按钮点击弹性反馈
                    restartButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameLoopManager.Instance.Restart();
                });
            }

            if (settingButton != null)
            {
                settingButton.onClick.RemoveAllListeners();
                settingButton.onClick.AddListener(() =>
                {
                    if (GameMain.Instance.isGameOverFillAnimating)
                    {
                        return;
                    }

                    // 按钮点击弹性反馈
                    settingButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameEntry.UI.OpenUIForm(UIFormType.SettingDialog);
                });
            }

            if (skinButton != null)
            {
                skinButton.onClick.RemoveAllListeners();
                skinButton.onClick.AddListener(() =>
                {
                    if (GameMain.Instance.isGameOverFillAnimating) return;
                    skinButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameEntry.UI.OpenUIForm(UIFormType.SkinDialog);
                });
            }

            if (dailySignButton != null)
            {
                dailySignButton.onClick.RemoveAllListeners();
                dailySignButton.onClick.AddListener(() =>
                {
                    if (GameMain.Instance.isGameOverFillAnimating) return;
                    dailySignButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 10, 1).SetUpdate(true);
                    GameEntry.UI.OpenUIForm(UIFormType.DailyRewardDialog);
                });
            }

            displayScore = GameLoopManager.Instance.score;
            if (scoreText != null)
            {
                UpdateScoreText(displayScore);
            }

            if (bestScoreText != null)
            {
                // 获取并显示最高分
                int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
                bestScoreText.text =
                    string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), bestScore); // 使用千位分隔符格式化
            }

            RefreshGoalPanel();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            EventManager.Instance.RemoveEventListener(Constant.Event.GameOver, OnGameOver);
            EventManager.Instance.RemoveEventListener(Constant.Event.RefreshScore, OnRefreshScore);
            EventManager.Instance.RemoveEventListener(Constant.Event.LanguageChangeSuccess, OnLanguageChanged);
            EventManager.Instance.RemoveEventListener(Constant.Event.CubeCrushGoalProgressChanged,
                OnGoalProgressChanged);

            for (int i = 0; i < goalItemBinds.Count; i++)
            {
                var bind = goalItemBinds[i];
                if (bind == null) continue;
                bind.transform.DOKill();
                bind.transform.localScale = Vector3.one;
                if (bind.checkIconImage != null)
                {
                    bind.checkIconImage.DOKill();
                    bind.checkIconImage.rectTransform.DOKill();
                    bind.checkIconImage.rectTransform.localScale = Vector3.one;
                }
            }

            _goalCompletedState.Clear();

            base.OnClose(isShutdown, userData);
        }

        private void OnGameOver(params object[] args)
        {
            GameEntry.UI.OpenUIForm(UIFormType.GameFailRetryDialog);
        }

        private void OnRefreshScore(params object[] args)
        {
            UpdateScore(GameLoopManager.Instance.score);
        }

        private void OnLanguageChanged(object[] args)
        {
            // 重新刷新当前显示的分数文本与最高分标签（不重置分数滚动动画进度）
            UpdateScoreText(displayScore);
            if (bestScoreText != null)
            {
                int bestScore = ProxyManager.UserProxy != null ? ProxyManager.UserProxy.userModel.bestScore : 0;
                bestScoreText.text = string.Format("{0}:{1:N0}", GameEntry.Localization.GetString("2"), bestScore);
            }

            RefreshGoalPanel();
        }

        private void OnGoalProgressChanged(params object[] args)
        {
            RefreshGoalPanel();
        }

        private void RefreshGoalPanel()
        {
            if (goalPanel == null) return;
            if (!GameMain.Instance.IsStageSurvival)
            {
                goalPanel.gameObject.SetActive(false);
                return;
            }

            List<CubeCrushGoalProgress> goals = GameLoopManager.Instance.StageGoals;
            if (goals == null || goals.Count == 0)
            {
                goalPanel.gameObject.SetActive(false);
                return;
            }

            goalPanel.gameObject.SetActive(true);

            // 只显示当前关卡真正需要的道具类型，其它 UI 自动隐藏
            HashSet<CubeCrushGoalItemType> requiredTypes = new HashSet<CubeCrushGoalItemType>();
            for (int i = 0; i < goals.Count; i++)
            {
                requiredTypes.Add(goals[i].itemType);
            }

            for (int j = 0; j < goalItemBinds.Count; j++)
            {
                var bind = goalItemBinds[j];
                if (bind == null) continue;

                bool shouldShow = requiredTypes.Contains(bind.itemType);
                bind.gameObject.SetActive(shouldShow);

                if (!shouldShow)
                {
                    bind.transform.DOKill();
                    bind.transform.localScale = Vector3.one;
                    if (bind.checkIconImage != null)
                    {
                        bind.checkIconImage.DOKill();
                        bind.checkIconImage.rectTransform.DOKill();
                        bind.checkIconImage.rectTransform.localScale = Vector3.one;
                        bind.checkIconImage.gameObject.SetActive(false);
                        bind.checkIconImage.color = new Color(1f, 1f, 1f, 0f);
                    }

                    continue;
                }

                // 找到对应 goal 数据
                for (int i = 0; i < goals.Count; i++)
                {
                    var goal = goals[i];
                    if (goal.itemType != bind.itemType) continue;

                    if (bind.iconImage != null)
                    {
                        bind.iconImage.sprite = GameMain.Instance.GetGoalItemSprite(goal.itemType);
                    }

                    if (bind.countText != null)
                    {
                        bind.countText.text = $"{goal.remainingCount}";
                    }

                    bool completed = goal.remainingCount <= 0;
                    UpdateGoalCompleteState(bind, completed);
                    if (!completed)
                    {
                        PlayGoalPulse(bind.transform);
                    }

                    break;
                }
            }
        }

        private void UpdateGoalCompleteState(GoalItemUIBind bind, bool completed)
        {
            bool oldCompleted = false;
            _goalCompletedState.TryGetValue(bind.itemType, out oldCompleted);
            _goalCompletedState[bind.itemType] = completed;

            bind.countText.gameObject.SetActive(!completed);

            bind.checkIconImage.DOKill();
            bind.checkIconImage.rectTransform.DOKill();
            bind.checkIconImage.rectTransform.localScale = Vector3.one;

            if (!completed)
            {
                bind.checkIconImage.gameObject.SetActive(false);
                bind.checkIconImage.color = new Color(1f, 1f, 1f, 0f);
                return;
            }

            bind.checkIconImage.gameObject.SetActive(true);
            // 只在状态首次完成时播放勾选动画，避免频繁重绘
            if (!oldCompleted)
            {
                bind.checkIconImage.color = new Color(1f, 1f, 1f, 0f);
                bind.checkIconImage.DOFade(1f, checkFadeDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                PlayGoalPulse(bind.checkIconImage.rectTransform);
            }
            else
            {
                bind.checkIconImage.color = Color.white;
            }
        }

        private void PlayGoalPulse(Transform target)
        {
            if (target == null) return;

            target.DOKill();
            target.localScale = Vector3.one;

            float duration = Mathf.Clamp(goalPulseDuration, 0.3f, 0.5f);
            float peakScale = Mathf.Clamp(goalPulseScale, 1.0f, 1.2f);
            float half = duration * 0.5f;

            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOScale(peakScale, half).SetEase(Ease.OutQuad).SetUpdate(true));
            seq.Append(target.DOScale(1f, half).SetEase(Ease.OutQuad).SetUpdate(true));
        }

        private void UpdateScore(int newScore)
        {
            if (scoreText != null)
            {
                // 分数滚动动画
                DOTween.To(() => displayScore, x =>
                {
                    displayScore = x;
                    UpdateScoreText(displayScore);
                }, newScore, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
            }
        }

        private void UpdateScoreText(int currentScore)
        {
            if (GameMain.Instance.IsStageSurvival)
            {
                int stageIndex = GameLoopManager.Instance.StageIndex;
                int stageStartScore = GameLoopManager.Instance.StageStartScore;
                int stageTargetDelta = GameLoopManager.Instance.StageTargetDelta;
                int scoreDelta = currentScore - stageStartScore;

                string scoreLabel = GameEntry.Localization.GetString("5"); // 分数
                string stageLabel = string.Format(GameEntry.Localization.GetString("32"), stageIndex); // 第 {0} 关
                string targetLabel = GameEntry.Localization.GetString("23"); // 目标分数
                scoreText.text = string.Format("{0}:{1:N0}\n{2}\n{3}:{4:N0}/{5:N0}",
                    scoreLabel,
                    currentScore,
                    stageLabel,
                    targetLabel,
                    scoreDelta,
                    stageTargetDelta);
            }
            else
            {
                string scoreLabel = GameEntry.Localization.GetString("5"); // 分数
                scoreText.text = string.Format("{0}:{1:N0}", scoreLabel, currentScore);
            }
        }
    }
}