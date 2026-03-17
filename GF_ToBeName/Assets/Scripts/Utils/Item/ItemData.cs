using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.DataTable;
using UnityEngine;

namespace NewSideGame
{
    public enum ItemDataType
    {
        None = 0,
        Item = 1,
    }

    /// <summary>
    /// 服务端交互用的通用奖励物品model
    /// </summary>
    [Serializable]
    public class ItemData
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public int id;

        /// <summary>
        /// 物品数量
        /// </summary>
        public int num;

        public ItemData(int _id)
        {
            id = _id;
            num = 1;
        }

        public ItemData(int _id, int _num)
        {
            id = _id;
            num = _num;
        }

        public ItemDataType GetItemDataType()
        {
            return ItemDataType.Item;
        }

        public string StyleNum()
        {
            return ItemUtils.GetNumByStyle(DRItem(), num);
        }

        public string MultStyleNum()
        {
            return ItemUtils.GetMultNumByStyle(DRItem(), num);
        }

        public string GetName(bool icon2 = false)
        {
            return icon2 ? DRItem().Icon2 : DRItem().Icon;
        }

        public Sprite GetSprite(bool icon2 = false)
        {
            return SpriteAtlasManager.Instance.GetSprite(SpriteAtlasId.Item, GetName(icon2));
        }

        public void SetSprite(UnityEngine.UI.Image img, bool icon2 = false)
        {
            img.sprite = SpriteAtlasManager.Instance.GetSprite(SpriteAtlasId.Item, GetName(icon2));
        }

        [NonSerialized] private DRItem _drItem = null;

        public DRItem DRItem()
        {
            if (_drItem == null)
            {
                _drItem = ProxyManager.UserProxy.ItemTables.GetDataRow(id);
            }

            if (_drItem == null)
            {
                UnityGameFramework.Runtime.Log.Error(" cant find dr item id={0}", id);
                id = 1;
                _drItem = ProxyManager.UserProxy.ItemTables.GetDataRow(id);
            }

            return _drItem;
        }
    }
}