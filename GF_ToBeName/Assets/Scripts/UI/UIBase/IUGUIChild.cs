using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public interface IUGUIChild
    {
        public void OnInit();

        public void OnOpen();

        public void OnUpdate(float elapseSeconds, float realElapseSeconds);

        public void OnClose();
        public void OnSecondUpdate(float time);
    }
}

