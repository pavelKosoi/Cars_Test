using UnityEngine;

public class BotSeekMoneyState : BotStateBase
{
    public BotSeekMoneyState(BotControlStrategy controlStrategy ) : base(controlStrategy)
    {
    }

    public override void Enter()
    {
        controlStrategy.CurrentTarget = controlStrategy.FindBestNote();
    }
  
    public override void Tick()
    {
        if (controlStrategy.CurrentTarget == null ||
           !controlStrategy.CurrentTarget.gameObject.activeInHierarchy)          
        {
            controlStrategy.CurrentTarget = controlStrategy.FindBestNote();

            if (controlStrategy.CurrentTarget == null)
            {
                if (Random.value > 0.5f) controlStrategy.SetState<BotWanderState>();
                else controlStrategy.SetState<BotIdleState>();
                return;
            }
        }

        Vector3 botPos = controlStrategy.Car.transform.position;
        Vector3 toTarget = (controlStrategy.CurrentTarget.transform.position - botPos).normalized;
        Vector3 avoidance = controlStrategy.GetTrafficAvoidance() + controlStrategy.GetBotSeparation();

        controlStrategy.CurrentDirection = (toTarget + avoidance).normalized;
    }
}