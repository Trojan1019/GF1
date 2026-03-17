using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameFramework;

namespace NewSideGame
{
    public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private bool _interactable = true;
        public bool interactable
        {
            get { return _interactable; }
            set
            {
                _interactable = value;
                if (m_ScrollRect != null) m_ScrollRect.horizontal = interactable;
            }
        }

        [Tooltip("页面移动速率")]
        public float smooting = 8f;
        [Tooltip("划过多少就算成功")]
        [Range(0.1f, 1f)]
        public float limit = 0.3f;


        public bool autoPlay = false;
        public float autoPlaySeconds = 5f;


        //本游戏对象上的ScrollRect组件
        private ScrollRect m_ScrollRect;

        private int totalPage = 0; //当前已创建的页面数量

        public int selectPage = 2; //当前选中的页面

        private bool isDrag = false;  //当前是否正在拖拽

        private float targetHorizontalPosition;  //当前目标水平位置

        private Vector2 rectSize = Vector2.zero; //窗口大小

        private GameFrameworkAction<int> OnPageScrolled;
        private GameFrameworkAction OnBeginDragCallback;
        private GameFrameworkAction OnRefreshCallback;


        void Awake()
        {
            m_ScrollRect = GetComponent<ScrollRect>();
            if (m_ScrollRect == null || !m_ScrollRect.enabled)
            {
                Debug.LogWarning("PageView Component Need Activity ScrollRect !");
                enabled = false;
                return;
            }
            //ScrollRect初始化设置
            m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;
            m_ScrollRect.vertical = false;
            m_ScrollRect.horizontal = _interactable;
            //重新给Content添加SizeFitter和Layout组件
            DestroyImmediate(m_ScrollRect.content.gameObject.GetComponent<ContentSizeFitter>());
            DestroyImmediate(m_ScrollRect.content.gameObject.GetComponent<HorizontalLayoutGroup>());
            var size_fitter = m_ScrollRect.content.gameObject.AddComponent<ContentSizeFitter>();
            var layout_group = m_ScrollRect.content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout_group.childControlHeight = true;
            layout_group.childForceExpandHeight = true;
            layout_group.childControlWidth = false;
            layout_group.childForceExpandWidth = false;
            layout_group.childAlignment = TextAnchor.MiddleLeft;
            size_fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            InitializationPage();
            m_ScrollRect.content.localPosition = new Vector3(targetHorizontalPosition, 0, 0);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_ScrollRect.GetComponent<RectTransform>());
        }

        // Update is called once per frame
        float count = 0;
        void Update()
        {
            //检查是否有节点变动(只通过节点数量判定,如果手动改变了节点<数量不变>,需要手动调用以下方法)
            if (totalPage != m_ScrollRect.content.childCount)
            {
                InitializationPage();
            }
            //没有拖拽时
            if (!isDrag && totalPage > 0)
            {
                //m_ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(m_ScrollRect.horizontalNormalizedPosition, targetHorizontal, Time.deltaTime * smooting);
                m_ScrollRect.content.localPosition = new Vector3(Mathf.Lerp(m_ScrollRect.content.localPosition.x, targetHorizontalPosition, Time.deltaTime * smooting), 0, 0);
            }
            //自动播放
            if (autoPlay && totalPage > 0)
            {
                if (!isDrag)
                {
                    count += Time.deltaTime;
                    if (count >= autoPlaySeconds)
                    {
                        count = 0f;
                        MoveLeft();
                    }
                }
                else
                {
                    count = 0f;
                }
            }
        }

        /// <summary>
        /// 如果手动改变了节点(数量不变),需要手动调用此方法 , 且不能有Active=false和非RectTransform的节点
        /// \n此方法只能在实例化出来第2帧后调用,否则可能获取不到窗口大小
        /// </summary>
        public void InitializationPage()
        {
            //获取窗口大小
            rectSize = m_ScrollRect.viewport.rect.size;
            if (rectSize.x == 0 || rectSize.y == 0) return;
            //初始化子节点
            foreach (RectTransform trf in m_ScrollRect.content)
            {
                trf.gameObject.SetActive(true);
                trf.sizeDelta = rectSize;
            }
            totalPage = m_ScrollRect.content.childCount;
            Compute();
            //回调方法
            OnRefreshCallback?.Invoke();
        }

        /// <summary>
        /// 向左移动,如果已经是最后一个,则循环到第一个
        /// </summary>
        public void MoveLeft(bool loop = true)
        {
            selectPage += 1;
            if (loop && selectPage > totalPage) selectPage = 1;
            Compute();
        }

        /// <summary>
        /// 向右移动,如果已经是第一个,则循环到最后一个
        /// </summary>
        public void MoveRight(bool loop = true)
        {
            selectPage -= 1;
            if (loop && selectPage <= 0) selectPage = totalPage;
            Compute();
        }

        public void MoveTo(int index)
        {
            selectPage = index;
            Compute();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("Begin");
            isDrag = true;

            OnBeginDragCallback?.Invoke();
        }

        public void SetOnBeginDrag(GameFrameworkAction callBack)
        {
            OnBeginDragCallback = callBack;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("End");
            isDrag = false;

            //窗口大小
            if (rectSize.x == 0 || rectSize.y == 0) return;
            //计算滑动结果
            var offsetWidth = -(selectPage - 1) * rectSize.x - m_ScrollRect.content.localPosition.x;//实际位置与预想位置差值
            if (Mathf.Abs(offsetWidth) > rectSize.x * limit)
            {
                //向右滑动
                if (offsetWidth < 0)
                {
                    selectPage -= 1;
                }
                //向左滑动
                else
                {
                    selectPage += 1;
                }
                Compute();
                // 回调
                OnPageScrolled?.Invoke(selectPage);
            }
        }

        //计算targetHorizontalPosition的值
        private void Compute()
        {
            //窗口大小
            if (rectSize.x == 0 || rectSize.y == 0) return;
            //计算值
            selectPage = totalPage <= 0 ? 0 : selectPage > totalPage ? totalPage : selectPage <= 0 ? 1 : selectPage;
            targetHorizontalPosition = -(selectPage - 1) * rectSize.x;
        }

        public int GetCurPageIndex()
        {
            return selectPage;
        }

        public void SetOnPageScrolled(GameFrameworkAction<int> callBack)
        {
            OnPageScrolled = callBack;
        }

        public void SetOnRefreshCallback(GameFrameworkAction callBack)
        {
            OnRefreshCallback = callBack;
        }
    }
}