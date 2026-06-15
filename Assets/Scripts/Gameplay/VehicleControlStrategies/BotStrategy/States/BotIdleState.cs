using UnityEngine;

public class BotIdleState : BotStateBase
{
    public BotIdleState(BotControlStrategy controlStrategy) : base(controlStrategy)
    {
    }

    public override void Tick()
    {
        if (controlStrategy.FindBestNote() != null)
        {
            controlStrategy.SetState<BotSeekMoneyState>();            
            return;
        }
     
        if (Random.value < 0.15f)
        {
            controlStrategy.SetState<BotWanderState>();
            return;
        }
      
        Vector3 avoidance = controlStrategy.GetTrafficAvoidance();

        controlStrategy.CurrentDirection = avoidance.sqrMagnitude > 0.001f 
            ? avoidance.normalized : Vector3.zero;
    }
}