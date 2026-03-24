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
        private readonly object _poolLock = new object();

        private float _toggle = ToggleTime;
        private long _spawnCalls;
        private long _deSpawnCalls;
        private long _poolCreateCount;

        public long SpawnCalls => _spawnCalls;
        public long DeSpawnCalls => _deSpawnCalls;
        public long PoolCreateCount => _poolCreateCount;
        public int ActivePoolCount
        {
            get
            {
                lock (_poolLock)
                {
                    return _prefabObjectPoolDictionary.Count;
                }
            }
        }

        public string GetRuntimeStats()
        {
            return $"PoolStats pools={ActivePoolCount}, spawnCalls={_spawnCalls}, despawnCalls={_deSpawnCalls}, createdPools={_poolCreateCount}";
        }

        public T SpawnSync<T>(int assetID) where T : BaseUnitObject
        {
            PrefabObjectPool prefabObjectPool = null;
            lock (_poolLock)
            {
                if (!_prefabObjectPoolDictionary.TryGetValue(assetID, out prefabObjectPool))
                {
                    prefabObjectPool = new PrefabObjectPool();
                    prefabObjectPool.AssetId = assetID;

                    _prefabObjectPoolDictionary[assetID] = prefabObjectPool;

                    prefabObjectPool.Initialize(1, this.transform);
                    _poolCreateCount++;
                }
            }

            BaseUnitObject poolPrefabBase = prefabObjectPool.SpawnNewObject();
            _spawnCalls++;

            if (poolPrefabBase == null) return null;

            poolPrefabBase.transform.SetParent(transform);

            return (T)poolPrefabBase;
        }

        public void DeSpawnSync(BaseUnitObject poolPrefabBase)
        {
            if (poolPrefabBase == null) return;
            PrefabObjectPool prefabObjectPool = null;
            lock (_poolLock)
            {
                _prefabObjectPoolDictionary.TryGetValue(poolPrefabBase.AssetId, out prefabObjectPool);
            }

            if (prefabObjectPool != null)
            {
                prefabObjectPool.DeSpawn(poolPrefabBase, transform);
                _deSpawnCalls++;
            }
        }

        private void Update()
        {
            _toggle -= Time.deltaTime;

            if (!(_toggle <= 0)) return;

            _toggle = ToggleTime;

            int keyToPurge = 0;
            bool hasPurge = false;
            lock (_poolLock)
            {
                foreach (var item in _prefabObjectPoolDictionary)
                {
                    if (!item.Value.NeedPurge) continue;

                    keyToPurge = item.Key;
                    hasPurge = true;
                    break;
                }
            }

            if (hasPurge)
            {
                lock (_poolLock)
                {
                    if (_prefabObjectPoolDictionary.TryGetValue(keyToPurge, out PrefabObjectPool pool))
                    {
                        pool.Purge();
                        _prefabObjectPoolDictionary.Remove(keyToPurge);
                        _toggle = 0;
                    }
                }
            }
        }
    }
}