using System;
using UnityEngine;

public abstract class InputVisualizerBase : MonoBehaviour, IPoolable
{
    protected DragInputProvider dragInput;
    protected Transform carTransform;

    public event Action OnReturnedToPool;

    public virtual void Init(DragInputProvider inputProvider, Transform carTransform)
    {
        dragInput = inputProvider;
        this.carTransform = carTransform;
    }

    public void ReturnToPool()
    {
        ObjectsPool.ReturnToPool(gameObject);
        OnReturnedToPool?.Invoke();
    }
}
