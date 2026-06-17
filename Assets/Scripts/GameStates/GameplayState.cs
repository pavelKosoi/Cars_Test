using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class GameplayState : GameStateBase
{
    int countdownTime;
    int gameplayTime;
    Playground playground;
    CarsManager carsManager;
    MoneyConfig moneyConfig;
    VisualsConfig visualsConfig;
    CompetitorSpawner competitorSpawner;
    List<IContinuousSpawner> coniniousSpawners = new();
    MatchScoreModel matchScoreModel;
    AsyncTimer countdownTimer;
    AsyncTimer matchTimer; 
    GameplayScreen gameplayScreen;
    InputSetupConfig inputConfig;

    (IInputProvider provider, InputVisualizerBase visulizer) input;
    CancellationTokenSource stateCts;

    GamePropertiesConfig gameProperties;

    public GameplayState(int countdownTime, int gameplayTime, Playground playground, CarsManager carsManager,
          MoneyConfig moneyConfig, VisualsConfig visualsConfig, InputSetupConfig inputConfig)
    {
        this.countdownTime = countdownTime;
        this.gameplayTime = gameplayTime;
        this.carsManager = carsManager;
        this.playground = playground;
        this.moneyConfig = moneyConfig;
        this.visualsConfig = visualsConfig;
        this.inputConfig = inputConfig;

        gameProperties = ServiceLocator.Get<GamePropertiesConfig>();

        countdownTimer = new AsyncTimer();
        matchTimer = new AsyncTimer();

        matchScoreModel = new MatchScoreModel();

        input.provider = inputConfig.CreateInput(Camera.main, playground);

        var trafficSpawner = new TrafficSpawner(playground, carsManager);
        var notesSpawner = new NotesSpawner(moneyConfig, playground);
        coniniousSpawners.Add(trafficSpawner);
        coniniousSpawners.Add(notesSpawner);

        competitorSpawner = new CompetitorSpawner(playground, carsManager, matchScoreModel);

        var botPerception = new BotPerception(trafficSpawner, notesSpawner, competitorSpawner);
        carsManager.InitVehicleStrategyFactory(new VehicleStrategyFactory(playground, input.provider, botPerception));

       
        InitCarsPool();       
        InitNotesPool();
        InitVisualsPool();
    }

    void InitCarsPool()
    {
        foreach (var item in carsManager.CarsConfig.Cars)
        {
            int amount = item.CarType == CarsConfig.CarType.Player ? 1
                : gameProperties.MaxSimultaneousNpcs;

            ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(item.prefabRef, amount, playground.transform));
        }
    }

    void InitNotesPool()
    {
        foreach (var item in moneyConfig.Notes)
        {
            int amount = gameProperties.MaxSimultaneousNotes;
            ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(item.prefabRef, amount, playground.transform));            
        }
    }

    void InitVisualsPool()
    {
        ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(visualsConfig.CollisionSparksRef,
            visualsConfig.MaxSimultaneousSparks, playground.transform));

        ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(visualsConfig.SkidmarkLineRef, 
            visualsConfig.MaxSimultaneousSkidmarks, playground.transform));

        ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(visualsConfig.FloatingTextRef,
         visualsConfig.MaxSimultaneousFloatingTexts, playground.transform));

        ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(visualsConfig.WinnerConfettiRef, 1, playground.transform));
    }


    public override void Enter()
    {
        base.Enter();
        stateCts = new CancellationTokenSource();

        competitorSpawner.SpawnCompetitors();

        foreach (var car in competitorSpawner.AllActiveCars)
        {
            car.Stop = true;
        }

        SetUpInputVisual().Forget();
        MatchFlowSequenceAsync(stateCts.Token).Forget();
    }

    async UniTask MatchFlowSequenceAsync(CancellationToken token)
    {
        bool isCancelled = await ProcessCountdownPhaseAsync(token);
        if (isCancelled) return;
        StartGameplayPhase();
    }
    async UniTask<bool> ProcessCountdownPhaseAsync(CancellationToken token)
    {
        var countdownScreen = ServiceLocator.Get<UiScreensManager>().Show<CountdownScreen>();

        countdownScreen.PointOnPlayer(competitorSpawner.PlayerCar.transform, Camera.main);

        countdownTimer.OnTick += countdownScreen.TimerVisual.Tick;
        countdownTimer.Start(countdownTime, true);

        var tcs = new UniTaskCompletionSource();
        Action onFinished = () => tcs.TrySetResult();
        countdownTimer.OnFinished += onFinished;

        bool isCancelled = await tcs.Task.AttachExternalCancellation(token).SuppressCancellationThrow();

        countdownTimer.OnFinished -= onFinished;
        countdownTimer.OnTick -= countdownScreen.TimerVisual.Tick;

        return isCancelled;
    }
    void StartGameplayPhase()
    {
        gameplayScreen = ServiceLocator.Get<UiScreensManager>().Show<GameplayScreen>();
        gameplayScreen.SetTimerActive(true);
        gameplayScreen.SetContinueButtonActive(false);
        gameplayScreen.SetupCounters(competitorSpawner.ActiveProfiles, matchScoreModel).Forget();

        foreach (var car in competitorSpawner.AllActiveCars)
        {
            car.Stop = false;
        }

        foreach (var spawner in coniniousSpawners)
        {
            spawner.StartSpawning();
        }

        var timerVisual = gameplayScreen.TimerVisual;
        matchTimer.OnTick += timerVisual.Tick;
        matchTimer.OnFinished += HandleMatchFinished;

        matchTimer.Start(gameplayTime);
    }


    async UniTask SetUpInputVisual()
    {
        var visualizer = await inputConfig.SetupVisuals(input.provider, 
            competitorSpawner.PlayerCar, input.visulizer != null);
        input.visulizer = visualizer;
    }

    void HandleMatchFinished()
    {
        gameplayScreen.SetTimerActive(false);
        gameplayScreen.SetContinueButtonActive(true);
        if (input.visulizer != null) input.visulizer.ReturnToPool();

        input.provider.Dispose();

        foreach (var spawner in coniniousSpawners)
        {
            spawner.StopSpawning();
        }

        foreach (var item in competitorSpawner.AllActiveCars)
        {
            item.Stop = true;
        }

        if (matchScoreModel.TryGetWinners(out var winners))
        {
            foreach (var winner in winners)
            {               
                if (winner is CarController winnerCar)
                {
                    var confetti = ObjectsPool.GetInstance(visualsConfig.WinnerConfettiRef, 
                        winnerCar.transform.position, setActive: true);
                    confetti.transform.SetParent(winnerCar.transform);

                    winnerCar.OnReturnedToPool += ()=> ObjectsPool.ReturnToPool(confetti);

                    ObjectsPool.ReturnToPool(confetti, 5f);
                }
            }

            if (winners.Count == 1) ServiceLocator.Get<CameraManager>().SwitchCamera
                    (CameraManager.CameraType.Winner, new CameraManager.CameraContext(
                        2f, new Vector3(0, 2, -2), (winners[0] as Component).transform));

            gameplayScreen.ShowWinners(winners);
        }
        else gameplayScreen.SetDrawActive(true);
    }

    public override void Exit()
    {
        base.Exit();

        stateCts?.Cancel();

        foreach (var item in coniniousSpawners)
        {
            item.Clear();
        }

        competitorSpawner.Clear();
        matchScoreModel.Dispose();

        if (gameplayScreen != null)
        {
            gameplayScreen.SetContinueButtonActive(false);
            gameplayScreen.SetDrawActive(false);
        }

        ServiceLocator.Get<CameraManager>().SwitchCamera
                   (CameraManager.CameraType.Main, new CameraManager.CameraContext(0f));

        countdownTimer.Stop();
        matchTimer.Stop();
    }


    public override void Dispose()
    {
        base.Dispose();
        matchScoreModel.Dispose();
        matchTimer.Dispose();
        countdownTimer.Dispose();
        stateCts?.Dispose();
    }
}

