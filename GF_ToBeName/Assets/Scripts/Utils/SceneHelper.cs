using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace NewSideGame
{
    public class SceneHelper
    {
        public static void LoadScene(string sceneName, Action onCompleteAction = null)
        {
            var state = GameEntry.Procedure.CurrentProcedure as ProcedureBase;

            TransitionForm.Open(() => { state.SwitchScene(sceneName, (owner) => onCompleteAction?.Invoke()); });
        }

        public static void LoadHomeScene(Action onCompleteAction = null)
        {
            SceneHelper.LoadScene(Constant.Scene.Home, () =>
            {
                var state = GameEntry.Procedure.CurrentProcedure as ProcedureBase;
                state.SwitchProcedure<ProcedureMainGame>();
                onCompleteAction?.Invoke();
            });
        }

        public static void LoadGameScene(Action onCompleteAction = null)
        {
            LoadScene(Constant.Scene.Gameplay, () =>
            {
                var state = GameEntry.Procedure.CurrentProcedure as ProcedureBase;
                state.SwitchProcedure<ProcedureGamePlay>();
                onCompleteAction?.Invoke();
            });
        }

        public static bool IsHomeScene()
        {
            return GameEntry.Procedure.CurrentProcedure is ProcedureMainGame;
        }

        public static bool IsPuzzleScene()
        {
            return GameEntry.Procedure.CurrentProcedure is ProcedureGamePlay;
        }
    }
}