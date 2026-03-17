using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GameFramework;
using UnityEngine.UI;

namespace NewSideGame
{
    public class ScrollDynamicListItem<T, D> : MonoBehaviour, IScrollDynamicListItem<T> where T : ScrollDymaicBaseData<D> where D : ScrollDymaicChildData
    {
        public Transform parent;
        protected T itemData;
        public ScrollDynamicListChildItem<D> childItem;
        public SlotList<ScrollDynamicListChildItem<D>> slotList = new SlotList<ScrollDynamicListChildItem<D>>();

        public virtual void SetData(T data)
        {
            if (itemData != null && itemData.index == data.index)
                return;

            itemData = data;
            slotList.Init(childItem, parent, true);
            slotList.SetCount(data.datas.Count);

            Vector2 grid = Vector2.zero;
            int r = 0;
            for (int i = 0; i < slotList.Count; i++)
            {
                var cell = slotList[i];
                int result = i % itemData.CellAlignment;
                if (i > 0 && i % itemData.CellAlignment == 0) r++;
                grid = new Vector2(result, r);
                float x = itemData.Offset.x + (itemData.SizeDelta.x + itemData.Spacing.x) * grid.x;
                float y = -(itemData.Offset.y + (itemData.SizeDelta.y + itemData.Spacing.y) * grid.y);
                RectTransform childRect = cell.GetComponent<RectTransform>();
                childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                childRect.pivot = new Vector2(0.5f, 0.5f);
                childRect.anchoredPosition = new Vector2(x, y);
                cell.SetData(data.datas[i]);
            }

            float sizeY = r * data.Spacing.y + (r + 1) * data.SizeDelta.y + data.MarginBottom;
            RectTransform rect = parent.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, sizeY);
        }

        public void ClearData()
        {
            itemData = null;
        }
    }
}