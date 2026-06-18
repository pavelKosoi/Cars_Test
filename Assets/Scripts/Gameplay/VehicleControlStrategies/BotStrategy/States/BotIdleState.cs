using UnityEngine;

public class BotIdleState : BotStateBase
{
    float idleTime;
    float timer;

    public BotIdleState(BotControlStrategy controlStrategy) : base(controlStrategy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        idleTime = Random.Range(0f, 0.5f);
        timer = 0;
    }

    public override void Tick()
    {
        if (controlStrategy.FindBestNote() != null)
        {
            controlStrategy.SetState<BotSeekMoneyState>();
            return;
        }

        Vector3 avoidance = controlStrategy.GetTrafficAvoidance(2);
        if (avoidance.sqrMagnitude > 0.01f)
        {
            controlStrategy.SetState<BotWanderState>();
            return;
        }

        if (timer < idleTime)
        {
            timer += Time.deltaTime;
            controlStrategy.CurrentDirection = Vector3.zero;
            return;
        }
        
        if (Random.value < 0.5f)
        {
            controlStrategy.SetState<BotWanderState>();
        }
        else Enter();        
    }
}