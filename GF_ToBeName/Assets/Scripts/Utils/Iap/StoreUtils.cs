using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using GameFramework.DataTable;

namespace NewSideGame
{
    public class StoreUtils
    {
        //根据商品id获取ItemIAP，订阅BaseProductId和ProductId相同
        public static DRItemIAP GetItemIAP(string productID)
        {
            IDataTable<DRItemIAP> iaps = GameEntry.DataTable.GetDataTable<DRItemIAP>();
            return iaps.GetDataRow((DRItemIAP info) => info.ProductId == productID);
        }

        public static DRItemIAP GetItemIAP(int goodsID)
        {
            IDataTable<DRItemIAP> iaps = GameEntry.DataTable.GetDataTable<DRItemIAP>();
#if UNITY_ANDROID
            goodsID = goodsID + 1000;
#endif
            return iaps.GetDataRow((DRItemIAP info) => info.Id == goodsID);
        }

        public static List<ItemData> GetItems(string rewardStr, bool removeRepeat = false)
        {
            List<ItemData> retList = new List<ItemData>();

            if (rewardStr == "-1" || rewardStr == "-1.0" || string.IsNullOrEmpty(rewardStr)) //空数据
            {
                return retList;
            }

            try
            {
                string[] rewards = rewardStr.Split(';');

                foreach (var reward in rewards)
                {
                    string[] rewardItem = reward.Split(',');

                    ItemData item = new ItemData(int.Parse(rewardItem[0]), int.Parse(rewardItem[1]));
                    retList.Add(item);
                }

                if (removeRepeat)
                {
                    return GetItemsByFilter(retList);
                }
                else
                {
                    return retList;
                }
            }
            catch (Exception ex)
            {

                UnityGameFramework.Runtime.Log.Error("解析异常 ex:{0}   reward:{1}", ex.Message, rewardStr);

                return retList;
            }

        }

        /// <summary>
        /// 过滤重复奖励
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<ItemData> GetItemsByFilter(List<ItemData> items)
        {
            Dictionary<int, ItemData> allItems = new Dictionary<int, ItemData>();
            foreach (var item in items)
            {
                if (!allItems.ContainsKey(item.id))
                {
                    allItems.Add(item.id, item);
                }
                else
                {
                    allItems[item.id].num += item.num;
                }
            }
            return allItems.Values.ToList();
        }


    }
}