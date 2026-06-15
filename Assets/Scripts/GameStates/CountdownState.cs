using System;
using UnityEngine;

public class CountdownState : GameStateBase
{
    int countdownDuration;
    ITimerVisual timerVisual;
    AsyncTimer timer;

    public CountdownState(int countdownDuration)
    {
        this.countdownDuration = countdownDuration;
        timer = new AsyncTimer();
    }

    public override void Enter()
    {
        base.Enter();
        var screen = UiScreensManager.Instance.Show<CountdownScreen>();
        timerVisual = screen.TimerVisual;

        timer.OnTick += timerVisual.Tick;
        timer.OnFinished += HandleCountdownFinished;

        timer.Start(countdownDuration, true);
    }

    private void HandleCountdownFinished()
    {
        IsFinished = true;
        GeneralGameManager.Instance.SetState<GameplayState>();
    }

    public override void Exit()
    {
        base.Exit();
        timer.OnTick -= timerVisual.Tick;
        timer.OnFinished -= HandleCountdownFinished;

        timer.Stop();
    }

    public override void Dispose()
    {
        timer.Dispose();
    }
}