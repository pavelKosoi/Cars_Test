using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameplayState : GameStateBase
{
    int gameplayTime;
    Playground playground;
    CarsManager carsManager;
    MoneyConfig moneyConfig;
    VisualsConfig visualsConfig;
    CompetitorSpawner competitorSpawner;
    List<IContinuousSpawner> coniniousSpawners = new();
    IInputProvider inputProvider;
    MatchScoreModel matchScoreModel;
    AsyncTimer matchTimer;
    ITimerVisual timerVisual;
    GameplayScreen gameplayScreen;

    public GameplayState(int gameplayTime, Playground playground, CarsManager carsManager, MoneyConfig moneyConfig, VisualsConfig visualsConfig)
    {
        this.gameplayTime = gameplayTime;
        this.carsManager = carsManager;
        this.playground = playground;
        this.moneyConfig = moneyConfig;
        this.visualsConfig = visualsConfig;

        matchTimer = new AsyncTimer();
        matchScoreModel = new MatchScoreModel();

        inputProvider = new DragInputProvider(Camera.main, playground);

        var trafficSpawner = new TrafficSpawner(playground, carsManager);
        var notesSpawner = new NotesSpawner(moneyConfig, playground);
        coniniousSpawners.Add(trafficSpawner);
        coniniousSpawners.Add(notesSpawner);

        competitorSpawner = new CompetitorSpawner(playground, carsManager, matchScoreModel);


        var botPerception = new BotPerception(trafficSpawner, notesSpawner, competitorSpawner);
        carsManager.InitVehicleStrategyFactory(new VehicleStrategyFactory(playground, inputProvider, botPerception));

       
        InitCarsPool();       
        InitNotesPool();
        InitVisualsPool();
    }

    void InitCarsPool()
    {
        foreach (var item in carsManager.CarsConfig.Cars)
        {
            int amount = item.CarType == CarsConfig.CarType.Player ? 1
                : GeneralGameManager.Instance.GamePropertiesConfig.MaxSimultaneousNpcs;

            ObjectsPool.RegisterEntry(new ObjectsPool.PoolEntry(item.prefabRef, amount, playground.transform));
        }
    }

    void InitNotesPool()
    {
        foreach (var item in moneyConfig.Notes)
        {
            int amount = GeneralGameManager.Instance.GamePropertiesConfig.MaxSimultaneousNotes;
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
        gameplayScreen = UiScreensManager.Instance.Show<GameplayScreen>();
        gameplayScreen.SetTimerActive(true);
        gameplayScreen.SetContinueButtonActive(false);


        foreach (var spawner in coniniousSpawners)
        {
            spawner.StartSpawning();
        }

        competitorSpawner.SpawnCompetitors();
        gameplayScreen.SetupCounters(competitorSpawner.ActiveProfiles, matchScoreModel).Forget();

        timerVisual = gameplayScreen.TimerVisual;
        matchTimer.OnTick += timerVisual.Tick;
        matchTimer.OnFinished += HandleMatchFinished;

        matchTimer.Start(gameplayTime);
    }

    private void HandleMatchFinished()
    {
        gameplayScreen.SetTimerActive(false);
        gameplayScreen.SetContinueButtonActive(true);

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

                    ObjectsPool.ReturnToPool(confetti, 5f);
                }
            }

            gameplayScreen.ShowWinners(winners);
        }
        else gameplayScreen.SetDrawActive(true);
    }

    public override void Exit()
    {
        base.Exit();

        foreach (var item in coniniousSpawners)
        {
            item.Clear();
        }
        competitorSpawner.Clear();
        gameplayScreen.SetContinueButtonActive(false);
        gameplayScreen.SetDrawActive(false);
        Dispose();
    }
  

    public override void Dispose()
    {
        base.Dispose();        
        matchScoreModel.Clear();
        matchTimer.Dispose();
    }

}

