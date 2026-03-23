using UnityEngine;

namespace NewSideGame
{
    public enum RedPointType
    {
        None,
    }
    public class SingleRedPoint : MonoBehaviour, IUGUIChild
    {
        public RedPointType redPointType;

        public void OnOpen()
        {
            RedDotManager.Instance.RegisterRedPoint(redPointType, this);
        }

        public void OnClose()
        {
            RedDotManager.Instance.UnRegisterRedPoint(redPointType);
        }

        public void ShowRedPoint()
        {
            gameObject.SetActive(true);
        }

        public void HideRedPoint()
        {
            gameObject.SetActive(false);
        }

        public void OnInit() { }
        public void OnUpdate(float elapseSeconds, float realElapseSeconds) { }
        public void OnSecondUpdate(float time) { }
    }
}