using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public class BetterScrollList<T, D> : MonoBehaviour where T : IBetterScrollListItem<D>
    {
        //将模板cell的GameObject的节点拉到这里。
        public GameObject tempCell;

        ScrollGridVertical scrollGridVertical;

        public virtual ScrollRect.MovementType movementType
        {
            get => ScrollRect.MovementType.Elastic;
        }

        public virtual float elasticity
        {
            get => 0.1f;
        }

        public virtual float scrollSensitivity
        {
            get => 1f;
        }

        public virtual float decelerationRate
        {
            get => 0.135f;
        }

        public virtual float spacing
        {
            get => 8;
        }

        public virtual Vector4 margin
        {
            get => new Vector4(20, 0, 20, 20);
        }

        public virtual float paddingBottom
        {
            get => 0;
        }

        public virtual int cellAlignment
        {
            get => 0;
        }

        public virtual float threshold
        {
            get => 0.1f; // 滚动到底部时的阈值，0.1表示10%
        }

        // 是否额外添加Mask2D
        public virtual bool addRectMask2D => false;

        [HideInInspector] public List<D> list;

        public void Init(Action onScroll = null)
        {
            scrollGridVertical = gameObject.AddComponent<ScrollGridVertical>();
            scrollGridVertical.SetMovementType(movementType);
            scrollGridVertical.SetElasticity(elasticity);
            scrollGridVertical.SetScrollSensitivity(scrollSensitivity);
            scrollGridVertical.SetDecelerationRate(decelerationRate);
            if (addRectMask2D)
                scrollGridVertical.addRectMask2D = addRectMask2D;
            //步骤一：设置模板cell。
            scrollGridVertical.tempCell = tempCell;
            scrollGridVertical.cellAlignment = cellAlignment;
            //步骤二:设置cell刷新的事件监听。
            scrollGridVertical.AddCellListener(this.OnCellUpdate);
            //步骤三：设置间距。
            scrollGridVertical.SetItemSpacing(spacing);
            scrollGridVertical.SetMargin(margin.x, margin.y, margin.z, margin.w);
            scrollGridVertical.SetPaddingBottom(paddingBottom);
            scrollGridVertical.SetThreshold(threshold, OnScrollBottom);
            scrollGridVertical.SetScrollPos(OnScrollPos);
            //步骤四：更新数据
            UpdateList();

            scrollGridVertical.onScroll = onScroll;
        }

        public void UpdateMask(bool showMask)
        {
            //特殊操作
            //目前撑开的布局方式初始化mask有问题，需要通过重新enable的解决
            if (gameObject != null && gameObject.GetComponent<ScrollRect>() != null)
            {
                Mask mask = gameObject.GetComponent<ScrollRect>().viewport.gameObject.GetComponent<Mask>();
                if (mask != null)
                {
                    mask.enabled = showMask;
                    mask.enabled = !mask.enabled;
                }
            }
        }

        /// <summary>
        /// 监听cell的刷新消息，修改cell的数据。
        /// </summary>
        /// <param name="cell"></param>
        public virtual void OnCellUpdate(ScrollGridCell cell)
        {
            if (cell.index >= list.Count) return;

            T item = cell.GetComponent<T>();
            item.SetData(list[cell.index]);
        }

        public virtual void OnScrollBottom(bool result)
        {
        }

        public virtual void OnScrollPos(Vector2 pos)
        {
        }

        public void UpdateList(List<D> list,bool showMask = true, bool updateMask = false)
        {
            this.list = list;
            UpdateList();

            if (updateMask)
            {
                UpdateMask(showMask);
            }
        }

        public void UpdateList()
        {
            if (scrollGridVertical != null && list != null)
            {
                //如果数据有新的变化，重新直接设置即可。
                float curMinY = 0;
                scrollGridVertical.SetCellCount(list.Count, ref curMinY);
            }
        }

        public void UpdateListWithScrollTo(List<D> list, int targetIdx)
        {
            this.list = list;
            if (scrollGridVertical != null && list != null)
            {
                float curMinY = 0;
                scrollGridVertical.SetCellCount(list.Count, ref curMinY);
                scrollGridVertical.ScrollToIndex(targetIdx, false, curMinY);
            }
        }

        public void ScrollToItem(int index)
        {
            scrollGridVertical.ScrollToIndex(index);
        }

        public virtual void ScrollToTop()
        {
            scrollGridVertical.ScrollToIndex(0);
        }

        public void SrcollToBottom()
        {
            scrollGridVertical.ScrollToIndex(list.Count - 1);
        }

        public void DelayScrollToItem(int index, float delay)
        {
            StartCoroutine(IDelayScrollToItem(index, delay));
        }

        public IEnumerator IDelayScrollToItem(int index, float delay)
        {
            yield return YieldHepler.WaitForSeconds(delay);
            scrollGridVertical.ScrollToIndex(index);
        }

        public Mask GetViewPortMask()
        {
            if (scrollGridVertical != null)
                return scrollGridVertical.viewportMask;
            else
                return null;
        }

        public void RefershAll()
        {
            if (scrollGridVertical != null)
                scrollGridVertical.RefreshAllCells();
        }

        public List<GameObject> GetAllCells()
        {
            if (scrollGridVertical != null)
                return scrollGridVertical.GetAllCells();

            return null;
        }

        public void ControlScroll(bool canScroll)
        {
            if (scrollGridVertical != null)
                scrollGridVertical.ControlScroll(canScroll);
        }

        public void SetScrollMargin(Vector4 margin)
        {
            if (scrollGridVertical != null)
                scrollGridVertical.SetScrollMargin(margin);
        }
    }
}