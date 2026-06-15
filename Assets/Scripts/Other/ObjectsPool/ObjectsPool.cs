using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

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
                var instance = await Addressables.InstantiateAsync(prefabRef, defaultParent);
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

            if (defaultParent != null)
            {
                instance.transform.SetParent(defaultParent);
            }
        }
    }

    static Dictionary<AssetReferenceGameObject, PoolEntry> entries = new();

    static Dictionary<GameObject, PoolEntry> instanceToEntry = new();

    public static void RegisterEntry(PoolEntry entry)
    {
        if (entries.ContainsKey(entry.prefabRef))
        {
            Debug.LogWarning($"An entry for prefab {entry.prefabRef} is registered already.");
            return;
        }

        entries[entry.prefabRef] = entry;
        entry.Init().Forget();
    }

    public static GameObject GetInstance(AssetReferenceGameObject prefabRef,
        Vector3 onPosition = default, bool setActive = false, bool forceExtend = true)
    {
        if (!entries.TryGetValue(prefabRef, out var entry))
        {
            Debug.LogError($"[ObjectsPool] No entry found for {prefabRef}. Please register it first.");
            return null;
        }

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
                instance.transform.position = onPosition;
                instance.SetActive(setActive);
                return instance;
            }
        }

        if (!forceExtend)
        {
            Debug.LogWarning($"[ObjectsPool] Pool for {prefabRef} is empty and forceExtend is false.");
            return null;
        }

        return entry.Extend(onPosition, setActive);
    }

    public static bool ReturnToPool(GameObject instance)
    {
        if (instance == null) return false;

        if (instanceToEntry.TryGetValue(instance, out var entry))
        {
            entry.ReturnInstance(instance);
            return true;
        }
        else
        {
            Debug.LogWarning($"[ObjectsPool] Cannot return {instance.name} to pool. " +
                $"It doesn't belong to any registered entry.");
            return false;
        }
    }

    public static void Dispose()
    {
        foreach (var entry in entries.Values)
        {
            foreach (var instance in entry.instances)
            {
                if (instance != null)
                {
                    Addressables.ReleaseInstance(instance);
                }
            }
        }

        entries.Clear();
        instanceToEntry.Clear();
    }
}