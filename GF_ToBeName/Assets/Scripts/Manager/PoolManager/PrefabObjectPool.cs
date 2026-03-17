using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public class PrefabObjectPool
    {
        public int AssetId;
        // 预加载数量
        public int PreLoadCount = 10;
        // 是否常驻
        public bool Resident = false; //是否常驻内存

        private BaseUnitObject _baseUnitObject;
        private object _asset;

        //最后一次使用时间
        private float _lastUseTime;

        private List<BaseUnitObject> _freeBaseUnitObjects = new List<BaseUnitObject>();

        private List<BaseUnitObject> _usedBaseUnitObjects = new List<BaseUnitObject>();

        private int FreeCount => _freeBaseUnitObjects.Count;

        private int UsedCount => _usedBaseUnitObjects.Count;

        public int TotalCount => _usedBaseUnitObjects.Count + _freeBaseUnitObjects.Count;

        public override string ToString()
        {
            return string.Format("{0} T{1}-U{2}:F{3}", _baseUnitObject.name, TotalCount, UsedCount, FreeCount);
        }

        public void Initialize(int count, Transform root)
        {
            _freeBaseUnitObjects = new List<BaseUnitObject>();
            _usedBaseUnitObjects = new List<BaseUnitObject>();

            GameEntry.Resource.LoadAssetSync<Object>(AssetId, asset =>
            {
                if (asset is not GameObject gameObject) return;

                _asset = asset;

                _lastUseTime = Time.realtimeSinceStartup;
                _baseUnitObject = gameObject.GetComponent<BaseUnitObject>();

                for (int i = 0; i < count; i++)
                {
                    BaseUnitObject poolPrefabBase = UnityEngine.Object.Instantiate(_baseUnitObject);
                    poolPrefabBase.transform.SetParent(root);

                    poolPrefabBase.AssetId = AssetId;
                    poolPrefabBase.OnInit(null);
                    _freeBaseUnitObjects.Add(poolPrefabBase);
                }
            }, () =>
            {
            });
        }

        public BaseUnitObject SpawnNewObject()
        {
            if (_baseUnitObject == null) return null;

            _lastUseTime = Time.realtimeSinceStartup;
            BaseUnitObject poolPrefabBase = null;
            if (_freeBaseUnitObjects.Count > 0)
            {
                poolPrefabBase = _freeBaseUnitObjects[0];
                _freeBaseUnitObjects.RemoveAt(0);
            }
            else
            {
                poolPrefabBase = UnityEngine.Object.Instantiate(_baseUnitObject);

                poolPrefabBase.AssetId = AssetId;
                poolPrefabBase.OnInit(null);
            }

            if (poolPrefabBase == null)
            {
                return poolPrefabBase;
            }

            _usedBaseUnitObjects.Add(poolPrefabBase);
            poolPrefabBase.OnSpawn(null);

            return poolPrefabBase;
        }


        public void DeSpawn(BaseUnitObject baseUnitObject, Transform root)
        {
            _lastUseTime = Time.realtimeSinceStartup;

            baseUnitObject.OnDeSpawn(GameEntry.PoolManager);
            _freeBaseUnitObjects.Add(baseUnitObject);
            _usedBaseUnitObjects.Remove(baseUnitObject);

            baseUnitObject.transform.SetParent(root);
        }

        public void Purge()
        {
            foreach (var baseUnitObject in _freeBaseUnitObjects)
            {
                UnityEngine.Object.Destroy(baseUnitObject.gameObject);
            }
            _freeBaseUnitObjects.Clear();

            GameEntry.Resource.UnloadAsset(_asset);
            _asset = null;

            _baseUnitObject = null;
        }

        public bool NeedPurge
        {
            get
            {
                if (Resident)
                {
                    return false;
                }

                return UsedCount <= 0 && Time.realtimeSinceStartup - _lastUseTime > PoolManager.MAX_CLEAR_POOL_TIME;
            }
        }
    }
}