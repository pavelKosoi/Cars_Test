using UnityEngine;

public class BotWanderState : BotStateBase
{
    public BotWanderState(BotControlStrategy controlStrategy) : base(controlStrategy)
    {
    }

    public override void Enter()
    {
        controlStrategy.WanderTarget = controlStrategy.Bounds.GetRandomPointInside(0.8f);
    }
 
    public override void Tick()
    {
        if (controlStrategy.FindBestNote() != null)
        {
            controlStrategy.SetState<BotSeekMoneyState>();
            return;
        }

        Vector3 botPos = controlStrategy.Car.transform.position;

        if ((controlStrategy.WanderTarget - botPos).sqrMagnitude < 2f)
        {
            if (Random.value > 0.5f) controlStrategy.SetState<BotIdleState>();
            else Enter();

            return;
        }

        Vector3 toTarget = (controlStrategy.WanderTarget - botPos).normalized;
        Vector3 avoidance = controlStrategy.GetTrafficAvoidance() + controlStrategy.GetBotSeparation();

        controlStrategy.CurrentDirection = (toTarget + avoidance).normalized;
    }
}