using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NewSideGame
{
    public class SettingGMButton : Button
    {
        private TextMeshProUGUI _fpsText;
        private RectTransform _rectTransformParent;
        private float _startTime = 0;
        private bool _pointDown = false;
        private RectTransform _rectTransform;
        private Vector2 _clickScreenPosition;

        private float m_UpdateInterval = 0.5f;
        private float m_CurrentFps;
        private int m_Frames;
        private float m_Accumulator;

        private float m_TimeLeft;

        private void ResetButton()
        {
            m_CurrentFps = 0f;
            m_Frames = 0;
            m_Accumulator = 0f;
            m_TimeLeft = 0f;
        }

        protected override void Awake()
        {
            base.Awake();

            _rectTransform = GetComponent<RectTransform>();
            _rectTransformParent = transform.parent.GetComponent<RectTransform>();
            _fpsText = GetComponentInChildren<TextMeshProUGUI>();

            ResetButton();
        }

        private void OnClick_Button()
        {
            if (GameEntry.UI.GetUIForm(UIFormType.DebugDialog) != null)
            {
                GameEntry.UI.CloseUIForm(UIFormType.DebugDialog);
            }
            else
            {
                GameEntry.UI.OpenUIForm(UIFormType.DebugDialog);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            _pointDown = true;
            _startTime = 0;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            _pointDown = false;

            if (IsClick)
            {
                OnClick_Button();
            }
        }

        private bool IsClick => _startTime < 0.2f;

        private void Update()
        {
            m_Frames++;
            m_Accumulator += Time.unscaledDeltaTime;
            m_TimeLeft -= Time.unscaledDeltaTime;

            if (m_TimeLeft <= 0f)
            {
                m_CurrentFps = m_Accumulator > 0f ? m_Frames / m_Accumulator : 0f;
                m_Frames = 0;
                m_Accumulator = 0f;
                m_TimeLeft += m_UpdateInterval;

                RefreshFPSText();
            }

            if (!_pointDown) return;

            _startTime += Time.unscaledDeltaTime;

            if (IsClick) return;

            if (ReferenceEquals(GameEntry.UI.UICamera, null)) return;

            _clickScreenPosition = Input.mousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransformParent, _clickScreenPosition,
                GameEntry.UI.UICamera, out Vector2 localPoint);

            _rectTransform.localPosition = localPoint;
        }

        private void RefreshFPSText()
        {
            _fpsText.text = $"{(int)m_CurrentFps}";
        }
    }
}