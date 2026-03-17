//------------------------------------------------------------
// File : CasualCurrencyGroup.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class CasualCurrencyGroup : ActivatablePoolPrefabBase, ICurrencyGroup
    {
        public static int CasualAssetId = 12005;
        private readonly Vector2 spaing = new Vector2(25f, -39f);
        private readonly float paddingLeft = 70f;
        private readonly float minAnchorY = -60f;
        private readonly float sizeDeltaY = 78f;

        public CurrencyCell m_Temp;
        private Dictionary<int, CurrencyCell> m_CurrencyCellMap = new Dictionary<int, CurrencyCell>();
        private SlotList<CurrencyCell> m_SlotList = new SlotList<CurrencyCell>();
        private RectTransform rectTransform => gameObject.GetComponent<RectTransform>();

        private UIDepth depth;

        public override void OnInit(PoolManager ppm)
        {
            base.OnInit(ppm);

            gameObject.name = "CurrencyGroup";
            m_SlotList.Init(m_Temp, transform, true);
            depth = gameObject.GetComponent<UIDepth>();
        }

        public override void OnSpawn(PoolManager ppm)
        {
            base.OnSpawn(ppm);
            m_SlotList.SetCount(0);
            m_CurrencyCellMap.Clear();
            EventManager.Instance.AddEventListener(Constant.Event.UserCurrencyChange, OnCurrnecyChange);
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);
            EventManager.Instance.RemoveEventListener(Constant.Event.UserCurrencyChange, OnCurrnecyChange);
            foreach (var item in m_CurrencyCellMap)
            {
                item.Value.ClearData();
            }

            m_CurrencyCellMap.Clear();
            m_SlotList.SetCount(0);
        }

        public void InitialCurrency(Transform parent, List<ItemType> itemsType, Vector2 anchor = default)
        {
            List<int> itemsId = new List<int>();
            itemsType.ForEach((item) => { itemsId.Add((int)item); });
            InitialCurrency(parent, itemsId, anchor);
        }

        // private void RefreshSafeArea()
        // {
        //     Rect safeArea = Screen.safeArea;
        //     float screenWidth = Screen.width;
        //     float screenHeight = Screen.height;
        //     float HeightNormalized = rectTransform.sizeDelta.y * GameEntry.UI.MainCanvas.scaleFactor / screenHeight;
        //
        //     Vector2 anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
        //     Vector2 anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
        //     anchorMin = new Vector2(anchorMin.x, anchorMax.y - HeightNormalized);
        //     Vector2 resultAnchorMin = new Vector2(anchorMin.x, anchorMin.y <= AspectSafeAreaHelper.anchorMinY ? AspectSafeAreaHelper.anchorMinY : anchorMin.y);
        //     Vector2 resultAnchorMax = new Vector2(anchorMax.x, anchorMax.y >= AspectSafeAreaHelper.anchorMaxY ? AspectSafeAreaHelper.anchorMaxY : anchorMax.y);
        //
        //     rectTransform.anchorMin = new Vector2(resultAnchorMin.x, resultAnchorMin.y);
        //     rectTransform.anchorMax = new Vector2(resultAnchorMax.x, resultAnchorMax.y);
        //     rectTransform.sizeDelta = Vector2.zero;
        //     rectTransform.anchoredPosition = Vector2.zero;
        // }

        /// <summary>
        /// 初始化货币信息
        /// </summary>
        /// <param name="itemsId"> 要展示的货币数量 </param>
        /// <param name="usingSystemValue"> 是否使用系统值  false: 使用局部变量 </param>
        private void InitialCurrency(Transform parent, List<int> itemsId, Vector2 anchor = default)
        {
            SetTransform(parent);
            //RefreshSafeArea();

            m_SlotList.SetCount(itemsId.Count);

            for (int i = 0; i < itemsId.Count; i++)
            {
                var slot = m_SlotList.GetSlot(i);
                slot.SetData(itemsId[i]);
                slot.name = string.Format("Currency_{0}", itemsId[i]);

                RectTransform cellRect = slot.GetComponent<RectTransform>();
                int index = i;
                switch ((ItemType)itemsId[i])
                {
                    case ItemType.Coin:
                        index = 0;
                        break;
                    //co总要求的
                    case ItemType.Prop_Tip:
                    case ItemType.Prop_Tube:
                    case ItemType.Prop_Undo:
                    case ItemType.Prop_Skip:
                        slot.m_icon.transform.localScale = 0.9f * Vector3.one;
                        break;
                    default:
                        index = i;
                        break;
                }

                float x = cellRect.sizeDelta.x / 2 + paddingLeft + spaing.x * index + index * cellRect.sizeDelta.x;
                cellRect.anchoredPosition = new Vector2(x, spaing.y) + anchor;
                m_CurrencyCellMap.Add(itemsId[i], slot);
            }
        }

        private void SetTransform(Transform parent)
        {
            if (parent != null)
            {
                CachedTransform.SetParent(parent);
            }
            else
            {
                CachedTransform.SetParent(UITopCoverForm.Instance.transform);
            }

            CachedTransform.localScale = Vector3.one;
            CachedTransform.localPosition = Vector3.zero;
        }

        //货币变化
        private void OnCurrnecyChange(params object[] args)
        {
            if (args.Length == 1)
            {
                int itemId = (int)args[0];

                var itemCell = GetCurrencyCell(itemId);
                if (itemCell != null)
                {
                    itemCell.OnCurrencyChange();
                }
            }
        }

        public CurrencyCell GetCurrencyCell(int itemId)
        {
            if (m_CurrencyCellMap.ContainsKey(itemId))
            {
                return m_CurrencyCellMap[itemId];
            }

            foreach (var item in m_CurrencyCellMap)
            {
                if (item.Value.CurrencyType == itemId)
                {
                    return item.Value;
                }
            }

            return null;
        }

        public Transform GetItemTransform(ItemType type)
        {
            var currencyCell = GetCurrencyCell((int)type);
            if (currencyCell != null)
                return currencyCell.m_icon.transform;
            return null;
        }

        public Transform GetItemTransform(int type)
        {
            var currencyCell = GetCurrencyCell(type);
            if (currencyCell != null)
                return currencyCell.m_icon.transform;
            return null;
        }

        public void SetDepth(int layer)
        {
            depth.order = layer;
            depth.Refresh();
        }
    }
}