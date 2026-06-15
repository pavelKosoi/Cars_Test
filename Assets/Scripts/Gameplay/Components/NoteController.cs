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

   
    public void ReturnToPool()
    {
        if (ObjectsPool.ReturnToPool(gameObject))
        {
            OnReturnedToPool?.Invoke();
            OnReturnedToPool = null;           
        }
    }
}
