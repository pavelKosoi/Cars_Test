using System;
using UnityEngine;

public abstract class GameStateBase : IState, IDisposable
{
    public bool IsFinished { get; protected set; }

    public virtual void Enter()
    {
        IsFinished = false;
    }

    public virtual void Exit() { }    

    public virtual void Tick() { }    
    public virtual void Dispose() { }
}
