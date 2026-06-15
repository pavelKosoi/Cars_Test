using System;
using UnityEngine;

public class GeneralGameManager : MonoBehaviour
{
    public static GeneralGameManager Instance;

    [SerializeField] GamePropertiesConfig gamePropertiesConfig;
    [SerializeField] CarsConfig carsConfig;
    [SerializeField] MoneyConfig moneysConfig;
    [SerializeField] Playground playground;

    StateMachine stateMachine;

    public GamePropertiesConfig GamePropertiesConfig => gamePropertiesConfig;
    public Playground Playground => playground;
    public CarsManager CarsManager { get; private set; }
    public MathCollisionManager collisionManager { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        stateMachine = new StateMachine();

        CarsManager = new CarsManager(carsConfig, playground);
        collisionManager = new MathCollisionManager();

        stateMachine.RegisterState(new MenuState());
        stateMachine.RegisterState(new CountdownState(gamePropertiesConfig.CountdownDuration));
        stateMachine.RegisterState(new GameplayState(gamePropertiesConfig.PlaytimeDuration, playground, CarsManager, moneysConfig));
    }

    private void Start()
    {
        SetState<MenuState>();
    }

    public void SetState<T>() where T : GameStateBase => stateMachine.ChangeState<T>();
    public T GetState<T>() where T : GameStateBase => stateMachine.GetState<T>();
   

    private void Update()
    {
        stateMachine?.Update();
        collisionManager?.Update();
    }

    private void OnDestroy()
    {
        stateMachine.Dispose();
        ObjectsPool.Dispose();
    }
}
