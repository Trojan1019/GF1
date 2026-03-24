using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    [CreateAssetMenu(fileName = "SkinDatabase", menuName = "CubeCrush/Skin Database")]
    public class SkinDatabase : ScriptableObject
    {
        public List<SkinConfig> skins = new List<SkinConfig>();
    }
}

