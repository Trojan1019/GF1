using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameFramework;
using TMPro;

namespace NewSideGame
{
    public class EffectManager : Singleton<EffectManager>
    {
        // public void PlayClearEffect(List<GridCell> cells)
        // {
        //     foreach (var cell in cells)
        //     {
        //         if (cell == null) continue;
        //         cell.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);
        //         cell.image.DOFade(0, 0.2f).OnComplete(() =>
        //         {
        //             cell.image.DOFade(1, 0.1f);
        //             cell.SetState(false, Color.gray);
        //         });
        //     }
        // }
    }
}