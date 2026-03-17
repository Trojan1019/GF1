// //------------------------------------------------------------
// // File : FlyElementTarget.cs
// // Email: mailto:zhiqiang.yang
// // Desc : 飞行到目标接受的地方
// //------------------------------------------------------------
//
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using DG.Tweening;
// using MEC;
//
// namespace NewSideGame
// {
//     public class FlyElementTarget : MonoBehaviour
//     {
//         private float CD = 1.5f;
//         private float lastTimstemp;
//
//         public int effectId = 31013;
//
//         public void Punch(int itemType, int itemNum, bool withEff = true, bool consume = false)
//         {
//             Sequence seq = DOTween.Sequence();
//             seq.Append(transform.DOScale(Vector3.one * 1.2f, 0.1f));
//             seq.Append(transform.DOScale(Vector3.one, 0.1f));
//
//
//             if (withEff)
//             {
//                 if (Time.timeSinceLevelLoad - lastTimstemp < CD)
//                     return;
//                 lastTimstemp = Time.timeSinceLevelLoad;
//
//                 EffectManager.Instance.FlyEffect(effectId).AttachUIVfx(2, false, gameObject.layer).AttachSpawnPosition(transform.position).AttachAutoDestory(1f);
//
//                 MEC.Timing.RunCoroutineSingleton(OnPlaySounds(itemType, itemNum, consume), gameObject, MEC.SingletonBehavior.Abort);
//             }
//         }
//
//         private IEnumerator<float> OnPlaySounds(int itemType, int itemNum, bool consume = false)
//         {
//             if (consume)
//             {
//                 int soundId = 2001;
//                 int playNum = itemNum;
//
//                 while (playNum > 0)
//                 {
//                     playNum -= 1;
//                     GameEntry.Sound.PlaySound(soundId);
//                     yield return Timing.WaitForSeconds(0.1f);
//                 }
//             }
//             else
//             {
//                 int soundId = 2001;
//                 GameEntry.Sound.PlaySound(soundId);
//                 yield return Timing.WaitForSeconds(1f);
//             }
//         }
//     }
// }
