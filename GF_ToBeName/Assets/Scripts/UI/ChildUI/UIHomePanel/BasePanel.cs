//------------------------------------------------------------
// File : BasePanel.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public enum PanelType
    {
        Career,
        Home,
        Album,
    }

    public class BasePanel : MonoBehaviour
    {
        public PanelType _panelType;
    }
}