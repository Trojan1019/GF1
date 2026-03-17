using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewSideGame
{
    public enum ShopItemType
    {
        Single = 0,
        CoinPack = 1,
        NoAdPack = 2,
        NoAdSingle = 3,
        FreePack = 4,
    }

    public class ShopItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Desc;
        [SerializeField] private TextMeshProUGUI m_PriceTxt, m_OriginalPrice;
        [SerializeField] private List<TextMeshProUGUI> m_PropCountTxtList;
        [SerializeField] private Button m_BtnBuy;
        [SerializeField] private Image m_CoinIcon;
        [SerializeField] private UIGray m_BtnGray;

        private ShopItemType shopItemType;

        public DRShopItem m_DRShopItem;
        private bool clicking = false;
        private const int m_MaxFreePackCount = 10;

        void OnEnable()
        {
            m_BtnBuy?.onClick.AddListener(OnClickBuy);
        }

        void OnDisable()
        {
            m_BtnBuy?.onClick.RemoveListener(OnClickBuy);
        }

        private void OnClickBuy()
        {
            if (clicking) return;

            clicking = true;

            DRItemIAP iap = StoreUtils.GetItemIAP(m_DRShopItem.IapId);

            Reward();
            Debug.Log("购买成功");
        }

        private void Reward()
        {
       

            List<ItemData> rewards = ItemUtils.GetItems(m_DRShopItem.Rewards);

        
        }

        public void UpdateInfo(DRShopItem dRShopItem)
        {
            // m_DRShopItem = dRShopItem;
            //
            // shopItemType = (ShopItemType)dRShopItem.Type;
            //
            // if (shopItemType != ShopItemType.FreePack)
            // {
            //     DRItemIAP iap = StoreUtils.GetItemIAP(m_DRShopItem.IapId);
            //     m_PriceTxt.text = iap.GetPrize();
            //     if (m_OriginalPrice) m_OriginalPrice.text = iap.GetOriginalPrize();
            // }
            //
            // m_BtnBuy.gameObject.SetActive(true);
            //
            // if (m_CoinIcon != null && dRShopItem.Icon != "-1")
            // {
            //     SpriteAtlasManager.Instance.SetSprite(m_CoinIcon, SpriteAtlasId.Item, dRShopItem.Icon);
            // }
            //
            //
            // if (shopItemType == ShopItemType.CoinPack)
            // {
            //     List<ItemData> itemDataList = ItemUtils.GetItems(dRShopItem.Rewards);
            //     for (int i = 0; i < m_PropCountTxtList.Count; i++)
            //     {
            //         m_PropCountTxtList[i].text = $"x{itemDataList[i].num}";
            //     }
            // }
            // else if (shopItemType == ShopItemType.NoAdPack)
            // {
            //     bool canBuy = !UserProxy.HasNoAd && !UserProxy.IsVip;
            //
            //     m_BtnBuy.enabled = canBuy;
            //     m_BtnGray.IsGray = !canBuy;
            //     if (!canBuy)
            //     {
            //         m_PriceTxt.text = GameEntry.Localization.GetString("53");
            //         m_PriceTxt.fontSize = 28;
            //     }
            //
            //     List<ItemData> itemDataList = ItemUtils.GetItems(dRShopItem.Rewards);
            //     if (itemDataList.Count > 0)
            //     {
            //         for (int i = 0; i < m_PropCountTxtList.Count; i++)
            //         {
            //             m_PropCountTxtList[i].text = $"x{itemDataList[i].num}";
            //         }
            //     }
            // }
            // else if (shopItemType == ShopItemType.NoAdSingle)
            // {
            //     bool canBuy = !UserProxy.HasNoAd && !UserProxy.IsVip;
            //
            //     m_BtnBuy.enabled = canBuy;
            //     m_BtnGray.IsGray = !canBuy;
            //     if (!canBuy)
            //     {
            //         m_PriceTxt.text = GameEntry.Localization.GetString("53");
            //         m_PriceTxt.fontSize = 28;
            //     }
            // }
            // else if (shopItemType == ShopItemType.FreePack)
            // {
            //     int count = ProxyManager.UserProxy.GetFreePack(dRShopItem.Id);
            //     bool canBuy = count < m_MaxFreePackCount;
            //     m_BtnBuy.enabled = canBuy;
            //     m_BtnGray.IsGray = !canBuy;
            //
            //     m_PriceTxt.text = $"({m_MaxFreePackCount - count}/{m_MaxFreePackCount})";
            // }
            //
            // if (dRShopItem.Desc != "-1" && m_Desc != null)
            // {
            //     m_Desc.text = dRShopItem.Desc;
            // }
        }
    }
}