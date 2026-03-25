using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class SkinItem : MonoBehaviour
    {
        [SerializeField] private Image previewImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI unlockHintText;
        [SerializeField] private Image checkIcon;
        [SerializeField] private Image maskImage;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Button clickButton;

        private int _skinId;
        private System.Action<int> _onClick;

        private void Awake()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(OnClick);
            }
        }

        public void Setup(SkinConfig config, string displayName, SkinState state, int remainAdCount,
            System.Action<int> onClick)
        {
            if (config == null) return;
            _skinId = config.skinId;
            _onClick = onClick;

            previewImage.sprite = config.previewSprite;
            nameText.text = displayName;

            bool isUsing = state == SkinState.Using;
            bool isLocked = state == SkinState.Locked;

            checkIcon.gameObject.SetActive(isUsing);
            lockIcon.gameObject.SetActive(isLocked);
            maskImage.gameObject.SetActive(isLocked);

            if (isLocked)
            {
                if (unlockHintText == null) return;

                // 这里必须使用“只需要 {0} 一个参数”的广告解锁文案 key。
                // 否则 string.Format 会因占位符数量不匹配而抛异常。
                string fmt = GameEntry.Localization.GetString("74");
                if (string.IsNullOrEmpty(fmt) || fmt.Contains("<NoKey>") || !fmt.Contains("{0}"))
                {
                    fmt = "Watch {0} ads to unlock";
                }

                if (remainAdCount <= 0)
                {
                    unlockHintText.gameObject.SetActive(false);
                    return;
                }

                unlockHintText.text = SafeFormat(fmt, Mathf.Max(0, remainAdCount));
                unlockHintText.gameObject.SetActive(true);
            }
            else
            {
                unlockHintText.gameObject.SetActive(false);
            }
        }

        private void OnClick()
        {
            _onClick?.Invoke(_skinId);
        }

        private static string SafeFormat(string fmt, int value0)
        {
            // 安全适配：如果 fmt 里有 {1} {2} 之类的占位符但只传了一个值，就用 0 补齐。
            // 避免因本地化配置差异导致 FormatException。
            try
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(fmt, @"\{(\d+)\}");
                int max = -1;
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    if (m.Success)
                    {
                        if (int.TryParse(m.Groups[1].Value, out int idx))
                            max = Mathf.Max(max, idx);
                    }
                }

                if (max <= 0)
                {
                    return string.Format(fmt, value0);
                }

                object[] args = new object[max + 1];
                args[0] = value0;
                for (int i = 1; i < args.Length; i++) args[i] = 0;
                return string.Format(fmt, args);
            }
            catch
            {
                return $"Watch {value0} ads to unlock";
            }
        }
    }
}