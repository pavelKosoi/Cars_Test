using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class AsyncTimer : IDisposable
{
    public event Action<int> OnTick;
    public event Action OnFinished;

    public int CurrentTime { get; private set; }

    CancellationTokenSource cts;

    public void Start(int duration, bool delayAfterZero = false)
    {
        Stop();

        CurrentTime = duration;
        cts = new CancellationTokenSource();

        RunTimerAsync(cts.Token, delayAfterZero).Forget();
    }

    public void Stop()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    async UniTask RunTimerAsync(CancellationToken token, bool delayAfterZero)
    {
        OnTick?.Invoke(CurrentTime);

        while (CurrentTime > 0)
        {
            bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;

            CurrentTime--;
            OnTick?.Invoke(CurrentTime);
        }

        if (delayAfterZero)
        {
            bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;
        }

        OnFinished?.Invoke();
    }

    public void Dispose()
    {
        Stop();
        OnTick = null;
        OnFinished = null;
    }
}