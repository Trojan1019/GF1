using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace NewSideGame
{
    public class PoolManager : GameFrameworkComponent
    {
        public const float ToggleTime = 5;
        public const float MAX_CLEAR_POOL_TIME = 40;

        private Dictionary<int, PrefabObjectPool> _prefabObjectPoolDictionary = new Dictionary<int, PrefabObjectPool>();

        private float _toggle = ToggleTime;

        public T SpawnSync<T>(int assetID) where T : BaseUnitObject
        {
            PrefabObjectPool prefabObjectPool = null;

            if (!_prefabObjectPoolDictionary.TryGetValue(assetID, out prefabObjectPool))
            {
                prefabObjectPool = new PrefabObjectPool();
                prefabObjectPool.AssetId = assetID;

                _prefabObjectPoolDictionary[assetID] = prefabObjectPool;

                prefabObjectPool.Initialize(1, this.transform);
            }

            BaseUnitObject poolPrefabBase = prefabObjectPool.SpawnNewObject();

            if (poolPrefabBase == null) return null;

            poolPrefabBase.transform.SetParent(transform);

            return (T)poolPrefabBase;
        }

        public void DeSpawnSync(BaseUnitObject poolPrefabBase)
        {
            if (_prefabObjectPoolDictionary.TryGetValue(poolPrefabBase.AssetId, out PrefabObjectPool prefabObjectPool))
            {
                prefabObjectPool.DeSpawn(poolPrefabBase, transform);
            }
        }

        private void Update()
        {
            _toggle -= Time.deltaTime;

            if (!(_toggle <= 0)) return;

            _toggle = ToggleTime;

            foreach (var item in _prefabObjectPoolDictionary)
            {
                if (!item.Value.NeedPurge) continue;

                item.Value.Purge();
                _prefabObjectPoolDictionary.Remove(item.Key);
                _toggle = 0;
                break;
            }
        }
    }
}