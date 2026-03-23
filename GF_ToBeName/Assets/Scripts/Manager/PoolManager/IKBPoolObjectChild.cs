using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewSideGame
{
    public interface IKBPoolObjectChild
    {
        public void OnInit(PoolManager ppm);

        public void OnSpawn(PoolManager ppm);

        public void OnDeSpawn(PoolManager ppm);

        public void OnUpdate(float delta);
    }
}