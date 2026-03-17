using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace NewSideGame
{
    public class UIHomeForm : UGuiForm
    {
        [HideInInspector] public CasualCurrencyGroup m_CasualCurrency;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // if (m_CasualCurrency == null)
            // {
            //     m_CasualCurrency = GameEntry.PoolManager.SpawnSync<CasualCurrencyGroup>(CasualCurrencyGroup.CasualAssetId);
            //     List<ItemType> items = new List<ItemType>() { ItemType.Coin };
            //     m_CasualCurrency.InitialCurrency(transform, items);
            //     m_CasualCurrency.SetDepth(1);
            // }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            // if (m_CasualCurrency != null)
            // {
            //     GameEntry.PoolManager.DeSpawnSync(m_CasualCurrency);
            //     m_CasualCurrency = null;
            // }
        }
    }
}