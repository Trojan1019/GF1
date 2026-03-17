//------------------------------------------------------------
// File : UserProxy.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 玩家数据存储
//------------------------------------------------------------

using System;
using System.Linq;
using GameFramework.DataTable;
using UnityEngine;
using System.Collections.Generic;
using GameFramework;
using UnityGameFramework.Runtime;
using Newtonsoft.Json;

namespace NewSideGame
{
    public partial class UserProxy : BaseProxy<UserModel>
    {
        public UserModel userModel
        {
            get { return (UserModel)this.Data; }
        }

        public IDataTable<DRItem> ItemTables;
        public bool IsNewUser => IsProxyNull;

        public UserProxy(string proxyName, object data = null) : base(proxyName, data)
        {
            if (IsProxyNull)
            {
                SetItemCount((int)ItemType.Coin, 10);
            }
        }

        public override void OnRegister()
        {
            ItemTables = GameEntry.DataTable.GetDataTable<DRItem>();
        }

        public void Login(long loginTime)
        {
            Save();
        }

        #region 物品

        public int ItemCount(int itemId)
        {
            if (userModel.Items.ContainsKey(itemId.ToString()))
            {
                return userModel.Items[itemId.ToString()];
            }

            return 0;
        }

        public int ItemCount(ItemType itemType)
        {
            return ItemCount((int)itemType);
        }

        public void SetItemCount(int itemId, int count)
        {
            userModel.Items[itemId.ToString()] = count;
            Save();
        }

        public void AddItems(List<ItemData> items, string source = "", bool notify = true)
        {
            foreach (var item in items)
            {
                AddItem(item, source, notify);
            }
        }

        public void AddItem(ItemData item, string source = "", bool notify = true)
        {
            AddItem(item.id, item.num, source, notify);
        }

        public bool AddItem(int itemId, int count = 1, string source = "", bool notify = true)
        {
            ItemData itemData = new ItemData(itemId, count);

            switch (itemData.GetItemDataType())
            {
                case ItemDataType.Item:
                    {
                        ItemType itemType = (ItemType)itemId;
                        DRItem drItem = itemData.DRItem();
                        if (userModel.Items.ContainsKey(itemId.ToString()))
                            userModel.Items[itemId.ToString()] += count;
                        else
                            userModel.Items[itemId.ToString()] = count;
                        Save();
                        if (notify)
                        {
                            EventManager.Instance.NotifyEvent(Constant.Event.UserCurrencyChange, itemId);
                        }

                        return true;
                    }
            }


            return false;
        }

        public bool CostItem(ItemType itemType, int count, string source = "")
        {
            int itemId = (int)itemType;
            return CostItem(itemId, count, source);
        }

        public bool CostItem(int itemId, int count, string source = "")
        {
            if (userModel.Items.ContainsKey(itemId.ToString()))
            {
                int total = userModel.Items[itemId.ToString()];
                if (total >= count)
                {
                    userModel.Items[itemId.ToString()] -= count;
                    ProxyManager.UserProxy.NotifyCurrency(itemId);
                    Save();
                    return true;
                }
            }

            return false;
        }

        public void NotifyCurrency(int itemId)
        {
            EventManager.Instance.NotifyEvent(Constant.Event.UserCurrencyChange, itemId);
        }

        public void NotifyCurrency(List<ItemData> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                NotifyCurrency(datas[i].id);
            }
        }

        #endregion



        #region 内购

        public void AddPayment(float prize, float prizeUsd, string currency)
        {
            Save();
        }

        public bool IsIapPlayer()
        {
            if (userModel.transactionIDs == null)
            {
                return false;
            }
            return userModel.transactionIDs.Count > 0;
        }

        #endregion

        #region 弹窗

        public enum DialogFlag
        {
            NoAdDialog = 0,
        }

        public void SetDialogFlag(DialogFlag dialogFlag, bool flag)
        {
            Utility.Bitwise.SetBit(ref userModel.dialogShowFlag, (int)dialogFlag, flag);
            Save();
        }

        public bool CheckNoAdDialogFlag(DialogFlag dialogFlag)
        {
            return Utility.Bitwise.IsBit(userModel.dialogShowFlag, (int)dialogFlag);
        }

        #endregion



    }
}