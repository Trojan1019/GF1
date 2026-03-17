//------------------------------------------------------------
// File : ActivatablePoolPrefabBase.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;

namespace NewSideGame
{
    [DisallowMultipleComponent]
    public abstract class ActivatablePoolPrefabBase : BaseUnitObject
    {
        private Transform m_CachedTransform = null;
        public Transform CachedTransform { get => m_CachedTransform; }
        public override void OnInit(PoolManager ppm)
        {
            gameObject.SetActive(false);
            m_CachedTransform = transform;
        }

        public override void OnSpawn(PoolManager ppm)
        {
            gameObject.SetActive(true);
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            gameObject.SetActive(false);
        }

    }
}