//------------------------------------------------------------
// File : ScrollDynamicList.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GameFramework;

public abstract class ScrollDymaicBaseData<T> : IScrollDynamicReference<T> where T : ScrollDymaicChildData
{
    public int index;
    public List<T> datas;
    public ScrollDymaicBaseData(int index, List<T> datas)
    {
        this.index = index;
        this.datas = datas;
    }
    public void Clear()
    {
        if (datas == null || datas.Count <= 0) return;
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i] is IReference)
            {
                ReferencePool.Release(datas[i]);
            }
        }
        datas.Clear();
    }
    public abstract int CellAlignment { get; }
    public abstract Vector2 Spacing { get; }
    public abstract Vector2 SizeDelta { get; }
    public abstract Vector2 Offset { get; }
    public abstract float MarginTop { get; }
    public abstract float MarginBottom { get; }
}

public abstract class ScrollDymaicChildData : IReference
{
    public void Clear() { }
}

public abstract class ScrollDynamicList<T, D, C> : MonoBehaviour
    where T : IScrollDynamicListItem<D>
    where D : ScrollDymaicBaseData<C>
    where C : ScrollDymaicChildData
{
    public GameObject cell;
    private ScrollRect scrollRect;
    private RectTransform content;
    private int totalItems;
    private List<float> itemHeights = new List<float>();
    private List<float> cumulativeHeights = new List<float>();
    private int visibleItemCount;
    private List<T> items = new List<T>();
    private List<D> datas = new List<D>();
    private float minHeight;

    public void Init()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    private void OnDisable()
    {
        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i]?.ClearData();
            }
        }
    }

    private void CalculateCumulativeHeights()
    {
        for (int i = 0; i < datas.Count; i++)
        {
            float height = GetY(datas[i]);
            itemHeights.Add(height);
        }
        float totalHeight = 0;
        for (int i = 0; i < totalItems; i++)
        {
            cumulativeHeights.Add(totalHeight);
            totalHeight += itemHeights[i];
        }
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
    }

    private void CalculateVisibleItemCount()
    {
        float viewportHeight = scrollRect.viewport.rect.height;
        visibleItemCount = Mathf.CeilToInt(viewportHeight / itemHeights.Average());
        if (visibleItemCount < 2) visibleItemCount = 2;
    }

    private void CreatetIems()
    {
        for (int i = 0; i < visibleItemCount; i++)
        {
            T item = Instantiate(cell.gameObject, content).GetComponent<T>();
            item.gameObject.name = $"ScrollDynamicListItem_{i}";
            items.Add(item);
        }
    }

    private void OnScroll(Vector2 position)
    {
        UpdateVisibleItems();
    }

    void UpdateVisibleItems()
    {
        float scrollPos = content.anchoredPosition.y;
        float viewportHeight = scrollRect.viewport.rect.height;
        float startHeight = 0;
        int startIndex = 0;
        for (int i = 0; i < totalItems; i++)
        {
            if (startHeight + itemHeights[i] > scrollPos)
            {
                startIndex = i;
                break;
            }
            startHeight += itemHeights[i];
        }

        float endHeight = startHeight;
        int endIndex = startIndex;

        for (int i = startIndex; i < totalItems; i++)
        {
            if (endHeight - scrollPos > viewportHeight)
            {
                break;
            }
            endHeight += itemHeights[i];
            endIndex = i;
        }

        for (int i = 0; i < visibleItemCount; i++)
        {
            int itemIndex = startIndex + i;
            if (itemIndex <= endIndex && itemIndex < totalItems)
            {
                RectTransform rt = items[i].gameObject.GetComponent<RectTransform>();
                items[i].gameObject.SetActive(true);
                rt.anchoredPosition = new Vector2(0, -startHeight);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, itemHeights[itemIndex]);
                items[i].SetData(datas[itemIndex]);
                startHeight += itemHeights[itemIndex];
            }
            else
            {
                items[i].gameObject.SetActive(false);
            }
        }
    }

    private int GetItemIndexAtPosition(float scrollPos, int itemOffset)
    {
        float targetPos = scrollPos;
        for (int i = 0; i < cumulativeHeights.Count; i++)
        {
            if (cumulativeHeights[i] + itemHeights[i] > targetPos)
            {
                return i;
            }
        }
        return cumulativeHeights.Count - 1;
    }

    private void UpdateItem(T item, int index)
    {
        RectTransform rt = item.gameObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, itemHeights[index]);
        item.SetData(datas[index]);
    }

    public void UpdateList(List<D> datas)
    {
        if (totalItems <= 0)
        {
            this.datas = datas;
            totalItems = datas.Count;
            CalculateCumulativeHeights();
            CalculateVisibleItemCount();
            CreatetIems();
        }
        UpdateVisibleItems();
    }

    public float GetY(D data)
    {
        int r = 0;
        for (int i = 0; i < data.datas.Count; i++)
        {
            int result = i % data.CellAlignment;
            if (i > 0 && i % data.CellAlignment == 0) r++;
        }
        float sizeY = r * data.Spacing.y + (r + 1) * data.SizeDelta.y;
        return sizeY + data.MarginTop + data.MarginBottom;
    }
}