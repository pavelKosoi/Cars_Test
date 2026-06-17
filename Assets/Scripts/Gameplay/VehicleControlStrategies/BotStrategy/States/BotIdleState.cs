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

        if (timer < idleTime)
        {
            timer += Time.deltaTime;
            controlStrategy.CurrentDirection = Vector3.zero;
            return;
        }
     
        if (Random.value < 0.5f)
        {
            controlStrategy.SetState<BotWanderState>();
            return;
        }
        else Enter();
      
        Vector3 avoidance = controlStrategy.GetTrafficAvoidance();

        controlStrategy.CurrentDirection = avoidance.sqrMagnitude > 0.001f 
            ? avoidance.normalized : Vector3.zero;
    }
}