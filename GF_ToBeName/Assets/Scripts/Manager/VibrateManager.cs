//------------------------------------------------------------
// File : VibrateManager.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 震动管理
//------------------------------------------------------------
using UnityEngine;
using UnityGameFramework.Runtime;
using NewSideGame;

public class VibrateManager : Singleton<VibrateManager>
{
    /// <summary>
    /// Unity 提供的单次震动时长
    /// </summary>
    public float VibrateOnceTime = 0.015f;
    public float VibrateInterval = 0;

    public int androidAmplitude = 80;
    public int iOSAmplitude = 0;

    private bool isPlaying = false;
    private float timer = 0;  //定时器事件
    private float totalTime = 0;


    /// <summary>
    /// 循环播放震动
    /// </summary>
    public void PlayVibrateLoop()
    {
        isPlaying = true;
        timer = 0;
        totalTime = float.MaxValue;
        VibrateInterval = 0.1f;
        // Log.Info("PlayVibrateLoopPlayVibrateLoop");
    }

    public void PlayOnce()
    {
        isPlaying = true;
        timer = 0;
        totalTime = VibrateOnceTime;
        VibrateInterval = 0;

        androidAmplitude = 40;
        iOSAmplitude = 0;
    }

    public void PlayOnceMerge()
    {
        isPlaying = true;
        timer = 0;
        totalTime = VibrateOnceTime;
        VibrateInterval = 0;

        androidAmplitude = 80;
        iOSAmplitude = 0;
    }

    public void PlayOnceStrength()
    {
        isPlaying = true;
        timer = 0;
        totalTime = VibrateOnceTime;
        VibrateInterval = 0;

        androidAmplitude = 140;
        iOSAmplitude = 1;
    }

    public void PlayOnceIfNotPlaying()
    {
        if (isPlaying)
        {
            return;
        }
        PlayOnce();
    }


    public void StopVibrate()
    {
        isPlaying = false;
        timer = 0;
        totalTime = 0;
        VibrateInterval = 0;
    }


    public void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (timer > .0f)
        {
            timer -= Time.deltaTime;
            //Log.Info("Time.deltaTime=" + Time.deltaTime);
        }
        else
        {
            timer = VibrateOnceTime + VibrateInterval;
            totalTime -= VibrateOnceTime + VibrateInterval;
            if (totalTime < 0)
            {
                isPlaying = false;
            }
            else
            {
                NativeSample.Vibrate((int)(VibrateOnceTime * 1000), androidAmplitude);
                // Log.Info("====================Vibrate=======================" + timer);
            }

        }
    }

}

