//------------------------------------------------------------
// File : UGUIEventListener.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 动态监听
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NewSideGame
{
    public class UGUIEventListener : UnityEngine.EventSystems.EventTrigger
    {
        public UnityAction<GameObject> onClick;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            onClick?.Invoke(gameObject);
        }

        static public UGUIEventListener Get(GameObject go)
        {
            UGUIEventListener listener = go.GetOrAddComponent<UGUIEventListener>();
            return listener;
        }

    }
}


