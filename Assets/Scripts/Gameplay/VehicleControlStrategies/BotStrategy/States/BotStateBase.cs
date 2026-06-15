using UnityEngine;

public abstract class BotStateBase : IState
{
    protected BotControlStrategy controlStrategy;    

    protected BotStateBase(BotControlStrategy controlStrategy)
    {
        this.controlStrategy = controlStrategy;        
    }

    public virtual void Dispose() { }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Tick() { }
}
