//------------------------------------------------------------
// File : PoolPrefabBase.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NewSideGame
{

    public abstract class BaseUnitObject : MonoBehaviour
    {
        [HideInInspector] public int AssetId;

        public int PoolCount = 0;

        public bool Resident = false;//是否常驻内存

        public virtual void OnInit(PoolManager ppm)
        {
        }

        public virtual void OnSpawn(PoolManager ppm)
        {
        }

        public virtual void OnDeSpawn(PoolManager ppm)
        {
        }

#if UNITY_EDITOR
        [ContextMenu("获取一个没用过的资源ID")]
        public void GetNotSameAssetID()
        {
            string numberString = ResourceIdentificationTool.GetNumbersFromString(name);

            int startIndex = 20000;
            if (int.TryParse(numberString, out int result))
            {
                startIndex = result;
            }

            int assetID = ResourceIdentificationTool.GetNotSameAssetIDByRange(startIndex);

            name = name.Replace(numberString, assetID.ToString());
        }
#endif
    }
}