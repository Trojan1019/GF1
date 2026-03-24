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

        public void Setup(SkinConfig config, string displayName, SkinState state, int remainAdCount, System.Action<int> onClick)
        {
            if (config == null) return;
            _skinId = config.skinId;
            _onClick = onClick;

            if (previewImage != null) previewImage.sprite = config.previewSprite;
            if (nameText != null) nameText.text = displayName;

            bool isUsing = state == SkinState.Using;
            bool isLocked = state == SkinState.Locked;
            if (checkIcon != null) checkIcon.gameObject.SetActive(isUsing);
            if (lockIcon != null) lockIcon.gameObject.SetActive(isLocked);

            if (unlockHintText != null)
            {
                if (isLocked)
                {
                    string fmt = GameEntry.Localization.GetString("20003");
                    if (string.IsNullOrEmpty(fmt) || fmt.Contains("<NoKey>"))
                    {
                        fmt = "Watch {0} ads to unlock";
                    }
                    unlockHintText.text = string.Format(fmt, Mathf.Max(0, remainAdCount));
                    unlockHintText.gameObject.SetActive(true);
                }
                else
                {
                    unlockHintText.gameObject.SetActive(false);
                }
            }
        }

        private void OnClick()
        {
            _onClick?.Invoke(_skinId);
        }
    }
}

