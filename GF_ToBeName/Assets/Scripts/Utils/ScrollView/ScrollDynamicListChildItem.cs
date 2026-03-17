using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSideGame
{
    public abstract class ScrollDynamicListChildItem<T> : MonoBehaviour where T : ScrollDymaicChildData
    {
        public abstract void SetData(T data);
    }
}