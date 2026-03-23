using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework;

namespace NewSideGame
{
    public delegate void PoolObjectRecycleD(KBPoolObjectBase unit);

    public abstract class KBPoolObjectBase : ActivatablePoolPrefabBase
    {
        protected bool recyled;
        public event PoolObjectRecycleD OnRecycle;

        public int Index { get; set; }

        public virtual int currentKBobjectFacilityId => 0;

        private List<Timer> timers = new List<Timer>();

        private readonly List<IKBPoolObjectChild> _kbPoolObjectChildren = new List<IKBPoolObjectChild>();

        public override void OnInit(PoolManager ppm)
        {
            base.OnInit(ppm);

            GetComponentsInChildren(true, _kbPoolObjectChildren);

            foreach (var kbPoolObjectChild in _kbPoolObjectChildren)
            {
                kbPoolObjectChild.OnInit(ppm);
            }
        }

        public override void OnSpawn(PoolManager ppm)
        {
            base.OnSpawn(ppm);
            recyled = false;

            foreach (var kbPoolObjectChild in _kbPoolObjectChildren)
            {
                kbPoolObjectChild.OnSpawn(ppm);
            }
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);
            for (int i = 0; i < timers.Count; i++)
            {
                timers[i].Cancel();
            }
            timers.Clear();

            foreach (var kbPoolObjectChild in _kbPoolObjectChildren)
            {
                kbPoolObjectChild.OnDeSpawn(ppm);
            }
        }

        public void AddTimer(Timer t)
        {
            timers.Add(t);
        }

        // public void RemoveTimer(Timer t)
        // {
        //     if (timers.Contains(t))
        //         timers.Remove(t);
        // }

        protected virtual void OnUpdate(float delta)
        {
            foreach (var kbPoolObjectChild in _kbPoolObjectChildren)
            {
                kbPoolObjectChild.OnUpdate(delta);
            }
        }
        protected virtual void TriggerEnter(Collider collision) { }
        protected virtual void TriggerExit(Collider collision) { }
        protected virtual void TriggerStay(Collider collision) { }

        private void Update()
        {
            OnUpdate(Time.deltaTime);
        }
        private void OnTriggerEnter(Collider collision)
        {
            TriggerEnter(collision);
        }

        private void OnTriggerExit(Collider collision)
        {
            TriggerExit(collision);
        }

        private void OnTriggerStay(Collider collision)
        {
            TriggerStay(collision);
        }

        public virtual void DoRecycle()
        {
            if (!recyled)
            {
                OnRecycle?.Invoke(this);
                recyled = true;
            }
        }
    }
}

