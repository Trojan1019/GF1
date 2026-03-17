using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;

public interface IScrollDynamicListItem<T>
{
    GameObject gameObject { get; }
    void SetData(T data);
    void ClearData();
}

public interface IScrollDynamicReference<T>
{
    /// <summary>
    /// 每行展示子元素数量
    /// </summary>
    /// <value></value>
    int CellAlignment { get; }

    /// <summary>
    /// 子元素间距
    /// </summary>
    /// <value></value>
    Vector2 Spacing { get; }

    /// <summary>
    /// 子元素大小
    /// </summary>
    /// <value></value>
    Vector2 SizeDelta { get; }

    /// <summary>
    /// anchorMin和anchorMax(0,1)时x,y的偏移量
    /// </summary>
    /// <value></value>
    Vector2 Offset { get; }

    /// <summary>
    /// 子元素和父级元素顶部距离（存在有标题情况）
    /// </summary>
    /// <value></value>
    float MarginTop { get; }

    /// <summary>
    /// 父级元素底部距离
    /// </summary>
    /// <value></value>
    float MarginBottom { get; }
}