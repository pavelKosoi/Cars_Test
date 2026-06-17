using UnityEngine;

public class BotControlStrategy : VehicleControlStrategyBase, IStateSwitcher<BotStateBase>
{
    StateMachine stateMachine;

    public readonly GamePropertiesConfig GamePropertiesConfig;
    public readonly IPlaygroundBounds PlaygroundBounds;

    public IBotPerception Perception { get; private set; }
    public CarController Car => carController;

    public NoteController CurrentTarget { get; set; }
    public Vector3 WanderTarget { get; set; }
    public Vector3 CurrentDirection { get; set; }

    public BotControlStrategy(CarController carController, IBotPerception perception, 
        IPlaygroundBounds playgroundBounds) : base(carController)
    {
        this.Perception = perception;
        GamePropertiesConfig = ServiceLocator.Get<GamePropertiesConfig>();
        PlaygroundBounds = playgroundBounds;

        stateMachine = new StateMachine();

        stateMachine.RegisterState(new BotIdleState(this));
        stateMachine.RegisterState(new BotWanderState(this));
        stateMachine.RegisterState(new BotSeekMoneyState(this));

    }

    public override void Reset()
    {
        CurrentTarget = null;
        CurrentDirection = Vector3.zero;

        if (FindBestNote() != null) stateMachine.ChangeState<BotSeekMoneyState>();
        else stateMachine.ChangeState<BotWanderState>();

    }

    public override Vector3 GetMoveDirection()
    {    
        stateMachine.Update();

        return CurrentDirection;
    }

   public void SetState<T>() where T : BotStateBase => stateMachine.ChangeState<T>();



    public NoteController FindBestNote()
    {
        NoteController bestNote = null;
        float bestScore = -1f;
        Vector3 botPos = carController.transform.position;

        foreach (var note in Perception.VisibleNotes)
        {
            if (!note.gameObject.activeInHierarchy) continue;

            float distSqr = (botPos - note.transform.position).sqrMagnitude;
            if (distSqr < 0.1f) distSqr = 0.1f;

            float baseScore = note.Denomination / distSqr;
            float randomFactor = Random.Range(1f - GamePropertiesConfig.BotTargetRandomness, 1f + GamePropertiesConfig.BotTargetRandomness);
            float finalScore = baseScore * randomFactor;

            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                bestNote = note;
            }
        }
        return bestNote;
    }

    public Vector3 GetTrafficAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 botPos = carController.transform.position;
        float avoidRadSqr = GamePropertiesConfig.BotTrafficAvoidanceRadius * GamePropertiesConfig.BotTrafficAvoidanceRadius;

        foreach (var traffic in Perception.VisibleTraffic)
        {
            Vector3 toTraffic = traffic.transform.position - botPos;
            float distSqr = toTraffic.sqrMagnitude;

            if (distSqr < avoidRadSqr && distSqr > 0.01f)
            {
                float repulsionWeight = 1f - (distSqr / avoidRadSqr);
                avoidance -= toTraffic.normalized * (repulsionWeight * GamePropertiesConfig.BotTrafficEvasionStrength);
            }
        }
        return avoidance;
    }

    public Vector3 GetBotSeparation()
    {
        Vector3 separation = Vector3.zero;
        if (Perception.VisibleBots == null) return separation;

        Vector3 botPos = carController.transform.position;
        float sepRadSqr = GamePropertiesConfig.BotSeparationRadius * GamePropertiesConfig.BotSeparationRadius;

        foreach (var otherBot in Perception.VisibleBots)
        {
            if (otherBot == carController) continue;

            Vector3 toBot = otherBot.transform.position - botPos;
            float distSqr = toBot.sqrMagnitude;

            if (distSqr < sepRadSqr && distSqr > 0.01f)
            {
                float repulsionWeight = 1f - (distSqr / sepRadSqr);
                separation -= toBot.normalized * (repulsionWeight * GamePropertiesConfig.BotSeparationStrength);
            }
        }
        return separation;
    }

   
}