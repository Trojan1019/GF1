//------------------------------------------------------------
// File : ProcedureBase.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using System;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace NewSideGame
{

    public abstract class ProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        protected ProcedureOwner ownerState;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            ownerState = procedureOwner;
            Debug.Log("==> lyly ProcedureBase OnEnter type: " + GetType().Name);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Debug.Log("==> lyly ProcedureBase OnLeave type: " + GetType().Name);
        }

        public void SwitchScene(string scenePath, Action<ProcedureOwner> onCompleteAction)
        {
            if (GameEntry.Procedure.CurrentProcedure is ProcedureChangeScene)
            {
                Log.Info("changing scene, please wait...");
                return;
            }

            VarAction<ProcedureOwner> varAction = new VarAction<ProcedureOwner>();
            varAction.Value = onCompleteAction;

            ownerState.SetData<VarString>(Constant.ProcedureData.ScenePath, scenePath);
            ownerState.SetData(Constant.ProcedureData.SceneLoadSuccessAction, varAction);
            ChangeState<ProcedureChangeScene>(ownerState);
        }

        public void SwitchProcedure<T>() where T : ProcedureBase
        {
            ChangeState<T>(ownerState);
        }
    }
}
