using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class ContinuousSpawnerBase<T> : IContinuousSpawner where T : IPoolable
{
    protected CancellationTokenSource cts;
    protected List<T> activeInstances = new();
    protected GamePropertiesConfig gameProperties;

    protected ContinuousSpawnerBase()
    {
        gameProperties = ServiceLocator.Get<GamePropertiesConfig>();
    }

    public IReadOnlyList<T> ActiveInstances => activeInstances;

    public void StartSpawning()
    {
        PerformSpawn().Forget();
    }

    public void StopSpawning()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    async UniTask PerformSpawn()
    {
        cts = new CancellationTokenSource();

        while (cts != null && !cts.IsCancellationRequested)
        {
            if (!CanSpawnBatch())
            {
                await UniTask.Yield();
                continue;
            }

            int toSpawn = GetSpawnCount();

            for (int i = 0; i < toSpawn; i++)
            {
                if (!CanSpawnInstance()) break;

                if (TrySpawnInstance(out T instance))
                {
                    activeInstances.Add(instance);
                }
            }

            float waitFor = GetSpawnDelay();
            await UniTask.Delay(TimeSpan.FromSeconds(waitFor), cancellationToken: cts.Token);
        }
    }


    protected abstract bool CanSpawnBatch();
    protected virtual bool CanSpawnInstance() => CanSpawnBatch();

    protected abstract int GetSpawnCount();
    protected abstract float GetSpawnDelay();

    protected abstract bool TrySpawnInstance(out T instance);

    protected virtual void HandleInstanceReturned(T instance)
    {
        activeInstances.Remove(instance);
    }

    public virtual void Clear()
    {
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            activeInstances[i].ReturnToPool();
        }
    }
}