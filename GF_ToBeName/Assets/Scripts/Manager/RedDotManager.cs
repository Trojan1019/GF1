//------------------------------------------------------------
// File : RedDotManager.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace NewSideGame
{
    public class RedDotManager : MonoSingleton<RedDotManager>
    {
        private readonly Dictionary<RedPointType, SingleRedPoint> _redPointDic = new Dictionary<RedPointType, SingleRedPoint>();

        private void CheckRedPoint(RedPointType redPointType)
        {
            if (_redPointDic.TryGetValue(redPointType, out SingleRedPoint redPoint))
            {
                redPoint.HideRedPoint();

                switch (redPointType)
                {
                    case RedPointType.None:
                        {
                        
                        }
                        break;
              
                }
            }
        }

        public void RegisterRedPoint(RedPointType redPointType, SingleRedPoint redPoint)
        {
            _redPointDic[redPointType] = redPoint;
            CheckRedPoint(redPointType);
        }

        public void UnRegisterRedPoint(RedPointType redPointType)
        {
            if (_redPointDic.ContainsKey(redPointType))
            {
                _redPointDic.Remove(redPointType);
            }
        }

        public void UpdateRedPoint(RedPointType redPointType)
        {
            CheckRedPoint(redPointType);
        }
    }
}