using System;
using UnityEngine;

public interface IPoolable 
{
    public event Action OnReturnedToPool;
    public void ReturnToPool();
}
