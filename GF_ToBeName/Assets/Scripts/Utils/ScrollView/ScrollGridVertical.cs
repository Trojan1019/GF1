using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollGridVertical : MonoBehaviour
{
    public GameObject tempCell;//模板cell，以此为目标，克隆出每个cell。
    public GameObject scrollbar;
    [HideInInspector]
    public Mask viewportMask;
    private int cellCount;//要显示数据的总数。
    private float cellWidth;
    private float cellHeight;
    private float cellOffsetX;

    private List<System.Action<ScrollGridCell>> onCellUpdateList = new List<System.Action<ScrollGridCell>>();
    private ScrollRect scrollRect;
    private ScrollRect.MovementType movementType = ScrollRect.MovementType.Elastic;
    private float elasticity = 0.1f;
    private float scrollSensitivity = 1f;
    private float decelerationRate = 0.135f;

    private int row;//克隆cell的GameObject数量的行。
    private int col;//克隆cell的GameObject数量的列。
    [HideInInspector]
    public int cellAlignment = 0;//0:左对齐，1居中对齐
    [HideInInspector]
    public bool addRectMask2D = false;//添加Mask2D

    private List<GameObject> cellList = new List<GameObject>();
    private bool inited;

    public Action onScroll;
    public void AddCellListener(System.Action<ScrollGridCell> call)
    {
        this.onCellUpdateList.Add(call);
        //this.RefreshAllCells();
    }
    public void RemoveCellListener(System.Action<ScrollGridCell> call)
    {
        this.onCellUpdateList.Remove(call);
    }

    public void SetMovementType(ScrollRect.MovementType type)
    {
        this.movementType = type;
    }

    public void SetElasticity(float value)
    {
        this.elasticity = value;
    }

    public void SetScrollSensitivity(float value)
    {
        this.scrollSensitivity = value;
    }

    public void SetDecelerationRate(float value)
    {
        this.decelerationRate = value;
    }

    private float spacing = 20; //默认item间距
    private float marginLeft = 0, marginRight = 0, marginBottom = 0, marginTop = 0;
    private float paddingBottom = 0;
    private float threshold = 0.1f;
    private System.Action<bool> onScrollBottom = null;
    private System.Action<Vector2> OnScrollPos = null;

    /// <summary>
    /// 设置Item间间距。
    /// </summary>
    public void SetItemSpacing(float spacing)
    {
        this.spacing = spacing;
    }

    public void SetMargin(float left, float right, float top, float bottom)
    {
        marginLeft = left;
        marginRight = right;
        marginBottom = bottom;
        marginTop = top;
    }

    public void SetPaddingBottom(float bottom)
    {
        paddingBottom = bottom;
    }

    public void SetThreshold(float threshold, System.Action<bool> OnScrollBottom)
    {
        this.threshold = threshold;
        this.onScrollBottom = OnScrollBottom;
    }

    public void SetScrollPos(System.Action<Vector2> OnScrollPos)
    {
        this.OnScrollPos = OnScrollPos;
    }

    /// <summary>
    /// 设置ScrollGrid要显示的数据数量。
    /// </summary>
    /// <param name="count"></param>
    public void SetCellCount(int count, ref float minY)
    {
        this.cellCount = Mathf.Max(0, count);

        if (this.inited == false)
        {
            this.Init();
        }
        //重新调整content的高度，保证能够包含范围内的cell的anchoredPosition，这样才有机会显示。
        float newContentHeight = this.cellHeight * Mathf.CeilToInt((float)cellCount / this.col) + paddingBottom;
        float newMinY = -newContentHeight + this.scrollRect.viewport.rect.height;
        float maxY = this.scrollRect.content.offsetMax.y;
        newMinY += maxY;//保持位置
        newMinY = Mathf.Min(maxY, newMinY);//保证不小于viewport的高度。
        this.scrollRect.content.offsetMin = new Vector2(0, newMinY);
        this.CreateCells();

        minY = newMinY;
    }

    private void Init()
    {
        if (tempCell == null)
        {
            Debug.LogError("tempCell不能为空！");
            return;
        }
        this.inited = true;
        this.tempCell.SetActive(false);

        //创建ScrollRect下的viewpoint和content节点。
        this.scrollRect = gameObject.AddComponent<ScrollRect>();
        this.scrollRect.vertical = true;
        this.scrollRect.horizontal = false;
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(transform);
        this.scrollRect.viewport = viewport.GetComponent<RectTransform>();
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform);
        this.scrollRect.content = content.GetComponent<RectTransform>();

        //加入滚动条
        if (scrollbar != null)
        {
            GameObject scrollBar = GameObject.Instantiate<GameObject>(this.scrollbar);
            scrollBar.transform.SetParent(transform);
            scrollBar.transform.localScale = new Vector3(1, 1, 1);
            scrollBar.GetComponent<RectTransform>().offsetMax = new Vector2(scrollBar.GetComponent<RectTransform>().offsetMax.x, -50);
            scrollBar.GetComponent<RectTransform>().offsetMin = new Vector2(scrollBar.GetComponent<RectTransform>().offsetMin.x, 50);
            this.scrollRect.verticalScrollbar = scrollBar.GetComponent<Scrollbar>();
        }

        //设置视野viewport的宽高和根节点一致。
        this.scrollRect.viewport.localScale = Vector3.one;
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        this.scrollRect.viewport.anchorMin = Vector2.zero;
        this.scrollRect.viewport.anchorMax = Vector2.one;
        this.scrollRect.viewport.offsetMax = new Vector2(-marginRight, -marginTop);
        this.scrollRect.viewport.offsetMin = new Vector2(marginLeft, marginBottom);
        this.scrollRect.viewport.anchoredPosition3D = new Vector3(
            this.scrollRect.viewport.anchoredPosition.x,
            this.scrollRect.viewport.anchoredPosition.y, 0);

        //设置viewpoint的mask。
        viewportMask = this.scrollRect.viewport.gameObject.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        if (addRectMask2D)
            this.scrollRect.viewport.gameObject.AddComponent<RectMask2D>();
        Image image = this.scrollRect.viewport.gameObject.AddComponent<Image>();
        Rect viewRect = this.scrollRect.viewport.rect;
        image.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(Vector2.zero, Vector2.one), Vector2.zero);

        //获取模板cell的宽高。
        Rect tempRect = tempCell.GetComponent<RectTransform>().rect;
        this.cellWidth = tempRect.width;
        this.cellHeight = tempRect.height + spacing;

        //设置viewpoint约束范围内的cell的GameObject的行列数。
        if (this.cellWidth == 0)
        {
            this.col = 1;
        }
        else
        {
            this.col = (int)(this.scrollRect.viewport.rect.width / this.cellWidth);
        }
        this.col = Mathf.Max(1, this.col);
        this.row = Mathf.CeilToInt(this.scrollRect.viewport.rect.height / this.cellHeight);
        this.cellOffsetX = (this.scrollRect.viewport.rect.width - this.cellWidth * this.col) / 2;

        //设置滚动模式
        this.scrollRect.movementType = movementType;
        this.scrollRect.elasticity = elasticity;
        this.scrollRect.scrollSensitivity = scrollSensitivity;
        this.scrollRect.decelerationRate = decelerationRate;

        //初始化content。
        this.scrollRect.content.localScale = Vector3.one;
        this.scrollRect.content.offsetMax = new Vector2(0, 0);
        this.scrollRect.content.offsetMin = new Vector2(0, 0);
        this.scrollRect.content.anchorMin = Vector2.zero;
        this.scrollRect.content.anchorMax = Vector2.one;
        this.scrollRect.onValueChanged.AddListener(this.OnValueChange);
        // this.CreateCells();

    }
    /// <summary>
    /// 刷新每个cell的数据
    /// </summary>
    public void RefreshAllCells()
    {
        foreach (GameObject cell in this.cellList)
        {
            this.cellUpdate(cell);
        }
    }

    public List<GameObject> GetAllCells()
    {
        return this.cellList;
    }

    public void RefershSingle(int index)
    {
        if (index >= 0 && index <= this.cellList.Count - 1)
            this.cellUpdate(this.cellList[index]);
    }

    /// <summary>
    /// 创建每个cell，并且根据行列定它们的位置，最多创建能够在视野范围内看见的个数，加上一行隐藏待进入视野的cell。
    /// </summary>
    private void CreateCells()
    {
        for (int r = 0; r < this.row + 1; r++)
        {
            for (int l = 0; l < this.col; l++)
            {
                int index = r * this.col + l;
                if (index < this.cellCount)
                {
                    if (this.cellList.Count <= index)
                    {
                        GameObject newcell = GameObject.Instantiate<GameObject>(this.tempCell);
                        newcell.SetActive(true);
                        RectTransform cellRect = newcell.GetComponent<RectTransform>();
                        float width = cellRect.sizeDelta.x;

                        //cell节点锚点强制设为左上角，以此方便算出位置。
                        if (width != 0)
                        {
                            cellRect.anchorMin = new Vector2(0, 1);
                            cellRect.anchorMax = new Vector2(0, 1);
                        }

                        //分别算出每个cell的位置。
                        float offsetX = (cellAlignment == 1) ? cellOffsetX : 0;
                        float x = this.cellWidth / 2 + l * this.cellWidth + offsetX;
                        float y = -r * this.cellHeight - this.cellHeight / 2;// - (index * 20);
                        cellRect.SetParent(this.scrollRect.content);
                        if (width == 0)
                        {
                            cellRect.offsetMax = new Vector2(0, cellRect.offsetMax.y);
                            cellRect.offsetMin = new Vector2(0, cellRect.offsetMin.y);
                        }
                        cellRect.localScale = Vector3.one;

                        cellRect.anchoredPosition = new Vector3(x, y);
                        newcell.AddComponent<ScrollGridCell>().SetObjIndex(index);
                        this.cellList.Add(newcell);
                    }
                }
            }
        }
        this.RefreshAllCells();
    }

    /// <summary>
    /// 滚动过程中，重复利用cell
    /// </summary>
    /// <param name="pos"></param>
    public void OnValueChange(Vector2 pos)
    {
        foreach (GameObject cell in this.cellList)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.scrollRect.content.offsetMax.y + cellRect.anchoredPosition.y;
            float maxTop = this.cellHeight / 2;
            float minBottom = -((this.row + 1) * this.cellHeight) + this.cellHeight / 2;
            if (dist > maxTop)
            {
                float newY = cellRect.anchoredPosition.y - (this.row + 1) * this.cellHeight;
                //保证cell的anchoredPosition只在content的高的范围内活动，下同理
                if (newY > -this.scrollRect.content.rect.height)
                {
                    //重复利用cell，重置位置到视野范围内。
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    this.cellUpdate(cell, true);
                }

            }
            else if (dist < minBottom)
            {
                float newY = cellRect.anchoredPosition.y + (this.row + 1) * this.cellHeight;
                if (newY < 0)
                {
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    this.cellUpdate(cell, true);
                }
            }
        }

        onScrollBottom?.Invoke(this.scrollRect.vertical && pos.y <= threshold);
        OnScrollPos?.Invoke(pos);
    }

    /// <summary>
    /// 自动滚动到某个位置
    /// </summary>
    public void ScrollToIndex(int index, bool forceScroll = true, float curMinY = 0)
    {
        float total = scrollRect.content.offsetMax.y - scrollRect.content.offsetMin.y;
        this.scrollRect.content.offsetMax = new Vector2(0, index * this.cellHeight);
        this.scrollRect.content.offsetMin = new Vector2(0, -(total - this.scrollRect.content.offsetMax.y));

        if (!forceScroll && (curMinY == this.scrollRect.content.offsetMin.y))
        {
            //Debug.Log("==> no need to scroll");
            return;
        }
        else
        {
            //Debug.Log($"==> scroll to {index}, [{curMinY}, {this.scrollRect.content.offsetMin.y}]");
        }

        for (int i = 0; i < this.cellList.Count; i++)
        {
            RectTransform cellRect = cellList[i].GetComponent<RectTransform>();
            float newY = -(this.scrollRect.content.offsetMax.y + this.cellHeight / 2) - (i / this.col) * this.cellHeight + spacing / 2;

            cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
            this.cellUpdate(cellList[i]);
        }
    }

    /// <summary>
    /// 所有的数据的真实行数
    /// </summary>
    private int allRow { get { return Mathf.CeilToInt((float)this.cellCount / this.col); } }

    /// <summary>
    /// cell被刷新时调用，算出cell的位置并调用监听的回调方法（Action）。
    /// </summary>
    /// <param name="cell"></param>
    private void cellUpdate(GameObject cell, bool fromScroll = false)
    {
        RectTransform cellRect = cell.GetComponent<RectTransform>();
        float offsetX = (cellAlignment == 1) ? cellOffsetX : 0;
        int x = Mathf.CeilToInt((cellRect.anchoredPosition.x - this.cellWidth / 2 - offsetX) / this.cellWidth);
        if (this.cellWidth == 0)
            x = 0;
        if (x >= this.col)
            x = this.col - 1;
        float v = (cellRect.anchoredPosition.y + this.cellHeight / 2) / this.cellHeight;
        int y = Mathf.Abs(Mathf.RoundToInt(v));
        int index = y * this.col + x;
        ScrollGridCell scrollGridCell = cell.GetComponent<ScrollGridCell>();
        scrollGridCell.UpdatePos(x, y, index);
        if (index >= cellCount || y >= this.allRow)
        {
            // 超出数据范围
            cell.SetActive(false);
        }
        else
        {
            if (cell.activeSelf == false)
            {
                cell.SetActive(true);
            }
            foreach (var call in this.onCellUpdateList)
            {
                call(scrollGridCell);
            }
        }

        if (fromScroll)
        {
            onScroll?.Invoke();
        }
    }

    public void ControlScroll(bool canScroll)
    {
        if (this.scrollRect != null)
        {
            this.scrollRect.vertical = canScroll;
        }
    }

    public void SetScrollMargin(Vector4 margin)
    {
        if (this.scrollRect != null)
        {
            this.scrollRect.viewport.offsetMax = new Vector2(-margin.y, -margin.z);
            this.scrollRect.viewport.offsetMin = new Vector2(margin.x, margin.w);
        }
    }
}
