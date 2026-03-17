using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace NewSideGame
{
    public class RewardItem : MonoBehaviour
    {
        public TextMeshProUGUI m_Desc;

        public Image m_ItemIcon;

        public void UpdateInfo(ItemData data)
        {
            m_Desc.text = "x" + data.num.ToString();

            if (data.id != (int)ItemType.Coin)
                data.SetSprite(m_ItemIcon);
            else
            {
                m_ItemIcon.sprite = SpriteAtlasManager.Instance.GetSprite(SpriteAtlasId.Item, "Coin_2");
            }
        }

    }
}