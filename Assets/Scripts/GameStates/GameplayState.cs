using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : GameStateBase
{
    int gameplayTime;
    Playground playground;
    CarsManager carsManager;
    MoneyConfig moneyConfig;
    CompetitorSpawner competitorSpawner;
    List<IContinuousSpawner> coniniousSpawners = new();
    IInputProvider inputProvider;
    MatchScoreModel matchScoreModel;
    AsyncTimer matchTimer;
    ITimerVisual timerVisual;
    GameplayScreen gameplayScreen;

    public GameplayState(int gameplayTime, Playground playground, CarsManager carsManager, MoneyConfig moneyConfig)
    {
        this.gameplayTime = gameplayTime;
        this.carsManager = carsManager;
        this.playground = playground;
        this.moneyConfig = moneyConfig;

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

    void HandleMatchFinished()
    {
        gameplayScreen.SetTimerActive(false);
        gameplayScreen.SetContinueButtonActive(true);

        var winneer = matchScoreModel.GetWinner();
        gameplayScreen.ShowWinner(winneer);

        foreach (var spawner in coniniousSpawners)
        {
            spawner.StopSpawning();
        }

        foreach (var item in competitorSpawner.AllActiveCars)
        {
            item.Stop = true;
        }
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
        Dispose();
    }
  

    public override void Dispose()
    {
        base.Dispose();        
        matchScoreModel.Clear();
        matchTimer.Dispose();
    }

}

