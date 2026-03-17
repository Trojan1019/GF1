//------------------------------------------------------------
// File : AutoDestroy.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using NewSideGame;
using UnityEngine;

namespace NewSideGame
{
    public class AutoDestroy : MonoBehaviour
    {
        public float delayTime = 1;

        private float time = 0;

        private BaseUnitObject prefabPool;

        void Start()
        {
            prefabPool = gameObject.GetComponent<BaseUnitObject>();
        }

        void Update()
        {
            time += Time.deltaTime;
            if (time >= delayTime)
            {
                _Destroy();
            }
        }

        public void Reset()
        {
            time = 0;
        }


        private void _Destroy()
        {
            if (prefabPool != null)
            {
                time = 0;
                GameEntry.PoolManager.DeSpawnSync(prefabPool);
            }
            else
            {
                CUtility.Mono.Destroy(gameObject);
            }
        }

    }

}