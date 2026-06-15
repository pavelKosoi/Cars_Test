using UnityEngine;

public abstract class TimerVisualBase : ITimerVisual
{
    public abstract void Tick(int remainTime);
}
