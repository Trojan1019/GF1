using System.Collections.Generic;
using UnityGameFramework.Runtime;
using GameFramework;
using System;
using UnityEngine;

namespace NewSideGame
{
    public class EventManager : GameFrameworkComponent
    {
        private static EventManager _instance;
        public static EventManager Instance => _instance;

        public delegate void EventDelegate(params object[] args);
        private Dictionary<int, List<EventEntity>> eventHash = new Dictionary<int, List<EventEntity>>();

        protected override void Awake()
        {
            base.Awake();

            _instance = this;
        }

        internal void AddEventListener(object switchRingLeve, object v)
        {
            throw new NotImplementedException();
        }

        #region 对外事件注册接口        
        /// <summary>
        /// 添加一个事件监听
        /// </summary>
        /// <param name="eventId">eventId,see@EventDefine.cs</param>
        /// <param name="eventFunc">Event delegate </param>
        public void AddEventListener(int eventId, EventDelegate eventFunc)
        {
            this.AddEventListenerImpl(eventId, eventFunc, null);
        }

        /// <summary>
        /// 通过指定一个func名称删除一个事件监听
        /// </summary>
        /// <param name="eventId">eventId,see@EventDefine.cs</param>
        /// <param name="eventFunc">Event delegate </param>
        public void RemoveEventListener(int eventId, EventDelegate eventFunc)
        {
            this.RemoveEventListenerImpl(eventId, eventFunc, null);
        }

        /// <summary>
        /// 添加一个ui事件监听;
        public void AddUIEventListener(UGuiForm uiGui, int eventId, EventDelegate eventFunc)
        {
            this.AddEventListenerImpl(eventId, eventFunc, uiGui);
        }

        /// <summary>
        /// 通过指定一个(uiGui,eventFunc)删除一个事件监听
        /// 注意：目前在lua层面没有面向对象概念，eventFunc应该是同一个；
        /// </summary>
        public void RemoveUIEventListener(UGuiForm uiGui, int eventId, EventDelegate eventFunc)
        {
            this.RemoveEventListenerImpl(eventId, eventFunc, uiGui);
        }
        #endregion


        #region 内部实现（添加／删除事件相关）

        private void AddEventListenerImpl(int eventId, EventDelegate eventFunc, UGuiForm uiGui)
        {
            List<EventEntity> eventlist = null;

            if (eventHash.ContainsKey(eventId))
            {
                eventlist = eventHash[eventId];
            }
            else
            {
                eventlist = new List<EventEntity>();
                eventHash[eventId] = eventlist;
            }

            if (eventlist == null)
            {
                return;
            }

            if (IsHasEventDelegate(eventlist, eventFunc, uiGui))
            {
                Log.Warning(Utility.Text.Format("重复添加事件监听,eventId={0},eventFunc = {1},uiGui = ", eventId, eventFunc, uiGui));
                return;
            }

            eventlist.Add(new EventEntity(eventFunc, uiGui));
        }


        private void RemoveEventListenerImpl(int eventId, EventDelegate eventFunc, UGuiForm uiGui)
        {
            if (!eventHash.ContainsKey(eventId))
            {
                return;
            }

            List<EventEntity> _eventlist = eventHash[eventId];

            if (_eventlist == null)
            {
                return;
            }
            RemoveEventDelegate(eventId, _eventlist, eventFunc, uiGui);
            // bool flag = this.RemoveEventDelegate(eventId, _eventlist, eventFunc, uiGui);
        }
        #endregion

        #region 事件通知接口
        /// <summary>
        /// 事件通知
        /// </summary>
        /// <param name="eventId">eventId,see@EventDefine.cs</param>
        /// <param name="obj">事件的参数</param>
        public void NotifyEvent(int eventId, params object[] args)
        {
            if (eventHash.ContainsKey(eventId))
            {
                List<EventEntity> _eventlist = eventHash[eventId];
                if (_eventlist == null)
                {
                    return;
                }
                for (int i = _eventlist.Count - 1; i >= 0; i--)
                {
                    EventEntity eventEntity = _eventlist[i];
                    if (eventEntity == null)
                    {
                        continue;
                    }
                    EventDelegate _dele = eventEntity.func;
                    if (_dele != null)
                    {
                        _dele(args);
                    }
                }
            }
        }
        #endregion


        #region 内部事件列表操作方法
        private bool IsHasEventDelegate(List<EventEntity> eventlist, EventDelegate eventFunc, UGuiForm uiGui)
        {
            if (eventlist == null)
            {
                return false;
            }

            for (int i = 0; i < eventlist.Count; i++)
            {
                EventEntity eventEntity = eventlist[i];
                if (eventEntity == null)
                {
                    continue;
                }
                if (uiGui != null)
                {
                    if (eventEntity.uiGui != uiGui)
                    {
                        continue;
                    }
                }

                if (eventEntity.func == eventFunc)
                {
                    return true;
                }
            }
            return false;
        }

        private bool RemoveEventDelegate(int eventId, List<EventEntity> eventlist, EventDelegate eventFunc, UGuiForm uiGui)
        {
            if (eventlist == null)
            {
                return false;
            }

            for (int i = (eventlist.Count - 1); i >= 0; i--)
            {
                EventEntity eventEntity = eventlist[i];
                if (eventEntity == null)
                {
                    continue;
                }

                if (uiGui != null)
                {
                    if (eventEntity.uiGui != uiGui)
                    {
                        continue;
                    }
                }

                if (eventEntity.func == eventFunc)
                {
                    eventlist.RemoveAt(i);
                    return true;
                }
            }

            // Log.Warning( Utility.Text.Format("－－－－－－－》要删除的事件监听不存在eventId= {0} ",eventId));
            return false;
        }
        #endregion
    }


    public class EventEntity
    {
        public EventManager.EventDelegate func;
        public UGuiForm uiGui;

        public EventEntity(EventManager.EventDelegate _func, UGuiForm _uiGui)
        {
            this.func = _func;
            this.uiGui = _uiGui;
        }
    }

}