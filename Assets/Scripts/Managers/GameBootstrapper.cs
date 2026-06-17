using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour, IStateSwitcher<GameStateBase>
{
    public static CancellationToken GlobalCt;

    [Header("Configs")]
    [SerializeField] GamePropertiesConfig gamePropertiesConfig;
    [SerializeField] CarsConfig carsConfig;
    [SerializeField] MoneyConfig moneysConfig;
    [SerializeField] InputSetupConfig inputConfig;
    [SerializeField] VisualsConfig visualsConfig;

    [Header("Scene References")]
    [SerializeField] UiScreensManager uiScreensManager;
    [SerializeField] Playground playground;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PlayerFeedbackVisualizer playerFeedbackVisualizer;

    StateMachine stateMachine;
    MathCollisionManager collisionManager;



    private void Awake()
    {
        GlobalCt = this.GetCancellationTokenOnDestroy();

        ServiceLocator.Clear();

        ServiceLocator.Register(gamePropertiesConfig);
        ServiceLocator.Register(visualsConfig);
        ServiceLocator.Register(cameraManager);
        ServiceLocator.Register(playground);
        ServiceLocator.Register(playerFeedbackVisualizer);
        ServiceLocator.Register(uiScreensManager);
        ServiceLocator.Register<IStateSwitcher<GameStateBase>>(this);

        var carsManager = new CarsManager(carsConfig, playground);
        ServiceLocator.Register(carsManager); 

        collisionManager = new MathCollisionManager();
        ServiceLocator.Register(collisionManager);


        stateMachine = new StateMachine();

        stateMachine.RegisterState(new MenuState());
        stateMachine.RegisterState(new GameplayState(gamePropertiesConfig.CountdownDuration, gamePropertiesConfig.PlaytimeDuration,
            playground, carsManager, moneysConfig, visualsConfig, inputConfig));

    }

    private void Start()
    {
        SetState<MenuState>();
    }

   public void SetState<T>() where T : GameStateBase => stateMachine.ChangeState<T>();
   

    private void Update()
    {
        stateMachine?.Update();
        collisionManager?.Update();
    }

    private void OnDestroy()
    {        
        stateMachine.Dispose();
        ObjectsPool.Dispose();
        ServiceLocator.Clear();
    }

  
}
