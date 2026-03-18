using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace NewSideGame
{
    public class UIButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] float minScaler = 0.9f;
        [SerializeField] float maxScaler = 1.1f;
        [SerializeField] bool useOneScale = false;

        [HideInInspector] public bool IsReturn;

        Vector3 initScale;

        private void OnEnable()
        {
            initScale = transform.localScale;
            IsReturn = false;
        }

        // OnDisable 里 DOTween.Kill(this) 通常就够了，不需要持有 Sequence
        private void OnDisable()
        {
            // DOTween 会自动处理，或者手动 Kill 该物体相关的 Tween
            transform.DOKill();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsReturn) return;
            
            // 修复：直接对 Transform 做 Tween，而不是用 Sequence 累加
            // 或者先 Kill 当前的动画
            transform.DOKill(); 
            Vector3 scale = useOneScale ? Vector3.one : initScale * maxScaler;
            transform.DOScale(scale, 0.05f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsReturn) return;
            
            transform.DOKill(); // 修复：先停止当前动画
            Vector3 scale = useOneScale ? Vector3.one : initScale;
            transform.DOScale(scale, 0.05f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsReturn) return;
            
            transform.DOKill(); // 修复：先停止当前动画
            Vector3 scale = useOneScale ? Vector3.one : initScale;
            transform.DOScale(scale, 0.05f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsReturn) return;
            
            transform.DOKill(); // 修复：先停止当前动画
            Vector3 scale = useOneScale ? Vector3.one : initScale * minScaler;
            transform.DOScale(scale, 0.05f);
        }
    }
}