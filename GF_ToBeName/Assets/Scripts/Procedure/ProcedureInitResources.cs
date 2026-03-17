//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace NewSideGame
{
    public class ProcedureInitResources : ProcedureBase
    {
        private bool m_InitResourcesComplete = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_InitResourcesComplete = false;

            GameEntry.Resource.InitResources(OnInitResourcesComplete);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_InitResourcesComplete)
            {
                return;
            }

            ChangeState<ProcedurePreload>(procedureOwner);
        }

        private void OnInitResourcesComplete()
        {
            m_InitResourcesComplete = true;
        }
    }
}