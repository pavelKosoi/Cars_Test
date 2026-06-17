using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ObjectsPool
{
    public class PoolEntry
    {
        public AssetReferenceGameObject prefabRef;
        public int startAmount;
        public Transform defaultParent;
        public List<GameObject> instances = new();

        public PoolEntry(AssetReferenceGameObject prefabRef, int startAmount, Transform defaultParent)
        {
            this.prefabRef = prefabRef;
            this.startAmount = startAmount;
            this.defaultParent = defaultParent;
        }

        public async UniTask Init()
        {
            for (int i = 0; i < startAmount; i++)
            {
                var instance = await Addressables.InstantiateAsync(prefabRef, defaultParent)
                    .ToUniTask(cancellationToken: GameBootstrapper.GlobalCt);
                SetupInstance(instance, false);
            }
        }

        public GameObject Extend(Vector3 position, bool setActive)
        {
            var newInstance = Addressables.InstantiateAsync(prefabRef, position, Quaternion.identity, defaultParent).WaitForCompletion();
            SetupInstance(newInstance, setActive);
            return newInstance;
        }

        private void SetupInstance(GameObject instance, bool setActive)
        {
            instance.SetActive(setActive);
            instances.Add(instance);
            instanceToEntry[instance] = this;
        }

        public void ReturnInstance(GameObject instance)
        {
            if (instance == null) return;
            instance.SetActive(false);
            if (defaultParent != null) instance.transform.SetParent(defaultParent);
        }
    }

    static Dictionary<AssetReferenceGameObject, PoolEntry> entries = new();
    static Dictionary<GameObject, PoolEntry> instanceToEntry = new();

    static Dictionary<GameObject, CancellationTokenSource> activeDelayTokens = new();

    public static void RegisterEntry(PoolEntry entry)
    {
        if (entries.ContainsKey(entry.prefabRef)) return;
        entries[entry.prefabRef] = entry;
        entry.Init().Forget();
    }

    public static async UniTask RegisterEntryAsync(PoolEntry entry)
    {
        if (entries.ContainsKey(entry.prefabRef)) return;
        entries[entry.prefabRef] = entry;
        await entry.Init();
    }


    public static GameObject GetInstance(AssetReferenceGameObject prefabRef,
        Vector3 onPosition = default, bool setActive = false, bool forceExtend = true)
    {
        if (!entries.TryGetValue(prefabRef, out var entry)) return null;

        for (int i = 0; i < entry.instances.Count; i++)
        {
            var instance = entry.instances[i];
            if (instance == null)
            {
                entry.instances.RemoveAt(i);
                i--;
                continue;
            }

            if (!instance.activeSelf)
            {               
                CancelActiveDelay(instance);

                instance.transform.position = onPosition;
                instance.SetActive(setActive);
                return instance;
            }
        }

        if (!forceExtend) return null;
        return entry.Extend(onPosition, setActive);
    }

    public static void ReturnToPool(GameObject instance, float delaySeconds, CancellationToken externalToken = default)
    {
        if (instance == null) return;

        if (delaySeconds <= 0f)
        {
            ReturnToPool(instance);
            return;
        }

        CancelActiveDelay(instance);

        var internalCts = new CancellationTokenSource();
        activeDelayTokens[instance] = internalCts;

        CancellationToken finalToken;
        CancellationTokenSource linkedCts = null;

        if (externalToken != default)
        {
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalCts.Token);
            finalToken = linkedCts.Token;
        }
        else
        {
            finalToken = internalCts.Token;
        }

        ReturnDelayedAsync(instance, delaySeconds, finalToken, internalCts, linkedCts).Forget();
    }

    static async UniTaskVoid ReturnDelayedAsync(GameObject instance, float delaySeconds,
        CancellationToken token, CancellationTokenSource internalCts, CancellationTokenSource linkedCts)
    {
        bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: token).SuppressCancellationThrow();

        linkedCts?.Dispose();

        if (activeDelayTokens.TryGetValue(instance, out var currentCts) && currentCts == internalCts)
        {
            activeDelayTokens.Remove(instance);
        }
        internalCts.Dispose();

        if (!isCancelled && instance != null && instance.activeSelf)
        {
            ReturnToPool(instance);
        }
    }

    public static bool ReturnToPool(GameObject instance)
    {
        if (instance == null) return false;

        CancelActiveDelay(instance);

        if (instanceToEntry.TryGetValue(instance, out var entry))
        {
            entry.ReturnInstance(instance);
            return true;
        }

        return false;
    }

    static void CancelActiveDelay(GameObject instance)
    {
        if (activeDelayTokens.TryGetValue(instance, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            activeDelayTokens.Remove(instance);
        }
    }

    public static void Dispose()
    {
        foreach (var cts in activeDelayTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        activeDelayTokens.Clear();

        foreach (var entry in entries.Values)
        {
            foreach (var instance in entry.instances)
            {
                if (instance != null) Addressables.ReleaseInstance(instance);
            }
        }

        entries.Clear();
        instanceToEntry.Clear();
    }
}