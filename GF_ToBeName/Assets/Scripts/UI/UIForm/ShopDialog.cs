using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NewSideGame
{
    public class ShopDialog : BaseDialog
    {
        #region SerializeField

        private UnityEngine.UI.Button m_BtnBack => GetRef<UnityEngine.UI.Button>("BtnBack");
        private ShopItem m_ShopItem => GetRef<ShopItem>("ShopItem");
        private UnityEngine.RectTransform m_Content => GetRef<UnityEngine.RectTransform>("Content");

        #endregion

        public CasualCurrencyGroup m_CasualCurrency;

        #region SerializeField

        private void OnClick_BtnBack()
        {
            Close();
        }

        #endregion


        public List<ShopItem> ShopItemList = new List<ShopItem>();

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (m_CasualCurrency == null)
            {
                m_CasualCurrency =
                    GameEntry.PoolManager.SpawnSync<CasualCurrencyGroup>(CasualCurrencyGroup.CasualAssetId);

                List<ItemType> items = new List<ItemType>() { ItemType.Coin };

                m_CasualCurrency.InitialCurrency(transform, items, new Vector2(80f, -11f));
                m_CasualCurrency.SetDepth(1);
            }

            RefreshUI();

            EventManager.Instance.AddEventListener(Constant.Event.OnRefreshShopDialog, RefreshUI);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            if (m_CasualCurrency != null)
            {
                GameEntry.PoolManager.DeSpawnSync(m_CasualCurrency);
                m_CasualCurrency = null;
            }

            EventManager.Instance.RemoveEventListener(Constant.Event.OnRefreshShopDialog, RefreshUI);

        }

        private void RefreshUI(params object[] args)
        {
            DRShopItem[] _AllInfo = GameEntry.DataTable.GetDataTable<DRShopItem>().GetAllDataRows();

            for (int i = 0; i < ShopItemList.Count; i++)
            {
                ShopItemList[i].UpdateInfo(_AllInfo[i]);
            }
        }

        private int click;
        private DateTime lastClickTime;

        private void OnClick_VerifyUser()
        {
            DateTime now = TimeUtils.GetToday();
            if ((now - lastClickTime).TotalMilliseconds <= 1000)
            {
                click++;
                if (click >= 8)
                {
                    click = 0;
                    UITopCoverForm.Instance.m_DebugBtn.gameObject.SetActive(true);
                }
            }
            else
            {
                click = 0;
            }
            lastClickTime = now;
        }
    }
}