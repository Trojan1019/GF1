using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public interface ICurrencyGroup
    {
        public Transform GetItemTransform(ItemType type);
        public Transform GetItemTransform(int type);
    }
}
