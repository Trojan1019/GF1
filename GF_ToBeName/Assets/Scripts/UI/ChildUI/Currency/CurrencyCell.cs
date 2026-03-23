using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace NewSideGame
{
    public class CurrencyCell : MonoBehaviour
    {
        private int m_type;
        public int CurrencyType => m_type;

        [SerializeField] private TextMeshProUGUI m_currenty;
        [SerializeField] public Image m_icon;


        private Tweener tweener;
        private int currentCurrencyCount = 0;
        private bool inited;
        private Sequence _sequence;

        public void Init()
        {
            currentCurrencyCount = ProxyManager.UserProxy.ItemCount((int)m_type);
            m_currenty.text = currentCurrencyCount.ToString();
            inited = true;
        }

        public void OnCurrencyChange()
        {
            if (!object.ReferenceEquals(tweener, null) && tweener.active)
            {
                tweener.Kill(false);
                tweener = null;
            }
            int oldValue = currentCurrencyCount;
            int newValue = ProxyManager.UserProxy.ItemCount(m_type);

            if (oldValue == newValue) return;

            tweener = DOTween.To(() => oldValue, (int curValue) =>
            {
                m_currenty.text = curValue.ToString("F0");
                currentCurrencyCount = curValue;
            }, newValue, 0.45f).SetEase(Ease.Linear);
        }

        public Sequence ShowAnimation()
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
            }

            if (_sequence.IsActive())
            {
                _sequence.Kill();
                canvasGroup.DOKill();
            }

            _sequence = DOTween.Sequence();

            gameObject.SetActive(true);
            _sequence.Append(canvasGroup.DOFade(1, 0.3f));

            return _sequence;
        }

        public Sequence HideAnimation(float interval = 1f)
        {
            if (_sequence.IsActive()) return null;

            CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            _sequence = DOTween.Sequence();

            _sequence.AppendInterval(interval);
            _sequence.Append(canvasGroup.DOFade(0, 0.3f));
            _sequence.OnComplete(() => { gameObject.SetActive(false); });

            return _sequence;
        }

        public void SetData(int itemId)
        {
            this.m_type = itemId;
            ItemData itemData = new ItemData(itemId);
            itemData.SetSprite(m_icon);

            currentCurrencyCount = ProxyManager.UserProxy.ItemCount(itemId);
            m_currenty.text = currentCurrencyCount.ToString();

            Init();
        }

        public void SetDataWithNum(int itemId, int num)
        {
            this.m_type = itemId;
            m_icon.sprite = SpriteAtlasManager.Instance.GetSprite(SpriteAtlasId.Item, itemId.ToString());
            currentCurrencyCount = num;
            m_currenty.text = currentCurrencyCount.ToString();

            Init();
        }

        public void ClearData()
        {
            m_type = -1;
            if (tweener.IsActive())
            {
                tweener.Kill(false);
                tweener = null;
            }
        }

        public bool IsAnimation => _sequence.IsActive();
    }
}