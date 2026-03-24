using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NewSideGame
{
    public class GoalItemUIBind : MonoBehaviour
    {
        public CubeCrushGoalItemType itemType = CubeCrushGoalItemType.None;
        public Image iconImage;
        public TextMeshProUGUI countText;
        public Image checkIconImage;
    }
}