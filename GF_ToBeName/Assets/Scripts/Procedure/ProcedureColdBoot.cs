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

    public class ProcedureColdBoot<T> : ProcedureBase where T : UGuiForm
    {
        protected bool needCloseLaunch = false;
        protected bool openUISuccess = false;

        protected virtual UIFormType FormType => UIFormType.Undefined;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            needCloseLaunch = procedureOwner.GetData<VarBoolean>(Constant.ProcedureData.ColdBoot).Value;
            GameEntry.UI.OpenUIForm(FormType);
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.UI.CloseUIForm(FormType);
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (needCloseLaunch && openUISuccess)
            {
                LaunchPage.CloseUI();
                SplashPage.CloseUI();
            }
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;

            if (ne.UIForm.Logic is T form)
            {
                openUISuccess = true;
            }
        }
    }
}