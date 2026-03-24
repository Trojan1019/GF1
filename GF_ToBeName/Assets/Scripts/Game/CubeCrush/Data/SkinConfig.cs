using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    [CreateAssetMenu(fileName = "SkinConfig", menuName = "CubeCrush/Skin Config")]
    public class SkinConfig : ScriptableObject
    {
        [Header("Identity")]
        public int skinId;
        public string skinName = "Default";
        public string skinNameKey = "20010";
        public Sprite previewSprite;

        [Header("Unlock")]
        public int adWatchRequiredCount = 0;
        public bool unlocked = false;

        [Header("Visual Mapping")]
        public Sprite bgSprite;
        public Sprite boardSprite;
        public List<string> relatedPrefabResourcePaths = new List<string>();
    }

    [Serializable]
    public enum SkinState
    {
        Locked = 0,
        Unlocked = 1,
        Using = 2
    }
}

