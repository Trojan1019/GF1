using System;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

using CubeCrush.Manager;

namespace NewSideGame
{
    public class ProcedureGamePlay : ProcedureColdBoot<UIGamePlayForm>
    {
        protected override UIFormType FormType => UIFormType.UIGamePlayForm;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            // 每次进入游戏流程时，根据设置决定是加载存档还是新游戏
            bool loadSave = GameEntry.Setting.GetBool("LoadSavedGame", false);
            GameLoopManager.Instance.StartGame(loadSave);
        }
    }
}