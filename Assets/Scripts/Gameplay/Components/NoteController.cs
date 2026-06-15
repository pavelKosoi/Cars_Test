using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static MoneyConfig;

[RequireComponent(typeof(MathCollider))]
public class NoteController : MonoBehaviour, IPoolable
{
    Note note;

    public event Action OnReturnedToPool;
    public int Denomination => note.denomination;

    public void Init(Note note)
    {
        this.note = note;
    }

    public async UniTask ReturnToPoolDelay(float delay)
    {
        var token = this.GetCancellationTokenOnDestroy();
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (ObjectsPool.ReturnToPool(gameObject))
        {
            OnReturnedToPool?.Invoke();
            OnReturnedToPool = null;           
        }
    }
}
