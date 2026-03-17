using System;
using UnityEngine;

/// <summary>
/// Contains extension methods related to <see cref="Timer"/>s.
/// </summary>
public static class TimerExtensions
{
    /// <summary>
    /// Attach a timer on to the behaviour. If the behaviour is destroyed before the timer is completed,
    /// e.g. through a scene change, the timer callback will not execute.
    /// </summary>
    /// <param name="behaviour">The behaviour to attach this timer to.</param>
    /// <param name="duration">The duration to wait before the timer fires.</param>
    /// <param name="onComplete">The action to run when the timer elapses.</param>
    /// <param name="onUpdate">A function to call each tick of the timer. Takes the number of seconds elapsed since
    /// the start of the current cycle.</param>
    /// <param name="isLooped">Whether the timer should restart after executing.</param>
    /// <param name="useRealTime">Whether the timer uses real-time(not affected by slow-mo or pausing) or
    /// game-time(affected by time scale changes).</param>
    public static Timer AttachTimer(this MonoBehaviour behaviour, float duration, Action onComplete,
        Action<float> onUpdate = null, bool isLooped = false, bool useRealTime = false)
    {
        return Timer.Register(duration, onComplete, onUpdate, isLooped, useRealTime, behaviour);
    }
    /// <summary>
    /// 针对tunit的定时器，tunit回收的时候会cancel掉所有定时器
    /// </summary>
    public static Timer AttachTimer(this NewSideGame.KBPoolObjectBase behaviour, float duration, Action onComplete,
        Action<float> onUpdate = null, bool isLooped = false, bool useRealTime = false)
    {
        Timer timer = null;
        timer = Timer.Register(duration, onComplete, onUpdate, isLooped, useRealTime, behaviour);
        behaviour.AddTimer(timer);
        return timer;
    }
}
