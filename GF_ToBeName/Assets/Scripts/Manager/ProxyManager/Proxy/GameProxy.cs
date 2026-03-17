using System;
using System.Collections;
using System.Collections.Generic;
using NewSideGame;
using Newtonsoft.Json;
using UnityEngine;

namespace NewSideGame
{
    public partial class GameProxy : BaseProxy<GameModel>
    {
        public GameModel GameModel
        {
            get { return (GameModel)this.Data; }
        }

        public GameProxy(string proxyName, object data = null) : base(proxyName, data)
        {
        }

        public override void OnRegister()
        {
        }

    }
}