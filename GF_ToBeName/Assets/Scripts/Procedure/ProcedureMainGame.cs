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

    public class ProcedureMainGame : ProcedureColdBoot<UIHomeForm>
    {
        protected override UIFormType FormType => UIFormType.UIHomeForm;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameManager.Instance.StartGame();
        }

        // protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        // {
        //     base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        //     {
        //         procedureOwner.SetData<VarBoolean>(Constant.ProcedureData.ColdBoot, true);
        //     }
        // }
    }
}