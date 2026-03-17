using System;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace NewSideGame
{
    public class ProcedureGamePlay : ProcedureColdBoot<UIGamePlayForm>
    {
        protected override UIFormType FormType => UIFormType.UIGamePlayForm;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
        }
    }
}